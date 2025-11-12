public class NetworkInterface
{
    public string  Name { get; set; }
    public byte[] IpAdress { get; set; } = new byte[4];
    public byte[] MacAdress { get; set; } = new byte[6];
    
    public byte[] Mask { get; set; }


    public NetworkInterface(string name, byte[] ipAdress, byte[] macAdress, byte[] mask)
    {
        this.Name = name;
        this.IpAdress = ipAdress;
        this.MacAdress = macAdress;
        this.Mask = mask;

    }
}