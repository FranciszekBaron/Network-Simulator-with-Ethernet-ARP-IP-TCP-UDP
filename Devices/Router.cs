public class Router : Host
{
    public List<Network> interfaces { get; set; }

    public Dictionary<string, string> FIB;

    public Router(string Name, byte[] MacAdress, byte[] IpAdress) : base(Name, MacAdress, IpAdress)
    {
        
    }
}