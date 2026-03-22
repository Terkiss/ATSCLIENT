namespace MapleATS.Network.Logic.Protocol.Direct
{
    public enum MethodsSelector : byte
    {
        RequestConnection = 0,
        RequestRegisterRole = 1,
        SendImage = 2,
        GeneratePlayer = 3,
        SendChatData = 4
    }

    public static class MethodsSelectorUtil
    {
        public static byte GetMethodId(MethodsSelector selector)
        {
            return (byte)selector;
        }
    }
}