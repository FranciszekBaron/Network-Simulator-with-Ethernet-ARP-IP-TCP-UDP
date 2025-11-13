public abstract class Device
{
    public string Name { get; set; }


    // KERNEL SPACE
    public List<Network> ConnectedNetwork { get; set; }
    public RoutingTable RoutingTable { get; set; }
    
    public Device(string name)
    {
        Name = name;
        RoutingTable = new RoutingTable();
    }
    
    public abstract void ReceiveFrame(EthernetFrame frame,NetworkInterface networkInterface);
    protected abstract void HandleIP(byte[] payload,NetworkInterface networkInterface);
    protected abstract void HandleARP(byte[] payload, NetworkInterface networkInterface);

    
    protected virtual bool IsItMyMAC(byte[] mac, NetworkInterface networkInterface)
    {
        string receivedMAC = ConvertionManager.MACtoString(mac);

        string myMAC = ConvertionManager.MACtoString(networkInterface.MacAdress);

        if (receivedMAC != myMAC)
        {
            return false;
        }
        return true;
    }
    public static bool IsBroadcast(byte[] mac)
    {
        int count = 0;
        for (int i = 0; i < 6; i++)
        {
            if (mac[i] == 0xFF)
            {
                count++;
            }
        }

        if (count == mac.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}