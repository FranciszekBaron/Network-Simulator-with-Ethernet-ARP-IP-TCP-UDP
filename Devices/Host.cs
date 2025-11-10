using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

public class Host
{

    public string Name { get; set; }
    public byte[] IpAdress { get; set; }
    public byte[] MacAdress { get; set; }

    


    // KERNEL SPACE
    public List<Network> ConnectedNetwork { get; set; }
    public RoutingTable routingTable { get; set; }


    //ARP CACHE 
    public Dictionary<string,byte[]> arpCache { get; set; }


    public Host(string Name, byte[] MacAdress, byte[] IpAdress)
    {
        this.Name = Name;
        this.IpAdress = IpAdress;
        this.MacAdress = MacAdress;
        ConnectedNetwork = new List<Network>();
        routingTable = new RoutingTable();

        arpCache = new Dictionary<string, byte[]>();
    }
    
    public virtual void SendPacket(byte[] IPAdrress,byte[] data)
    {
        byte[] nextHop = routingTable.GetNextHop(IPAdrress);

        byte[] nextHopMAC;

        string nextHopToString = ConvertionManager.IPtoString(nextHop);


        LoggingManager.PrintNormal($"HEJ to [{Name}] i moj MacAdress to: {ConvertionManager.MACtoString(MacAdress)}");

        if (arpCache.ContainsKey(nextHopToString))
        {
            nextHopMAC = arpCache[ConvertionManager.IPtoString(nextHop)];
            LoggingManager.PrintNormal($"[{Name}] MAC from ARP CACHE is:" + ConvertionManager.MACtoString(nextHopMAC));
        }
        else
        {
            //Nie ma IP <-> MAC w ARP? Zrób ARP Request
            LoggingManager.PrintNormal($"Wykonuje ARP Request do ... {ConvertionManager.IPtoString(nextHop)}");
            SendArpRequest(nextHop);

            nextHopMAC = arpCache[nextHopToString];
        }

        IPPacket ipPacket = new IPPacket(
            sourceIP: this.IpAdress,
            destinationIP: IPAdrress,
            protocol: 1,
            payload: data
        );

        byte[] ipBytes = IPPacket.Serialize(ipPacket);

        EthernetFrame ethernetFrame = new EthernetFrame(
            destinationMAC: nextHopMAC,
            sourceMAC: this.MacAdress,
            0x800,
            payload: ipBytes);

        LoggingManager.PrintNormal($"[{Name}] Created Ethernet Frame: {ConvertionManager.MACtoString(ethernetFrame.SourceMAC)} → {ConvertionManager.MACtoString(ethernetFrame.DestinationMAC)}");

        LoggingManager.PrintPositive("Packet send");
        SendFrame(ethernetFrame, ConnectedNetwork[0]);

    }

    public virtual void ReceiveFrame(EthernetFrame ethernetFrame)
    {
    
        //Czy moj MAC??

        if (!IsItMyMAC(ethernetFrame.DestinationMAC) && !IsBroadcast(ethernetFrame.DestinationMAC))
        {
            LoggingManager.PrintWarning($"Given MAC ({ethernetFrame.DestinationMAC}) not in local network, not a Broadcast either");
            return;
        }

        //Jaki EthernetType - taka operacja
        
        if (ethernetFrame.EtherType == NetworkConstants.ETHERTYPE_ARP)
        {
            HandleARP(ethernetFrame.Payload);
        }
        else if (ethernetFrame.EtherType == NetworkConstants.ETHERTYPE_IP)
        {
            HandleIP(ethernetFrame.Payload);
        }
        else
        {
            LoggingManager.PrintWarning("No such EtherType available");
        }

    }
    

    public virtual void SendFrame(EthernetFrame ethernetFrame, Network network)
    {
        if (this.ConnectedNetwork.Contains(network))
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
        else
        {
            throw new Exception("Nie podłączono do sieci");
        }
    }


    public void SendArpRequest(byte[] targetIP)
    {
        AdressResolutionProtocol arp = new AdressResolutionProtocol(1, MacAdress, IpAdress, [0, 0, 0, 0, 0, 0], targetIP);
        byte[] arpBytes = AdressResolutionProtocol.Serialize(arp);
        EthernetFrame sendedEthernetFrame = new EthernetFrame([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF], MacAdress, 0x806, arpBytes);

        SendFrame(sendedEthernetFrame, ConnectedNetwork[0]);
    }


    protected virtual void HandleARP(byte[] payload)
    {

        AdressResolutionProtocol arpRequest = AdressResolutionProtocol.Deserialize(payload);

        string targetIP = ConvertionManager.IPtoString(arpRequest.TargetIPAdress);
        string thisIP = ConvertionManager.IPtoString(IpAdress);



        byte[] targetMAC = null;

        Console.WriteLine(ConvertionManager.IPtoString(IpAdress));


        //Znalezienie Hosta/Routera do którego wysyłamy
        if (targetIP == thisIP && arpRequest.Opcode == NetworkConstants.ARP_REQUEST)
        {

            LoggingManager.PrintWarning($"[{Name}] answers: {ConvertionManager.IPtoString(arpRequest.TargetIPAdress)} is mine");

            //tworzenie nowej ramki ArpReply
            targetMAC = MacAdress;
            AdressResolutionProtocol arpReply = new AdressResolutionProtocol(2, targetMAC, IpAdress, arpRequest.SenderMACAdress, arpRequest.SenderIPAdress);
            byte[] arpReplyBytes = AdressResolutionProtocol.Serialize(arpReply);
            Console.WriteLine(BitConverter.ToString(arpRequest.SenderMACAdress).Replace("-", ":"));

            //wyślij zapakowana w ramke Ethernet
            SendFrame(new EthernetFrame(arpRequest.SenderMACAdress, targetMAC, NetworkConstants.ETHERTYPE_ARP, arpReplyBytes), ConnectedNetwork[0]);
        }

        //Nasza odpowiedź na to co Host/Router przesyła
        else if (targetIP == thisIP && arpRequest.Opcode == NetworkConstants.ARP_REPLY)
        {
            LoggingManager.PrintPositive($"[{Name}] Otrzymałem odpowiedz");

            //tworzenie nowej ramki ArpReply
            AdressResolutionProtocol arpReply = AdressResolutionProtocol.Deserialize(payload);
            targetMAC = arpReply.SenderMACAdress;

            SaveToArpCache(arpReply.SenderIPAdress, targetMAC);
        }
        else
        {
            LoggingManager.PrintNormal($"[{Name}] answers: {ConvertionManager.IPtoString(arpRequest.TargetIPAdress)} is not mine");
        }
    }

    protected virtual void HandleIP(byte[] payload)
    {
        Console.WriteLine($"Jestem router {MacAdress} i mam IP: {IpAdress} , dostałem ramkę IPv4");
    }


    public void SaveToArpCache(byte[] IpAdress, byte[] MacAdress)
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
        

        arpCache.Add(ConvertionManager.IPtoString(IpAdress), MacAdress);

        //Print dodanej pary IP <-> MAC
        foreach (var pair in arpCache)
        {
            if (pair.Key == ConvertionManager.IPtoString(IpAdress))
            {
                LoggingManager.PrintPositive($"Zapisuje w ARP CACHE  IP: {pair.Key} -> MAC: {BitConverter.ToString(pair.Value).Replace("-", ":")}");
            }
        }
    }

    protected virtual bool IsItMyMAC(byte[] mac)
    {
        string receivedMAC = ConvertionManager.MACtoString(mac);

        string myMAC = ConvertionManager.MACtoString(MacAdress);

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