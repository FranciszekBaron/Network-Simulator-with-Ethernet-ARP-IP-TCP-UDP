using System.Reflection.PortableExecutable;

public abstract class Device
{
    public string Name { get; set; }

    // KERNEL SPACE
    // public List<Network> ConnectedNetwork { get; set; }
    public RoutingTable RoutingTable { get; set; }
    
    public Device(string name)
    {
        Name = name;
        RoutingTable = new RoutingTable();
    }
    
    protected abstract void HandleIP(byte[] payload,NetworkInterface networkInterface);
    protected abstract void HandleARP(byte[] payload, NetworkInterface networkInterface);
    
    public void SendArpRequest(byte[] targetIP,NetworkInterface outgoingInterface)
    {
        if (outgoingInterface.ConnectedNetwork == null)
        {
            throw new Exception($"Interface {outgoingInterface.Name} not connected to any network");
        }
        AdressResolutionProtocol arp = new AdressResolutionProtocol(1, outgoingInterface.MacAdress, outgoingInterface.IpAdress, [0, 0, 0, 0, 0, 0], targetIP);
        byte[] arpBytes = AdressResolutionProtocol.Serialize(arp);
        EthernetFrame sendedEthernetFrame = new EthernetFrame([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF], outgoingInterface.MacAdress, 0x806, arpBytes);

        SendFrame(sendedEthernetFrame, outgoingInterface);
    }

    public virtual void SendFrame(EthernetFrame ethernetFrame, NetworkInterface outgoingInterface)
    {
        if (outgoingInterface.ConnectedNetwork == null)
        {
            throw new Exception($"Interface {outgoingInterface.Name} not connected to any network");
        }
        if (IsBroadcast(ethernetFrame.DestinationMAC)) //Broadcast - do wszystkich 
        {
            outgoingInterface.ConnectedNetwork.Broadcast(this, ethernetFrame);
        }
        else //Unicast - do konkretnego MAC
        {
            outgoingInterface.ConnectedNetwork.Unicast(ethernetFrame);
        }
        
        
    }


    public virtual void ReceiveFrame(EthernetFrame ethernetFrame, NetworkInterface incomingInterface)
    {
        //Czy moj MAC??

        if (!IsItMyMAC(ethernetFrame.DestinationMAC, incomingInterface) && !IsBroadcast(ethernetFrame.DestinationMAC))
        {
            LoggingManager.PrintWarning($"Given MAC ({ethernetFrame.DestinationMAC}) not in local network, not a Broadcast either");
            return;
        }

        //Jaki EthernetType - taka operacja

        if (ethernetFrame.EtherType == NetworkConstants.ETHERTYPE_ARP)
        {
            HandleARP(ethernetFrame.Payload, incomingInterface);
        }
        else if (ethernetFrame.EtherType == NetworkConstants.ETHERTYPE_IP)
        {
            HandleIP(ethernetFrame.Payload, incomingInterface);
        }
        else
        {
            LoggingManager.PrintWarning("No such EtherType available");
        }
    }

    public static byte[] CalculateNetwork(byte[] ipAdress,byte[] mask)
    {
        byte[] network = new byte[4];

        for(int i = 0; i < ipAdress.Length; i++)
        {
            network[i] = (byte)(ipAdress[i] & mask[i]);
        }
        return network;
    }
    
    public static bool IsItMyMAC(byte[] mac, NetworkInterface networkInterface)
    {
        string receivedMAC = ConvertionManager.MACtoString(mac);

        string myMAC = ConvertionManager.MACtoString(networkInterface.MacAdress);

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