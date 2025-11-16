using System.Net.NetworkInformation;

public class Network
{
    public List<(Device device, NetworkInterface iface)> ConnectedDevices { get; set; }

    public string Name { get; set; }

    public byte[] Netmask = { 255, 255, 255, 0 };

    public Network(string name)
    {
        this.Name = name;
    }

    public void ConnectDevice(Device device,NetworkInterface iface)
    {
        if (ConnectedDevices == null)
        {
            ConnectedDevices = new List<(Device,NetworkInterface)>();
        }
        ConnectedDevices.Add((device,iface));
        device.ConnectedNetwork.Add(this);
    }


    public void Broadcast(Device sender, EthernetFrame ethernetFrame)
    {
        LoggingManager.PrintNormal("\n" + "Wykonuje Broadcast...." + "\n");
        LoggingManager.PrintFrameWithDetails(ethernetFrame);

        string senderMAC = ConvertionManager.MACtoString(ethernetFrame.SourceMAC);

        foreach (var (device,iface) in ConnectedDevices)
        {
            string ifaceMAC = ConvertionManager.MACtoString(iface.MacAdress);

            // Nie wysyłaj do siebie samego
            if (ifaceMAC == senderMAC)
                continue;
                
            LoggingManager.PrintNormal($"  → Delivering to {device.Name} ({iface.Name})");
            device.ReceiveFrame(ethernetFrame, iface);
        }
    }


    public void Unicast(EthernetFrame ethernetFrame)
    {
        Device target = null;
        NetworkInterface upcomingInterface = null;
        LoggingManager.PrintFrameWithDetails(ethernetFrame);


        string targetMAC = ConvertionManager.MACtoString(ethernetFrame.DestinationMAC);

        foreach (var (device, iface) in ConnectedDevices)
        {
            string deviceMAC = ConvertionManager.MACtoString(iface.MacAdress);
            if (deviceMAC == targetMAC)
            {
                target = device;
                upcomingInterface = iface;
            }
        }

        if (target == null)
        {
            throw new Exception("Unknown Host Error");
        }
        
        if(upcomingInterface == null)
        {
            throw new Exception("Upcoming Interface Error");
        }

        LoggingManager.PrintNormal("\n" + $"Wykonuje Unicast do {BitConverter.ToString(ethernetFrame.DestinationMAC).Replace("-", ":")}..." + "\n");
        target.ReceiveFrame(ethernetFrame,upcomingInterface);
    }

    public override string ToString()
    {
        return Name;
    }
}