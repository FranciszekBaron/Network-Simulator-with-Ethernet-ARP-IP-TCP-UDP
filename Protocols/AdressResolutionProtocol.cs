using System.Reflection.Emit;

public class AdressResolutionProtocol : IProtocol<AdressResolutionProtocol>
{
    public ushort HTYPE { get; set; } = 1;       // Ethernet
    public ushort PTYPE { get; set; } = 0x0800;  // IP
    public byte HLEN { get; set; } = 6;      // liczba bajtów w adresie MAC (dla Ethernet = 6)
    public byte PLEN { get; set; } = 4; // liczba bajtów w adresie IP (dla IPv4 = 4)
    public ushort Opcode { get; set; }
    public byte[] SenderMACAdress { get; set; } // 6bajtow
    public byte[] SenderIPAdress { get; set; } // 4bajty
    public byte[] TargetMACAdress { get; set; } // 6bajtow 
    public byte[] TargetIPAdress { get; set; } // 4bajty

    public AdressResolutionProtocol(ushort opcode, byte[] sendeMacAdress, byte[] senderIPAdress, byte[] targetMacAdress, byte[] targetIPAdress)
    {
        Opcode = opcode;
        SenderMACAdress = sendeMacAdress;
        SenderIPAdress = senderIPAdress;
        TargetMACAdress = targetMacAdress;
        TargetIPAdress = targetIPAdress;
    }
    
    public AdressResolutionProtocol(ushort htype, ushort ptype, byte hlen,byte plen , ushort opcode, byte[] sendeMacAdress, byte[] senderIPAdress, byte[] targetMacAdress, byte[] targetIPAdress)
    {

        HTYPE = htype;
        PTYPE = ptype;
        HLEN = hlen;
        PLEN = plen;
        Opcode = opcode;
        SenderMACAdress = sendeMacAdress;
        SenderIPAdress = senderIPAdress;
        TargetMACAdress = targetMacAdress;
        TargetIPAdress = targetIPAdress;
    }


    public static byte[] Serialize(AdressResolutionProtocol arp)
    {
        int hardwareTypeLen = 2;
        int protocolTypeLen = 2;
        int hardwareAdressLenghtLen = 1;
        int protocolAdressLenghtLen = 1;

        int opcodeLen = 2;
        int sendMACLen = arp.SenderMACAdress.Length;
        int sendIPLen = arp.SenderIPAdress.Length;
        int targMACLen = arp.TargetMACAdress.Length;
        int targIPLen = arp.TargetIPAdress.Length;

        byte[] bytes = new byte[hardwareTypeLen + protocolTypeLen + hardwareAdressLenghtLen + protocolAdressLenghtLen+ opcodeLen + sendMACLen + sendIPLen + targMACLen + targIPLen];
        int offset = 0;


        ushort hardwareType = arp.HTYPE;
        bytes[offset] = (byte)(hardwareType >> 8);
        offset += 1;
        bytes[offset] = (byte)(hardwareType & 0xFF);
        offset += 1;


        ushort protocolType = arp.PTYPE;
        bytes[offset] = (byte)(protocolType >> 8);
        offset += 1;
        bytes[offset] = (byte)(protocolType & 0xFF);
        offset += 1;



        bytes[offset] = arp.HLEN;
        offset += 1;
        bytes[offset] = arp.PLEN;
        offset += 1;



        ushort opcode = arp.Opcode;
        bytes[offset] = (byte)(opcode >> 8);
        offset += 1;
        bytes[offset] = (byte)(opcode & 0xFF);
        offset += 1;

        Array.Copy(arp.SenderMACAdress, 0, bytes, offset, sendMACLen);
        offset += sendMACLen;

        Array.Copy(arp.SenderIPAdress, 0, bytes, offset, sendIPLen);
        offset += sendIPLen;

        Array.Copy(arp.TargetMACAdress, 0, bytes, offset, targMACLen);
        offset += targMACLen;

        Array.Copy(arp.TargetIPAdress, 0, bytes, offset, targIPLen);
        offset += targIPLen;

        return bytes;
    }


    public static AdressResolutionProtocol Deserialize(byte[] bytes)
    {
        

        int hardwareTypeLen = 2;
        int protocolTypeLen = 2;
        int hardwareAdressLenghtLen = 1;
        int protocolAdressLenghtLen = 1;
        int opcodeLen = 2;
        int sendeMacAdressLen = 6;
        int senderIPAdressLen = 4;
        int targetMacAdressLen = 6;
        int targetIPAdressLen = 4;



        int offset = 0;

        ushort htype = (ushort)(bytes[offset] << 8 | bytes[offset+1]);
        offset += hardwareTypeLen;

        ushort ptype = (ushort)(bytes[offset] << 8 | bytes[offset +1]);
        offset += protocolTypeLen;

        byte hlen = bytes[offset];
        offset += hardwareAdressLenghtLen;
        byte plen = bytes[offset];
        offset += protocolAdressLenghtLen;

        ushort opcode = (ushort)(bytes[offset] << 8 | bytes[offset +1]);
        offset += opcodeLen;


        byte[] senderMACAdress = new byte[sendeMacAdressLen];
        Array.Copy(bytes, offset, senderMACAdress, 0, sendeMacAdressLen);
        offset += sendeMacAdressLen;

        byte[] senderIPAdress = new byte[senderIPAdressLen];
        Array.Copy(bytes, offset, senderIPAdress, 0, senderIPAdressLen);
        offset += senderIPAdressLen;

        byte[] targetMACAdress = new byte[targetMacAdressLen];
        Array.Copy(bytes, offset, targetMACAdress, 0, targetMacAdressLen);
        offset += targetMacAdressLen;

        byte[] targetIPAdress = new byte[targetIPAdressLen];
        Array.Copy(bytes, offset, targetIPAdress, 0, targetIPAdressLen);
        offset += targetIPAdressLen;

        return new AdressResolutionProtocol(htype,ptype,hlen,plen,opcode,senderMACAdress,senderIPAdress,targetMACAdress,targetIPAdress);
    }



    public override string ToString()
    {
        return
        $@"ARP Packet:
        Hardware type: {HTYPE} (Ethernet)
        Protocol type: 0x{PTYPE:X4} (IPv4)
        Hardware size: {HLEN}
        Protocol size: {PLEN}
        Opcode: {(Opcode == 1 ? "Request (1)" : Opcode == 2 ? "Reply (2)" : Opcode.ToString())}
        Sender MAC: {BitConverter.ToString(SenderMACAdress).Replace("-", ":")}
        Sender IP: {string.Join(".", SenderIPAdress)}
        Target MAC: {BitConverter.ToString(TargetMACAdress).Replace("-", ":")}
        Target IP: {string.Join(".", TargetIPAdress)}";
                
    }
}


