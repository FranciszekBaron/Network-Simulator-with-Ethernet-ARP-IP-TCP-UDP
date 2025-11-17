public class IPPacket : IProtocol<IPPacket>
{
    public byte Version { get; set; } = 4; //potrzebne 4 bity

    public byte HeaderLength { get; set; } = 5; //5 * 4bajty = 20! , ale na sama zmienne 4 bity

    public byte TypeOfService { get; set; } = 0;

    public ushort TotalLength { get; set; } // Liczymy w konstruktorze 

    public ushort Identification { get; set; } // przez ID

    public byte Flags { get; set; } = 1;

    public ushort FragmentOffset { get; set; } = 3;

    public byte TTL { get; set; } = 64;

    public byte Protocol { get; set; } //Moze to być ICMP/TCP/UDP

    public ushort HeaderChecksum { get; set; } // Liczymy w Serialize

    public byte[] SourceIP { get; set; } = new byte[4];

    public byte[] DestinationIP { get; set; } = new byte[4];

    public byte[] Payload { get; set; }


    private static ushort nextID = 0;

    private static byte HEADER_LEN = 20;

    public IPPacket(byte[] sourceIP, byte[] destinationIP, byte protocol, byte[] payload)
    {
        SourceIP = sourceIP;
        DestinationIP = destinationIP;
        Protocol = protocol;
        Payload = payload;

        Identification = nextID++;
        TotalLength = (ushort)(HEADER_LEN + payload.Length);
    }
    
    public IPPacket() //Dla Deserializacji
    {
        
    }


    public static byte[] Serialize(IPPacket packet)
    {
        // versionAndHeaderLen = 1;
        // totalLenghtLen = 2;
        // identificationLen = 2;
        // flagsLen = 1;
        // fragmentOffsetLen = 2;
        // ttlLen = 1;
        // protocolLEN = 1;
        // headerChecksumLen = 2;



        int offset = 0;
        byte[] header = new byte[HEADER_LEN];

        byte version = packet.Version;
        byte headerLength = packet.HeaderLength;
        header[offset] = (byte)((version << 4) | headerLength);
        offset += 1;

        header[offset] = packet.TypeOfService;
        offset += 1;

        ushort totalLength = packet.TotalLength;
        header[offset] = (byte)(totalLength >> 8); //starszy byte 
        offset += 1;
        header[offset] = (byte)(totalLength & 0xFF); //mlodszy byte
        offset += 1;

        ushort identification = packet.Identification;
        header[offset] = (byte)(identification >> 8); //starszy byte 
        offset += 1;
        header[offset] = (byte)(identification & 0xFF); //mlodszy byte
        offset += 1;


        byte flags = packet.Flags;
        ushort fragmentOffset = packet.FragmentOffset;
        ushort flagsAndFragmentCombined = (ushort)((flags << 13) | fragmentOffset);// Połączenie 3bitów na flagę i 13 bitów na fragOffset

        header[offset] = (byte)(flagsAndFragmentCombined >> 8); //starszy byte 
        offset += 1;
        header[offset] = (byte)(flagsAndFragmentCombined & 0xFF); //mlodszy byte
        offset += 1;

        header[offset] = packet.TTL;
        offset += 1;

        header[offset] = packet.Protocol;
        offset += 1;

        ushort headerChecksum = packet.HeaderChecksum;
        header[offset] = (byte)(headerChecksum >> 8); //starszy byte 
        offset += 1;
        header[offset] = (byte)(headerChecksum & 0xFF); //mlodszy byte
        offset += 1;

        Array.Copy(packet.SourceIP, 0, header, offset, packet.SourceIP.Length);
        offset += packet.SourceIP.Length;

        Array.Copy(packet.DestinationIP, 0, header, offset, packet.DestinationIP.Length);
        offset += packet.DestinationIP.Length;

        //Obliczenie Checksum, po uzupełnieniu całego header
        ushort checkSumCalculated = CalculateChecksum(header);
        header[10] = (byte)(checkSumCalculated >> 8);
        header[11] = (byte)(checkSumCalculated & 0xFF);


        byte[] result = new byte[HEADER_LEN + packet.Payload.Length];

        Array.Copy(header, 0, result, 0, header.Length);

        Array.Copy(packet.Payload, 0, result, header.Length, packet.Payload.Length);

        return result;
    }


    public static ushort CalculateChecksum(byte[] header) //dodaje algorytmem One's Addition Complete, czyli jak przekroczy 65,535 bierze tylko przekroczenie i dalej dodaje 
    {
        UInt32 sum = 0;

        for (int i = 0; i < header.Length; i += 2)
        {
            ushort value = (ushort)((header[i] << 8) | header[i+1]);

            sum += value;

            while (sum > 0xFFFF)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }
        }

        return (ushort)~sum; //Negacja robi 
    }


    public static IPPacket Deserialize(byte[] bytes)
    {

        if (bytes.Length < 20)
            throw new Exception("Packet too short");

        IPPacket deserialized = new IPPacket();
        int offset = 0;

        byte version = (byte)((bytes[offset] >> 4) & 0x0F);

        byte headerLength = (byte)(bytes[offset] & 0x0F);
        offset += 1;

        byte tos = bytes[offset];
        offset += 1;

        ushort totalLength = (ushort)((bytes[offset] << 8) | (bytes[offset + 1]));
        offset += 2;

        ushort identification = (ushort)((bytes[offset] << 8) | bytes[offset + 1]);
        offset += 2;


        ushort combined = (ushort)((bytes[offset] << 8) | (bytes[offset + 1]));

        byte flags = (byte)((combined >> 13) & 0x07);
        ushort fragmentOffset = (ushort)(combined & 0x1FFF); //0x1FFF -> 0x1 - 0001  0xF - 1111 0xF - 1111 0xF - 1111 
        offset += 2;

        // Console.WriteLine($"Offset: {offset}, flags: {Convert.ToString(combined >> 13, 2).PadLeft(8, '0')}, fragmentOffset: {Convert.ToString(fragmentOffset, 2).PadLeft(16, '0')}");

        byte ttl = bytes[offset];
        offset += 1;

        byte protocol = bytes[offset];
        offset += 1;

        ushort checkSum = (ushort)((bytes[offset] << 8) | (bytes[offset + 1]));
        offset += 2;

        byte[] sourceIP = new byte[4];
        Array.Copy(bytes, offset, sourceIP, 0, sourceIP.Length);
        offset += 4;

        byte[] destIP = new byte[4];
        Array.Copy(bytes, offset, destIP, 0, destIP.Length);
        offset += 4;


        byte[] header = new byte[HEADER_LEN];
        Array.Copy(bytes, 0, header, 0, header.Length);

        if (!IsChecksumValid(header))
            throw new Exception("Checksum Invalid");

        byte[] payload = new byte[bytes.Length - HEADER_LEN];



        Array.Copy(bytes, offset, payload, 0, payload.Length);


        deserialized.Version = version;
        deserialized.HeaderLength = headerLength;
        deserialized.TypeOfService = tos;
        deserialized.TotalLength = totalLength;
        deserialized.Identification = identification;
        deserialized.Flags = flags;
        deserialized.FragmentOffset = fragmentOffset;
        deserialized.TTL = ttl;
        deserialized.Protocol = protocol;
        deserialized.HeaderChecksum = checkSum;
        deserialized.SourceIP = sourceIP;
        deserialized.DestinationIP = destIP;
        deserialized.Payload = payload;


        return deserialized;
    }
    

    public static bool IsChecksumValid(byte[] bytes)
    {

        UInt32 sum = 0;

        for (int i = 0; i < bytes.Length; i += 2)
        {
            ushort value = (ushort)((bytes[i] << 8) | bytes[i + 1]);

            sum += value;

            while (sum > 0xFFFF)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

        }
        
        return sum == 0xFFFF;
    }
    


    public override string ToString()
    {
        string protocolName = Protocol switch
        {
            1 => "ICMP",
            6 => "TCP",
            17 => "UDP",
            _ => "Unknown"
        };
        
        string flagsStr = GetFlagsString();
        
        return
        $@"IP Packet:
            Version: {Version}
            Header Length: {HeaderLength} ({HeaderLength * 4} bytes)
            Total Length: {TotalLength} bytes
            Identification: 0x{Identification:X4}
            Flags: {flagsStr}
            Fragment Offset: {FragmentOffset}
            TTL: {TTL}
            Protocol: {Protocol} ({protocolName})
            Header Checksum: 0x{HeaderChecksum:X4}
            Source IP: {ConvertionManager.IPtoString(SourceIP)}
            Destination IP: {ConvertionManager.IPtoString(DestinationIP)}
            Payload Length: {Payload.Length} bytes
        ";
    }

    private string GetFlagsString()
    {
        List<string> flags = new List<string>();
        
        if ((Flags & 0x04) != 0)  // Bit 0: Reserved (powinien być 0)
            flags.Add("Reserved");
        
        if ((Flags & 0x02) != 0)  // Bit 1: DF (Don't Fragment)
            flags.Add("DF");
        
        if ((Flags & 0x01) != 0)  // Bit 2: MF (More Fragments)
            flags.Add("MF");
        
        return flags.Count > 0 ? string.Join(", ", flags) : "None";
    }

    

    

}