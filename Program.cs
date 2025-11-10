using System.Runtime.Intrinsics.Wasm;

public class Program
{
    public static void Main(string[] args)
    {



        //Sieć
        Network LAN = new Network();
        Network WAN1 = new Network();
        Network WAN2 = new Network();
        Network WAN3 = new Network();
        Network WAN4 = new Network();


        //Sender and Receiver
        Host me = new Host("my computer", [11, 22, 33, 44, 55, 66], [192, 23, 1, 10]);

        Host myGirlfriend = new Host("Her computer", [12, 22, 33, 44, 55, 66], [192, 23, 1, 20]);

        Host myFlatmate = new Host("His computer", [13, 24, 35, 46, 57, 68], [192, 23, 1, 34]);

        Host target = new Host("Google", [88, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF], [8, 8, 8, 8]);

        // Router 1 
        byte[] router1MAC = new byte[] { 0xB3, 0x31, 0x11, 0x33, 0x33, 0x33 };
        byte[] router1IP = new byte[] { 100, 101, 0, 10 };
        Router router1 = new Router("My route(Gateway)", router1MAC, router1IP);

        // Router 2
        byte[] router2MAC = new byte[] { 0xB4, 0x41, 0x11, 0x44, 0x44, 0x44 };
        byte[] router2IP = new byte[] { 200, 202, 0, 20 };
        Router router2 = new Router("Router ISO", router2MAC, router2IP);

        // Router 3 (poprawione IP)
        byte[] router3MAC = new byte[] { 0xB5, 0x51, 0x11, 0x55, 0x55, 0x55 };
        byte[] router3IP = new byte[] { 10, 0, 0, 30 };  // Np. 10.0.0.30
        Router router3 = new Router("Router ISO", router3MAC, router3IP);

        // Router 4 (poprawione IP - np. Google DNS)
        byte[] router4MAC = new byte[] { 0xB6, 0x61, 0x61, 0x65, 0x65, 0x65 };
        byte[] router4IP = new byte[] { 8, 8, 8, 8 };  // Google DNS
        Router router4 = new Router("Google Router", router4MAC, router4IP);



        LAN.Connect(me);
        LAN.Connect(myGirlfriend);
        LAN.Connect(myFlatmate);
        LAN.Connect(router1);

        me.routingTable.AddRoute(new Route([192, 23, 0, 0], [255, 255, 0, 0], [192, 32, 1, 1], LAN)); //lokalna mała sieć 
        me.routingTable.AddRoute(new Route([192, 23, 2, 0], [255, 255, 255, 0], null, LAN)); //lokalna mała sieć 
        me.routingTable.SetDefaultGateway(router1.IpAdress, LAN);
        me.routingTable.AddRoute(new Route(myFlatmate.IpAdress, myFlatmate.ConnectedNetwork[0].Netmask, null, LAN));
        me.routingTable.AddRoute(new Route(myGirlfriend.IpAdress, myGirlfriend.ConnectedNetwork[0].Netmask, null, LAN));

        WAN1.Connect(router1);
        WAN1.Connect(router2);

        WAN2.Connect(router2);
        WAN2.Connect(router3);

        WAN3.Connect(router3);
        WAN3.Connect(router4);

        WAN4.Connect(router4);
        WAN4.Connect(target);


        Console.WriteLine(ConvertionManager.MACtoString(me.MacAdress));
        me.SendPacket(target.IpAdress,[255]);


        
        // ushort opcode = 1;
        // byte[] senderMAC = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
        // byte[] senderIP = new byte[] { 192, 168, 0, 10 };
        // byte[] targetMAC = new byte[] { 0, 0, 0, 0, 0, 0 }; // nieznamy go 
        // byte[] targetIP = new byte[] { 192, 168, 0, 1 };

        // AdressResolutionProtocol adr = new AdressResolutionProtocol(opcode, senderMAC, senderIP, targetMAC, targetIP);

        // Console.WriteLine(adr);

        // byte[] destinationMAC = new byte[] { 0b10001000, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        // byte[] sourceMAC = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
        // ushort etherType = 0x0806;
        // byte[] payload = AdressResolutionProtocol.Serialize(adr);

        // EthernetFrame ramka = new EthernetFrame(destinationMAC, sourceMAC, etherType, payload);

        // Console.WriteLine(ramka);



    }
}