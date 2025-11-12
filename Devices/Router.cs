public class Router : Device
{
    // public List<Network> interfaces { get; set; }

    public Dictionary<string, string> FIB;

    public int MyProperty { get; set; }

    public List<NetworkInterface> interfaces { get; set; }

    

    public Router(string Name, byte[] MacAdress, byte[] IpAdress) : base(Name)
    {

    }

    protected override void HandleIP(byte[] payload,NetworkInterface networkInterface)
    {
        //Deserialize packet
        IPPacket packet = IPPacket.Deserialize(payload);



        //Check TTL -- zabezpieczenianie przed petlami routingowymi 
        if (packet.TTL == 0)
        {
            LoggingManager.PrintWarning($"[{Name}] TTL expired, drop packet");
            return;
        }
        packet.TTL--;

        // byte[] nextHop = routingTable.GetNextHop(packet.DestinationIP);


    }

    public override void ReceiveFrame(EthernetFrame frame,NetworkInterface networkInterface)
    {
        throw new NotImplementedException();
    }

    protected override void HandleARP(byte[] payload,NetworkInterface networkInterface)
    {
        throw new NotImplementedException();
    }
}