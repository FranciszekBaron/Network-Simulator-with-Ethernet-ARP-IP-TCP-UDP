public class Network
{
    public List<Host> hosts { get; set; }

    public byte[] Netmask = { 255, 255, 255, 0 };

    public Network(){}

    public void Connect(Host host)
    {
        if (hosts == null)
        {
            hosts = new List<Host>();
        }
        hosts.Add(host);
        host.ConnectedNetwork.Add(this);
    }


    public void Broadcast(Host sender, EthernetFrame ethernetFrame)
    {
        LoggingManager.PrintNormal("\n" + "Wykonuje Broadcast...." + "\n");
        LoggingManager.PrintFrameWithDetails(ethernetFrame);
        foreach (var device in this.hosts)
        {
            if (device != sender)
            {
                device.ReceiveFrame(ethernetFrame);
            }
        }
    }
    

    public void Unicast(EthernetFrame ethernetFrame)
    {
        Host target = null;
        LoggingManager.PrintFrameWithDetails(ethernetFrame);


        string targetMAC = ConvertionManager.MACtoString(ethernetFrame.DestinationMAC);

        foreach (var device in hosts)
        {
            string deviceMAC = ConvertionManager.MACtoString(device.MacAdress);
            if (deviceMAC == targetMAC)
            {
                target = device;
            }
        }

        if (target == null)
        {
            throw new Exception("Unknown Host Error");
        }


        LoggingManager.PrintNormal("\n" + $"Wykonuje Unicast do {BitConverter.ToString(ethernetFrame.DestinationMAC).Replace("-",":")}..." + "\n");
        target.ReceiveFrame(ethernetFrame);
    }
}