


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using MapleATS.Network.Logic;
using MapleATS.Network.Logic.Protocol.Direct;
using MapleATS.Network.Logic.Protocol.Json;
using MapleATS.Network.Util;
using MapleATS.Util;


namespace MapleATS.Network
{
    public class Client
    {
        private Socket clientSocket;

        private string serverUri;
        private int serverPort;
        private string guid;

        private Proxy rpcProxy;
        private Stub stub;


        private Connect _ConLogic;


        private Queue<byte[]> protocolQueue = new Queue<byte[]>();
        public Client()
        {
            guid = "289bddf4-4576-4666-aa11-66c85e28c445";
            serverPort = 3000;
            serverUri = "192.168.0.25";
            rpcProxy = new Proxy();
            rpcProxy.SetClient(this);
            stub = new Stub();


            _ConLogic = new Connect(this);

            stub.Receive_Connect = _ConLogic.ReceiveConnect;
            stub.Receive_RoleRegistration = _ConLogic.ReceiveRoleRegister;
        }


        #region 설정 메서드

        /// <summary>
        /// 서버 URI 설정
        /// </summary>
        public void SetServerUri(string serverUri)
        {
            this.serverUri = serverUri;
        }

        /// <summary>
        /// 서버 포트 설정
        /// </summary>
        public void SetServerPort(int serverPort)
        {
            this.serverPort = serverPort;
        }

        /// <summary>
        /// 클라이언트 GUID 설정
        /// </summary>
        public void SetGuid(string guid)
        {
            this.guid = guid;
        }

        #endregion


        /// <summary>
        /// 비동기로 서버에 연결하고, 성공 또는 실패 콜백을 실행합니다.
        /// </summary>
        /// <param name="onSuccess">성공 시 콜백</param>
        /// <param name="onFailure">실패 시 콜백</param>
        public void ConnectToServerAsync(Action onSuccess, Action<Exception> onFailure)
        {
            try
            {
                // 서버 IP 파싱 후 소켓 연결
                IPAddress serverIP = IPAddress.Parse(serverUri);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(serverIP, serverPort));
                clientSocket.SendBufferSize = 4096;
                clientSocket.ReceiveBufferSize = 4096;

                // 비동기 수신 이벤트 설정
                SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
                receiveEventArgs.Completed += ReceiveCompleted;
                receiveEventArgs.SetBuffer(new byte[4096], 0, 4096);

                StartReceive(receiveEventArgs);

                // 연결 성공 처리
                ConnectSuccess();
                onSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                TeruTeruLogger.LogError($"서버 연결 실패: {ex.Message}");
                onFailure?.Invoke(ex);
            }
        }

        /// <summary>
        /// 연결 성공 시 처리 로직.
        /// 프로토콜 처리 시작 및 서버에 연결 요청을 보냅니다.
        /// </summary>
        private void ConnectSuccess()
        {
            StartProtocolProcessor();

        }

