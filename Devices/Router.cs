using System.Net;

public class Router : Device
{
    

    // public Dictionary<string, string> FIB;

    public List<NetworkInterface> Interfaces { get; set; }

    public Dictionary<string,byte[]> arpCache { get; set; }

    public Router(string Name) : base(Name)
    {
        Interfaces = new List<NetworkInterface>();
    }
    

    public NetworkInterface AddInterface(string name,byte[] IpAdress,byte[] MacAdress,byte[] mask)
    {
        var iface = new NetworkInterface(name, IpAdress, MacAdress, mask);
        Interfaces.Add(iface);

        byte[] network = CalculateNetwork(IpAdress, mask);
        RoutingTable.AddRoute(new Route(network, mask, null, iface,"Auto (Add-Interface)"));
        return iface;
    }


    //Na kazdym Hopie zmienia się SourceMAC,DestMAC oraz TTL
    protected override void HandleIP(byte[] payload, NetworkInterface networkInterface)
    {
        //Deserialize packet
        IPPacket packet = IPPacket.Deserialize(payload);

        //Packet dla mnie -- odpowiedz i zakończ
        string targetIP = ConvertionManager.IPtoString(packet.DestinationIP);
        NetworkInterface incomingInterface = Interfaces.FirstOrDefault(
            e => ConvertionManager.IPtoString(e.IpAdress) == targetIP
        );

        if (incomingInterface != null)
        {
            LoggingManager.PrintPositive($"Packet Received from {ConvertionManager.IPtoString(incomingInterface.IpAdress)}");
            return;
        } 

        //Check TTL -- zabezpieczenianie przed petlami routingowymi 
        if (packet.TTL == 0)
        {
            LoggingManager.PrintWarning($"[{Name}] TTL expired, drop packet");
            return;
        }
        packet.TTL--;


        // Sprawdź następny Hop
        LoggingManager.PrintNormal("========= HOST A Routing Table =========");
        LoggingManager.PrintNormal(RoutingTable.ToString());
        LoggingManager.PrintNormal("========================================");
        byte[] nextHop = RoutingTable.GetNextHop(packet.DestinationIP);

        if (nextHop == null)
        {
            LoggingManager.PrintWarning($"[{Name}] No next hop found for {ConvertionManager.IPtoString(packet.DestinationIP)}");
            return;
        }
        
        Route route = RoutingTable.routes.FirstOrDefault(e => e.Gateaway == nextHop || e.Destination == nextHop);

        if (route == null)
        {
            LoggingManager.PrintWarning($"[{Name}] No route to host {ConvertionManager.IPtoString(packet.DestinationIP)}");
            return;
        }

        //Sprawdz outgoing interface , który ma być uzyty
        NetworkInterface outgoingInterface = Interfaces.FirstOrDefault(e => e.Name == route.Interface.Name);

        if(outgoingInterface == null)
        {
            LoggingManager.PrintWarning($"No outgoing interface in Route: {route}");
            return;
        }

        //Arp lookup na nastepny MAC
        byte[] nextHopMAC;
        string nextHopToString = ConvertionManager.IPtoString(nextHop);
        if (outgoingInterface.arpCache.ContainsKey(nextHopToString))
        {
            nextHopMAC = outgoingInterface.arpCache[ConvertionManager.IPtoString(nextHop)];
            LoggingManager.PrintNormal($"[{Name}] MAC from ARP CACHE is:" + ConvertionManager.MACtoString(nextHopMAC));
        }
        else
        {
            //Nie ma IP <-> MAC w ARP? Zrób ARP Request
            LoggingManager.PrintNormal($"Wykonuje ARP Request do ... {ConvertionManager.IPtoString(nextHop)}");
            SendArpRequest(nextHop, outgoingInterface);
            if (!outgoingInterface.arpCache.ContainsKey(nextHopToString))
                throw new Exception("Could not Broadcast or Find next MAC");
            nextHopMAC = outgoingInterface.arpCache[nextHopToString];
        }

    }


    
    public virtual void SendFrame(EthernetFrame ethernetFrame, Network network)
    {
        
        if (IsBroadcast(ethernetFrame.DestinationMAC)) //Broadcast - do wszystkich 
        {
            network.Broadcast(this, ethernetFrame);
        }
        else //Unicast - do konkretnego MAC
        {
            network.Unicast(ethernetFrame);
        }
    }

