public static class NetworkConstants
{
    public static readonly byte[] BROADCAST_MAC = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    public static readonly byte[] ZERO_MAC = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    
    // EtherTypes
    public const ushort ETHERTYPE_IP = 0x0800;
    public const ushort ETHERTYPE_ARP = 0x0806;
    public const ushort ETHERTYPE_IPV6 = 0x86DD;
    
    // ARP Opcodes
    public const ushort ARP_REQUEST = 1;
    public const ushort ARP_REPLY = 2;

    public const int IP_ADRESS_LENGHT = 4;
    public const int MAC_ADRESS_LENGHT = 6;


    public static string BroadCastToString()
    {
        return ConvertionManager.MACtoString(BROADCAST_MAC);
    }
}