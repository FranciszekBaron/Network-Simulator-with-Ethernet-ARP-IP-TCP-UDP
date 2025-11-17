using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

public class Host : Device
{   
    public NetworkInterface Interface { get; set; } // Host ma jeden MAC i IP 

    public Host(string name,byte[] MacAdress, byte[] IpAdress,byte[] mask) : base(name)
    {
        Interface = new NetworkInterface("eth0", IpAdress, MacAdress, mask);
    }
    
    public virtual void SendPacket(byte[] IPAdrress,byte[] data)
    {
        //Znajdzmy NextHOP
        LoggingManager.PrintNormal("========= HOST A Routing Table =========");
        LoggingManager.PrintNormal(RoutingTable.ToString());
        LoggingManager.PrintNormal("========================================");

        byte[] nextHop = RoutingTable.GetNextHop(IPAdrress);

        byte[] nextHopMAC;

        string nextHopToString = ConvertionManager.IPtoString(nextHop);

        LoggingManager.PrintDevice($"HEJ to [{ConvertionManager.IPtoString(Interface.IpAdress)}] i moj MacAdress to: {ConvertionManager.MACtoString(Interface.MacAdress)}");

        if (Interface.ArpCache.ContainsKey(nextHopToString))
        {
            nextHopMAC = Interface.ArpCache[ConvertionManager.IPtoString(nextHop)];
            LoggingManager.PrintNormal($"[{Name}] MAC from ARP CACHE is:" + ConvertionManager.MACtoString(nextHopMAC));
        }
        else
        {
            //Nie ma IP <-> MAC w ARP? Zrób ARP Request
            LoggingManager.PrintNormal($"Wykonuje ARP Request do ... {ConvertionManager.IPtoString(nextHop)}");
            SendArpRequest(nextHop,Interface);

            nextHopMAC = Interface.ArpCache[nextHopToString];
        }

        IPPacket ipPacket = new IPPacket(
            sourceIP: this.Interface.IpAdress,
            destinationIP: IPAdrress,
            protocol: 1,
            payload: data
        );

        byte[] ipBytes = IPPacket.Serialize(ipPacket);

        EthernetFrame ethernetFrame = new EthernetFrame(
            destinationMAC: nextHopMAC,
            sourceMAC: this.Interface.MacAdress,
            0x800,
            payload: ipBytes);

        LoggingManager.PrintNormal($"[{Name}] Created Ethernet Frame: {ConvertionManager.MACtoString(ethernetFrame.SourceMAC)} → {ConvertionManager.MACtoString(ethernetFrame.DestinationMAC)}");

        LoggingManager.PrintNormal("Sending packet...");
        SendFrame(ethernetFrame, Interface);
    }
    

    protected override void HandleARP(byte[] payload,NetworkInterface incomingInterface)
    {

        //Deserialize packet
        AdressResolutionProtocol arpRequest = AdressResolutionProtocol.Deserialize(payload);

        string targetIP = ConvertionManager.IPtoString(arpRequest.TargetIPAdress);
        string thisIP = ConvertionManager.IPtoString(Interface.IpAdress);
        byte[]? targetMAC = null;

        Console.WriteLine(ConvertionManager.IPtoString(Interface.IpAdress));

        //Znalezienie Hosta/Routera do którego wysyłamy
        if (targetIP == thisIP && arpRequest.Opcode == NetworkConstants.ARP_REQUEST)
        {

            LoggingManager.PrintDevice($"[{ConvertionManager.IPtoString(Interface.IpAdress)}] answers: {ConvertionManager.IPtoString(arpRequest.TargetIPAdress)} is mine");

            //tworzenie nowej ramki ArpReply
            targetMAC = Interface.MacAdress;
            AdressResolutionProtocol arpReply = new AdressResolutionProtocol(
                opcode: 2,
                sendeMacAdress: targetMAC,
                senderIPAdress: Interface.IpAdress,
                targetMacAdress: arpRequest.SenderMACAdress,
                targetIPAdress: arpRequest.SenderIPAdress
            );

            byte[] arpReplyBytes = AdressResolutionProtocol.Serialize(arpReply);
            //wyślij zapakowana w ramke Ethernet
            SendFrame(new EthernetFrame(arpRequest.SenderMACAdress, targetMAC, NetworkConstants.ETHERTYPE_ARP, arpReplyBytes), Interface);
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

    protected override void HandleIP(byte[] payload, NetworkInterface incomingInterface)
    {
        IPPacket packet = IPPacket.Deserialize(payload);

        LoggingManager.PrintPositive($"Jestem router {ConvertionManager.MACtoString(Interface.MacAdress)} i mam IP: {ConvertionManager.IPtoString(Interface.IpAdress)} , dostałem ramkę IPv4");
        LoggingManager.PrintNormal("Data:");
        for (int i = 0; i < packet.Payload.Length; i++)
        {
            Console.Write(packet.Payload[i] + ",");
        }
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
        
        Interface.ArpCache.Add(ConvertionManager.IPtoString(IpAdress), MacAdress);
        LoggingManager.PrintNormal($"{Interface}");
        //Print dodanej pary IP <-> MAC
        foreach (var pair in Interface.ArpCache)
        {
            if (pair.Key == ConvertionManager.IPtoString(IpAdress))
            {
                LoggingManager.PrintPositive($"Zapisuje w ARP CACHE  IP: {pair.Key} -> MAC: {BitConverter.ToString(pair.Value).Replace("-", ":")}");
            }
        }
    }
}