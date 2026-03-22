
using MapleATS.Network.Logic.Protocol.Direct;
using MapleATS.Network.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MapleATS.Util;


namespace MapleATS.Network.Logic
{
    public class Connect
    {
        Client main_Cleint;


        public Connect(Client main_Cleint)
        {
            this.main_Cleint = main_Cleint;


        }




        /// <summary>
        /// 서버로부터 연결을 받았을 때 호출되는 메서드
        /// </summary>
        /// <param name="command"></param>
        /// <param name="hostID"></param>
        /// <param name="Guid"></param>
        /// <param name="data"></param>
        public void ReceiveConnect(int command, int hostID, string Guid, string data)
        {

            TeruTeruLogger.LogInfo($"받은 명령 : {command}");

            TeruTeruLogger.LogInfo($"할당된 호스트 아이디 : {hostID}");

            TeruTeruLogger.LogInfo($"할당된 임시 게임 아이디 : {data}");


            NetworkMemory.Instance.HostID = hostID;
            NetworkMemory.Instance.GameID = data;
            NetworkMemory.Instance.IsLogin = true;

            TeruTeruLogger.LogInfo("서버에 역활 등록");
            main_Cleint.Request_Resister("KeyBoard", "dotge");
        }
        public void ReceiveRoleRegister(SendData data)
        {

            var hostID = data.index;
            var stringData = Encoding.UTF8.GetString(data.data);


            // 부분문자열 검사
            if (stringData.Contains("Success"))
            {
                TeruTeruLogger.LogInfo($"역할 등록 성공 : {hostID}");
                NetworkMemory.Instance.IsRoleRegister = true;
            }
            else
            {
                TeruTeruLogger.LogError($"역할 등록 실패 : {hostID}");
            }

        }
    }
}