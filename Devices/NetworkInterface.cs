public class NetworkInterface
{
    public string  Name { get; set; }
    public byte[] IpAdress { get; set; } = new byte[4];
    public byte[] MacAdress { get; set; }
    public byte[] Mask { get; set; }

    // Status flags
    public bool IsUp { get; set; } = true;
    public string State => IsUp ? "UP" : "DOWN";


    public Dictionary<string, byte[]> ArpCache { get; set; }
    public Network ConnectedNetwork { get; set; }
    public NetworkInterface(string name, byte[] ipAdress, byte[] macAdress, byte[] mask)
    {
        Name = name;
        IpAdress = ipAdress;
        MacAdress = macAdress;
        Mask = mask;
        ArpCache = new Dictionary<string, byte[]>();
    }

    public override string ToString()
    {
        return $"{Name} {ConvertionManager.MACtoString(MacAdress)} {ConvertionManager.IPtoString(IpAdress)} {ConvertionManager.IPtoString(Mask)}";
    }


}