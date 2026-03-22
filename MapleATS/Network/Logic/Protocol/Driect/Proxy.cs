using MapleATS.Network.Util;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MapleATS.Network.Logic.Protocol.Direct
{
    public class Proxy
    {
        private Client _client;
        public Proxy()
        {

        }
        public void SetClient(Client client)
        {
            _client = client;
        }

        public void RequestConnection(string guid)
        {
            ConnectionData data = new ConnectionData();
            data.guid = guid;

            byte[] requestBytes = MarshalUtil.Serialize(data);
            SendRequestAndGetResponse(requestBytes, MethodsSelector.RequestConnection); // 0은 연결 요청 메서드 ID
        }

        public void RequestRegisterRole(string role, string clientName)
        {
            NetworkMemory.Instance.ClientRole = role;
            SendData data = new SendData();
            data.index = 0;

            var gameID = NetworkMemory.Instance.GameID;
            if (gameID != null)
            {
                string roledata = $"{gameID}!!!{role}!!!{clientName}!!!";
                var roledata_Byte = Encoding.UTF8.GetBytes(roledata);
                byte[] bytes = new byte[256];



                for (int i = 0; i < roledata_Byte.Length; i++)
                {
                    bytes[i] = roledata_Byte[i];
                }
                // 남은영역은 -1로채운다
                for (int i = roledata_Byte.Length + 1; i < 256; i++)
                {
                    bytes[i] = 0;
                }

                data.data = bytes;
                byte[] requestBytes = MarshalUtil.Serialize(data);
                SendRequestAndGetResponse(requestBytes, MethodsSelector.RequestRegisterRole);
            }
        }


        public void SendImage(int hostID, string gameID, byte[] data)
        {
            SendImageData imageData = new SendImageData();

            byte[] gameID_byte = new byte[256];
            byte[] sendImg = new byte[2097152];

            imageData.hostID = hostID;

            var temp_gameID_Byte = Encoding.UTF8.GetBytes(gameID);
            for (int i = 0; i < gameID_byte.Length; i++)
            {
                if (i < temp_gameID_Byte.Length)
                {
                    gameID_byte[i] = temp_gameID_Byte[i];
                }
                else
                {
                    gameID_byte[i] = 0;
                }
            }




            for (int i = 0; i < sendImg.Length; i++)
            {
                if (i < data.Length)
                {
                    sendImg[i] = data[i];
                }
                else
                {
                    sendImg[i] = 0;
                }
            }
            imageData.imgSize = data.Length;
            imageData.data = sendImg;
            imageData.userID = gameID_byte;


            byte[] requestBytes = MarshalUtil.Serialize(imageData);

            SendRequestAndGetResponse(requestBytes, MethodsSelector.SendImage);
        }



        public void RequestPlayerGen(float pos_x, float pos_y, float pos_z,
            float rot_x, float rot_y, float rot_z, float rot_w, int[] animation
            , bool gender, int[] skin)
        {
            PlayerData data = new PlayerData();
            data.PositionX = pos_x;
            data.PositionY = pos_y;
            data.PositionZ = pos_z;
            data.RotationX = rot_x;
            data.RotationY = rot_y;
            data.RotationZ = rot_z;
            data.RotationW = rot_w;
            data.Gender = gender;
            data.AnimationState = animation;
            data.SkinData = skin;

            data.Index = NetworkMemory.Instance.HostID;

            byte[] requestBytes = MarshalUtil.Serialize(data);


            SendRequestAndGetResponse(requestBytes, MethodsSelector.GeneratePlayer); // 3은 플레이어 생성 요청 메서드 ID
        }





        public void SendChatMessage(string message)
        {
            ChatData data = new ChatData();

            data.index = NetworkMemory.Instance.HostID;
            data.sender = NetworkMemory.Instance.HostID.ToString();
            data.message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));

            byte[] requestBytes = MarshalUtil.Serialize(data);
            SendRequestAndGetResponse(requestBytes, MethodsSelector.SendChatData);
        }


        // 공통 요청 처리 및 응답 수신
        private async void SendRequestAndGetResponse(byte[] requestBytes, MethodsSelector methodId)
        {
            // 메서드 ID와 함께 요청 전송
            byte[] fullRequest = new byte[2 + (requestBytes?.Length ?? 0)];
            fullRequest[0] = (byte)SendType.Direct;
            fullRequest[1] = (byte)methodId; // 첫 번째 바이트에 메서드 ID 삽입
            if (requestBytes != null)
            {
                // 2번쨰 원소 이후에 요청 데이터 복사
                //             Array.Copy(jsonDataBytes, 0, sendData, 2, jsonDataBytes.Length);
                Array.Copy(requestBytes, 0, fullRequest, 2, requestBytes.Length);
            }

            await _client.Sender(fullRequest);
        }

        // 응답이 필요 없는 요청
        private void SendRequest(byte[] requestBytes, int methodId)
        {
            using (Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // clientSocket.Connect(_serverIp, _serverPort);

                byte[] fullRequest = new byte[1 + requestBytes.Length];
                fullRequest[0] = (byte)methodId;
                Array.Copy(requestBytes, 0, fullRequest, 1, requestBytes.Length);

                clientSocket.Send(fullRequest);
            }
        }
    }
}