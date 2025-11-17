public class Route
{
    public byte[] Destination { get; set; }
    public byte[] Netmask { get; set; }
    public byte[] Gateaway { get; set; }
    public NetworkInterface Interface { get; set; }

    private string From { get; set; }

    public Route(byte[] destination, byte[] netmask, byte[] gateaway, NetworkInterface @interface, string from)
    {
        Destination = destination ?? new byte[] { 0, 0, 0, 0 };
        Netmask = netmask ?? new byte[] { 0, 0, 0, 0 };
        Gateaway = gateaway ?? new byte[] { 0, 0, 0, 0 };
        Interface = @interface ?? throw new ArgumentNullException(nameof(Interface));
        From = from;
    }
    
    public Route(byte[] destination, byte[] netmask, byte[] gateaway, NetworkInterface @interface)
    {
        Destination = destination ?? new byte[] { 0, 0, 0, 0 };
        Netmask = netmask ?? new byte[] { 0, 0, 0, 0 };
        Gateaway = gateaway ?? new byte[] { 0, 0, 0, 0 };
        Interface = @interface ?? throw new ArgumentNullException(nameof(Interface));
        From = "User";
    }


    public bool Matches(byte[] targetIP)
    {
        //sprawdzi czy target ip jest w tej samej sieci co route
        for (int i = 0; i < 4; i++)
        {
            //czy ta sama sieć?
            if ((targetIP[i] & Netmask[i]) != (Destination[i] & Netmask[i]))
            {
                //nie są w tej samej sieci
                return false;
            }
        }
        //są w tej samej sieci
        return true;
    }


    public byte CountPrefix()
    {
        byte count = 0;
        for (int i = 0; i < 4; i++)
        {
            byte current = Netmask[i];

            for (int bit = 7; bit > 0; bit--)
            {
                if ((current & (1 << bit)) != 0)
                {
                    count++;
                }
                else
                {
                    return count;
                }
            }
        }
        return count;
    }

    public override string ToString()
    {
        return string.Format("{0,-16}{1,-16}{2,-16}{3,-10}{4,-16}",
            ConvertionManager.IPtoString(Destination),
            ConvertionManager.IPtoString(Netmask),
            ConvertionManager.IPtoString(Gateaway),
            Interface?.Name ?? "null",
            From);
    }
}