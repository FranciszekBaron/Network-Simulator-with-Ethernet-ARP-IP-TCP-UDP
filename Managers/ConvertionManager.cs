public class ConvertionManager
{
    public static string IPtoString(byte[] ip)
    {
        return $"{ip[0]}.{ip[1]}.{ip[2]}.{ip[3]}";
    }
    

    public static string MACtoString(byte[] mac)
    {
        return $"{mac[0]}:{mac[1]}:{mac[2]}:{mac[3]}:{mac[4]}:{mac[5]}";
    }
}