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
    protected abstract void HandleARP(byte[] payload,NetworkInterface networkInterface);
}