

using MapleATS.Network.Logic.Protocol.Direct;




namespace MapleATS.Network.Logic.Protocol.Direct
{
    public class Stub
    {
        public Stub()
        {

        }
        public System.Action<int, int, string, string> Receive_Connect;

        public System.Action<SendData> Receive_RoleRegistration;

        public System.Action<SendImageData> Receive_ImageData;






        public System.Action<PlayerData> Receive_PlayerGen;

        public System.Action<PlayerData> Receive_LocationUpdate;

        public System.Action<ChatData> Receive_ChatData;

        public System.Action<SendData> Receive_NotifyPlayerExit;
    }
}