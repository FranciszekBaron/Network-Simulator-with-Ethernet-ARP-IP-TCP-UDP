
public class ConvertionManager
{
    public static string IPtoString(byte[] ip)
    {
        return $"{ip[0]}.{ip[1]}.{ip[2]}.{ip[3]}";
    }
    

    public static string MACtoString(byte[] mac)
    {
        return BitConverter.ToString(mac).Replace("-",":");
    }

    internal static string? IPtoString(string v)
    {
        throw new NotImplementedException();
    }
}