public class Route
{
    public byte[] Destination { get; set; }
    public byte[] Netmask { get; set; }
    public byte[] Gateaway { get; set; }
    public Network Interface { get; set; }

    public Route(byte[] Destination, byte[] Netmask, byte[] Gateaway, Network Interface)
    {
        this.Destination = Destination ?? new byte[] { 0, 0, 0, 0 };
        this.Netmask = Netmask ?? new byte[] { 0, 0, 0, 0 };
        this.Gateaway = Gateaway ?? new byte[] { 0, 0, 0, 0 };
        this.Interface = Interface ?? throw new ArgumentNullException(nameof(Interface));
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
        return string.Format("{0,-16}{1,-16}{2,-16}{3,-10}",
            ConvertionManager.IPtoString(Destination),
            ConvertionManager.IPtoString(Netmask),
            ConvertionManager.IPtoString(Gateaway),
            Interface?.ToString() ?? "null");
    }
}