    protected override void HandleARP(byte[] payload, NetworkInterface networkInterface)
    {
        //Deserialize packet
        AdressResolutionProtocol arpRequest = AdressResolutionProtocol.Deserialize(payload);

        string targetIP = ConvertionManager.IPtoString(arpRequest.TargetIPAdress);
        string thisIP = ConvertionManager.IPtoString(networkInterface.IpAdress);



        byte[] targetMAC = null;

        Console.WriteLine(ConvertionManager.IPtoString(networkInterface.IpAdress));


        //Znalezienie Hosta/Routera do którego wysyłamy
        if (targetIP == thisIP && arpRequest.Opcode == NetworkConstants.ARP_REQUEST)
        {

            LoggingManager.PrintDevice($"[{ConvertionManager.IPtoString(networkInterface.IpAdress)}] answers: {ConvertionManager.IPtoString(arpRequest.TargetIPAdress)} is mine");

            //tworzenie nowej ramki ArpReply
            targetMAC = networkInterface.MacAdress;
            AdressResolutionProtocol arpReply = new AdressResolutionProtocol(2, targetMAC, networkInterface.IpAdress, arpRequest.SenderMACAdress, arpRequest.SenderIPAdress);
            byte[] arpReplyBytes = AdressResolutionProtocol.Serialize(arpReply);
            Console.WriteLine(BitConverter.ToString(arpRequest.SenderMACAdress).Replace("-", ":"));

            //wyślij zapakowana w ramke Ethernet
            SendFrame(new EthernetFrame(arpRequest.SenderMACAdress, targetMAC, NetworkConstants.ETHERTYPE_ARP, arpReplyBytes), networkInterface.ConnectedNetwork);
        }

        //Nasza odpowiedź na to co Host/Router przesyła
        else if (targetIP == thisIP && arpRequest.Opcode == NetworkConstants.ARP_REPLY)
        {
            LoggingManager.PrintPositive($"[{Name}] Otrzymałem odpowiedz");

            //tworzenie nowej ramki ArpReply
            AdressResolutionProtocol arpReply = AdressResolutionProtocol.Deserialize(payload);
            targetMAC = arpReply.SenderMACAdress;

            SaveToArpCache(arpReply.SenderIPAdress, targetMAC,networkInterface);
        }
        else
        {
            LoggingManager.PrintNormal($"[{Name}] answers: {ConvertionManager.IPtoString(arpRequest.TargetIPAdress)} is not mine");
        }
    }

    public void SaveToArpCache(byte[] IpAdress, byte[] MacAdress,NetworkInterface networkInterface)
    {
        if (IpAdress.Length != NetworkConstants.IP_ADRESS_LENGHT)
        {
            LoggingManager.PrintPositive($"Adres IP: {IpAdress} ma niepoprwną długość");
            return;
        }

        if (MacAdress.Length != NetworkConstants.MAC_ADRESS_LENGHT)
        {
            LoggingManager.PrintPositive($"Adres MAC: {MacAdress} ma niepoprwną długość");
            return;
        }

        networkInterface.arpCache.Add(ConvertionManager.IPtoString(IpAdress), MacAdress);

        //Print dodanej pary IP <-> MAC
        foreach (var pair in networkInterface.arpCache)
        {
            if (pair.Key == ConvertionManager.IPtoString(IpAdress))
            {
                LoggingManager.PrintPositive($"Zapisuje w ARP CACHE  IP: {pair.Key} -> MAC: {BitConverter.ToString(pair.Value).Replace("-", ":")}");
            }
        }
    }


    public override string ToString()
    {
        return Name;
    }
}