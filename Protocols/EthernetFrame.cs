public class EthernetFrame : IProtocol<EthernetFrame>
{
    public byte[] DestinationMAC { get; set; }

    public byte[] SourceMAC { get; set; }

    public ushort EtherType { get; set; }

    public byte[] Payload { get; set; }

    public EthernetFrame(byte[] destinationMAC,byte[] sourceMAC,ushort etherType,byte[] payload)
    {
        if (destinationMAC.Length != 6 || sourceMAC.Length != 6)
            throw new ArgumentException("MAC must be 6");

        DestinationMAC = destinationMAC;
        SourceMAC = sourceMAC;
        EtherType = etherType;
        Payload = payload;
    }


    public static byte[] Serialize(EthernetFrame frame)
    {
        int destLen = frame.DestinationMAC.Length;
        int srcLen = frame.SourceMAC.Length;
        int etherLen = 2;
        int totalLen = destLen + srcLen + etherLen + frame.Payload.Length;

        byte[] result = new byte[totalLen];

        Array.Copy(frame.DestinationMAC, 0, result, 0, destLen);
        Array.Copy(frame.SourceMAC, 0, result, destLen, srcLen);

        ushort etherType = frame.EtherType;
        result[destLen + srcLen] = (byte)(etherType >> 8);
        result[destLen + srcLen + 1] = (byte)(etherType & 0xFF);

        Array.Copy(frame.Payload, 0, result, destLen + srcLen + etherLen, frame.Payload.Length);

        return result;
    }


    public static EthernetFrame Deserialize(byte[] bytes)
    {
        int destLen = 6;
        int srcLen = 6;
        int etherLen = 2;

        if (bytes.Length < destLen + srcLen + etherLen)
            throw new ArgumentException("To nie jest poprawna ramka Ethernet");

        int offset = 0;
        byte[] destinationMAC = new byte[destLen];
        Array.Copy(bytes, offset, destinationMAC, 0, destLen);
        offset += destLen;

        byte[] sourceMAC = new byte[srcLen];
        Array.Copy(bytes, offset, sourceMAC, 0, srcLen);
        offset += srcLen;

        ushort etherType = (ushort)(bytes[sourceMAC.Length + destinationMAC.Length] << 8 | bytes[sourceMAC.Length + destinationMAC.Length + 1]);
        offset += etherLen;

        byte[] payload = new byte[bytes.Length - destLen - srcLen - etherLen];
        Array.Copy(bytes, offset, payload, 0, payload.Length);

        return new EthernetFrame(destinationMAC, sourceMAC, etherType, payload);
    }


    public override string ToString()
    {
        string etherTypeName = EtherType switch
        {
            0x0800 => "IPv4",
            0x0806 => "ARP",
            0x86DD => "IPv6",
            _ => "Unknown"
        };

        return
        $@"Ethernet Frame:
        Destination MAC: {BitConverter.ToString(DestinationMAC).Replace("-", ":")}
        Source MAC: {BitConverter.ToString(SourceMAC).Replace("-",":")}
        EtherType: 0x{EtherType:X4} ({etherTypeName})
        PayLoad Lenght: {Payload.Length} 
        ";
    }

   
}