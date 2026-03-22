using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleATS.Network.Util
{
    public class NetworkMemory
    {
        private static NetworkMemory instance = new NetworkMemory();
        public static NetworkMemory Instance
        {
            get { return instance; }
        }



        // public ConcurrentQueue<AtsImage> atsImages = new ConcurrentQueue<AtsImage>();
        #region Infomation
        private int hostID;
        private string gameID;
        private string clientRole;

        private bool isLogin;
        private bool isRoleRegister;

        public int HostID
        {
            get { return hostID; }
            set { hostID = value; }
        }

        public string GameID
        {
            get { return gameID; }
            set { gameID = value; }

        }
        public string ClientRole
        {
            get { return clientRole; }
            set { clientRole = value; }
        }

        public bool IsLogin
        {
            get { return isLogin; }
            set { isLogin = value; }
        }

        public bool IsRoleRegister
        {
            get { return isRoleRegister; }
            set { isRoleRegister = value; }
        }

        #endregion
    }
}