        /// <summary>
        /// 비동기 수신 시작.
        /// </summary>
        private void StartReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            if (!clientSocket.ReceiveAsync(receiveEventArgs))
            {
                // 동기적으로 완료되었을 경우 바로 처리
                ProcessReceive(receiveEventArgs);
            }
        }

        /// <summary>
        /// 수신 완료 시 콜백.
        /// 데이터를 수신하고, 오류가 없는지 확인합니다.
        /// </summary>
        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                ProcessReceive(e);
                StartReceive(e); // 다시 수신 대기 시작
            }
            else
            {
                TeruTeruLogger.LogError($"수신 오류: {e.SocketError}");
            }
        }

        /// <summary>
        /// 수신된 데이터를 처리하고 큐에 저장합니다.
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // 수정
            byte[] receivedData = new byte[e.BytesTransferred];
            Array.Copy(e.Buffer, receivedData, e.BytesTransferred);
            protocolQueue.Enqueue(receivedData);


        }

        /// <summary>
        /// 프로토콜 처리 스레드를 시작합니다.
        /// </summary>
        public void StartProtocolProcessor()
        {
            Thread protocolThread = new Thread(ProtocolProcessor);
            protocolThread.IsBackground = true;
            protocolThread.Start();
        }

        /// <summary>
        /// 수신된 메시지를 처리하는 프로토콜 처리 루프.
        /// </summary>
        private void ProtocolProcessor()
        {


            while (true)
            {
                Thread.Sleep(16); // 0.016초 대기

                if (protocolQueue.TryDequeue(out byte[] item))
                {
                    var sendType = item[0];
                    var protocolType = item[1];

                    byte[] data = new byte[item.Length - 2];
                    Array.Copy(item, 2, data, 0, data.Length);

                    if (SendType.Direct == (SendType)sendType)
                    {
                        // 직접 처리
                        ProcessDirect(data, (MethodsSelector)protocolType);
                    }
                    else if (SendType.Json == (SendType)sendType)
                    {
                        // JSON 처리
                        ProcessJson(Encoding.ASCII.GetString(data), (ProtocolSelect)protocolType);
                    }
                }
            }
        }

        private void ProcessDirect(byte[] data, MethodsSelector methodId)
        {
            switch (methodId)
            {
                case MethodsSelector.RequestConnection:

                    string JsonString = Encoding.UTF8.GetString(data);

                    ConnectProtocol connectProtocol;

                    connectProtocol = System.Text.Json.JsonSerializer.Deserialize<ConnectProtocol>(JsonString);

                    if ((connectProtocol != null))
                    {
                        TeruTeruLogger.LogInfo($"host ID : {connectProtocol.HostId}");
                        TeruTeruLogger.LogInfo($"GuID : {connectProtocol.Guid}");
                        TeruTeruLogger.LogInfo($"data : {connectProtocol.Data}");
                    }
                    // 어텐션 레벨로 로그 출력
                    TeruTeruLogger.LogAttention($"받은 데이터 : {JsonString}");

                    this.stub.Receive_Connect?.Invoke(connectProtocol.Command, connectProtocol.HostId, connectProtocol.Guid, connectProtocol.Data);

                    break;
                case MethodsSelector.RequestRegisterRole:
                    // 역활 등록의 응답
                    SendData sendData = MarshalUtil.Deserialize<SendData>(data);


                    int size = Marshal.SizeOf(sendData);

                    if (size == 0)
                    {
                        TeruTeruLogger.LogError("역할 등록 응답 데이터 크기가 0입니다.");
                        return;
                    }
                    this.stub.Receive_RoleRegistration?.Invoke(sendData);

                    break;
                default:
                    TeruTeruLogger.LogError($"알 수 없는 메서드 ID: {methodId}");
                    break;
            }
        }

        private void ProcessJson(string json, ProtocolSelect protocolType)
        {
            switch (protocolType)
            {
                case ProtocolSelect.ConnectProtocol:
                    break;
                case ProtocolSelect.LoginProtocol:
                    break;
                default:
                    TeruTeruLogger.LogError($"알 수 없는 프로토콜 ID: {protocolType}");
                    break;
            }
        }


        private async Task SendProtocolData(string jsonData)
        {
            // JSON 데이터를 바이트 배열로 인코딩
            byte[] jsonDataBytes = Encoding.ASCII.GetBytes(jsonData + "\n");

            byte[] sendData = jsonDataBytes;

            try
            {
                // 데이터를 서버로 전송
                await clientSocket.SendAsync(new ReadOnlyMemory<byte>(sendData), SocketFlags.None);
                // 전송 성공 처리
            }
            catch (Exception e)
            {
                // 전송 실패 처리
                TeruTeruLogger.LogError($"데이터 전송 실패: {e.Message}");
            }
        }
        private async Task SendProtocolData(byte[] sendData)
        {
            try
            {
                // 데이터를 서버로 전송
                await clientSocket.SendAsync(new ReadOnlyMemory<byte>(sendData), SocketFlags.None);
                // 전송 성공 처리
            }
            catch (Exception e)
            {
                // 전송 실패 처리
                TeruTeruLogger.LogError($"데이터 전송 실패: {e.Message}");
            }
        }

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);


        public async Task Sender(string jsonData)
        {
            if (!clientSocket.Connected)
            {
                return;
            }

            // 비동기 잠금 시작
            await semaphore.WaitAsync();

            try
            {
                // 데이터를 서버로 전송
                await SendProtocolData(jsonData);
            }
            finally
            {
                // 잠금 해제
                semaphore.Release();
            }
        }
        public async Task Sender(byte[] data)
        {
            if (!clientSocket.Connected)
            {
                return;
            }

            // 비동기 잠금 시작
            await semaphore.WaitAsync();

            try
            {
                // 데이터를 서버로 전송
                await SendProtocolData(data);
            }
            finally
            {
                // 잠금 해제
                semaphore.Release();
            }
        }





        #region 2025.03.20 Request Method

        public void Request_Connect()
        {
            rpcProxy.RequestConnection(guid);

        }

        public void Request_Resister(string role, string clientName)
        {
            rpcProxy.RequestRegisterRole(role, clientName);
        }

        public void Send_Image(byte[] image_Byte)
        {
            rpcProxy.SendImage(NetworkMemory.Instance.HostID, NetworkMemory.Instance.GameID, image_Byte);
        }
        #endregion



    }
}