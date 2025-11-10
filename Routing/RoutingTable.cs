public class RoutingTable
{
    public List<Route> routes = new List<Route>();
    public RoutingTable()
    {
        
    }
    
    public void SetDefaultGateway(byte[] IPAdrress,Network iface)
    {
        routes.Add(new Route([0, 0, 0, 0], [0, 0, 0, 0], IPAdrress, iface));
    }


    public byte[] GetNextHop(byte[] IPAdrress)
    {
        int longestPrefix = -1;
        Route bestMatch = null;

        foreach (var route in routes)
        {
            //"jesli ta sama sieć"
            if (route.Matches(IPAdrress))
            {
                int prefixLenght = route.CountPrefix();

                if (prefixLenght > longestPrefix)
                {
                    longestPrefix = route.CountPrefix();
                    bestMatch = route;
                }
            }
        }

        if(bestMatch == null)
        {
            throw new Exception($"No route to host: {IPAdrress}");
        }

        if(bestMatch.Gateaway == null)
        {
            return IPAdrress;
        }
        else
        {
            return bestMatch.Gateaway;
        }
    }
    
    public void AddRoute(Route route)
    {
        routes.Add(route);
    }
}