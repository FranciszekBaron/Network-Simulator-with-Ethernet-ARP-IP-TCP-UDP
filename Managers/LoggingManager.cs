public class LoggingManager
{
    public static void PrintFrameWithDetails(EthernetFrame ethernetFrame)
    {


        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine("==== ETHERNET =====");
        Console.WriteLine("\t" + ethernetFrame);
        Console.ResetColor();

        switch (ethernetFrame.EtherType)
        {
            case 0x800:
                IPPacket iPPacket = IPPacket.Deserialize(ethernetFrame.Payload);

                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("==== IP Packet =====");
                Console.WriteLine("\t" + iPPacket);
                Console.ResetColor();
                break;
            case 0x806:
                AdressResolutionProtocol arp = AdressResolutionProtocol.Deserialize(ethernetFrame.Payload);

                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("==== ARP =====");
                Console.WriteLine("\t" + arp);
                Console.ResetColor();
                break;
        }

        
    }


    public static void PrintWarning(string toPrint)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(toPrint);
        Console.ResetColor();
    }


    public static void PrintNormal(string toPrint)
    {
        Console.WriteLine(toPrint);
        Console.ResetColor();
    }

    public static void PrintPositive(string toPrint)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(toPrint);
        Console.ResetColor();
    }
}