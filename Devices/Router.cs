public class Router : Host
{
    public List<Network> interfaces { get; set; }

    public Dictionary<string, string> FIB;

    public Router(string Name, byte[] MacAdress, byte[] IpAdress) : base(Name, MacAdress, IpAdress)
    {

    }

    protected override void HandleIP(byte[] payload)
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




    }

    public override void ReceiveFrame(EthernetFrame ethernetFrame)
    {

        //Czy moj MAC??

        if (!IsItMyMAC(ethernetFrame.DestinationMAC) && !IsBroadcast(ethernetFrame.DestinationMAC))
        {
            LoggingManager.PrintWarning($"Given MAC ({ethernetFrame.DestinationMAC}) not in local network, not a Broadcast either");
            return;
        }

        //Jaki EthernetType - taka operacja

        if (ethernetFrame.EtherType == NetworkConstants.ETHERTYPE_ARP)
        {
            HandleARP(ethernetFrame.Payload);
        }
        else if (ethernetFrame.EtherType == NetworkConstants.ETHERTYPE_IP)
        {
            HandleIP(ethernetFrame.Payload);
        }
        else
        {
            LoggingManager.PrintWarning("No such EtherType available");
        }

    }

    
}