namespace MapleATS.Network.Logic.Protocol.Json
{
    public class ConnectProtocol : BaseProtocol
    {
        public override ProtocolSelect ProtocolSelector { get; set; } = ProtocolSelect.ConnectProtocol;
        public override int Command { get; set; }
        public override int HostId { get; set; }
        public string Guid { get; set; }
        public string Data { get; set; }
    }
}
