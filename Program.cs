using System.Runtime.Intrinsics.Wasm;

public class Program
{
    public static void Main(string[] args)
    {



        //Sieć
        Network LAN = new Network("LAN");
        Network WAN1 = new Network("WAN1");
        Network WAN2 = new Network("WAN2");
        Network WAN3 = new Network("WAN3");
        Network WAN4 = new Network("WAN4");

        //Sender and Receiver
        Host me = new Host("my computer", [11, 22, 33, 44, 55, 66], [192, 23, 1, 10],[255,255,255,0]);

        Host myGirlfriend = new Host("Her computer", [12, 22, 33, 44, 55, 66], [192, 23, 1, 20],[255,255,255,0]);

        Host myFlatmate = new Host("His computer", [13, 24, 35, 46, 57, 68], [192, 23, 1, 34],[255,255,255,0]);

        Host target = new Host("Google", [88, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF], [8, 8, 8, 8],[255,255,255,0]);

        // Router 1 
        byte[] router1MAC = [0xB3, 0x31, 0x11, 0x33, 0x33, 0x33 ];
        byte[] router1IP = [ 100, 101, 0, 10 ];
        Router router1 = new Router("My route(Gateway)");

        var eth0 = router1.AddInterface("eth0",router1IP,router1MAC,[255, 255, 255, 0 ]);
        var eth1 = router1.AddInterface("eth1", [0xAA, 0xBB, 0xCC, 0x00, 0x01, 0x02], [10, 0, 0, 1], [255, 255, 255, 0]);

        // Router 2
        byte[] router2MAC = new byte[] { 0xB4, 0x41, 0x11, 0x44, 0x44, 0x44 };
        byte[] router2IP = new byte[] { 200, 202, 0, 20 };
        Router router2 = new Router("Router ISO");

        // Router 3 (poprawione IP)
        byte[] router3MAC = new byte[] { 0xB5, 0x51, 0x11, 0x55, 0x55, 0x55 };
        byte[] router3IP = new byte[] { 10, 0, 0, 30 };  // Np. 10.0.0.30
        Router router3 = new Router("Router ISO");

        // Router 4 (poprawione IP - np. Google DNS)
        byte[] router4MAC = new byte[] { 0xB6, 0x61, 0x61, 0x65, 0x65, 0x65 };
        byte[] router4IP = new byte[] { 8, 8, 8, 8 };  // Google DNS
        Router router4 = new Router("Google Router");


        LAN.ConnectDevice(me,me.Interface);
        LAN.ConnectDevice(myGirlfriend,myGirlfriend.Interface);
        LAN.ConnectDevice(myFlatmate, myFlatmate.Interface);
        
        //Router ma rózne (wiele) kart sieciowych - interfaców
        LAN.ConnectDevice(router1, eth0);
    

        me.RoutingTable.AddRoute(new Route([192, 23, 0, 0], [255, 255, 0, 0], [192, 32, 1, 1], me.Interface)); //lokalna mała sieć 
        me.RoutingTable.AddRoute(new Route([192, 23, 2, 0], [255, 255, 255, 0], null, me.Interface)); //lokalna mała sieć 
        me.RoutingTable.SetDefaultGateway(router1.Interfaces[0].IpAdress, me.Interface);
        me.RoutingTable.AddRoute(new Route(myFlatmate.Interface.IpAdress, myFlatmate.ConnectedNetwork[0].Netmask, null, me.Interface));
        me.RoutingTable.AddRoute(new Route(myGirlfriend.Interface.IpAdress, myGirlfriend.ConnectedNetwork[0].Netmask, null, me.Interface));

        WAN1.ConnectDevice(router1,eth1);
        // WAN1.ConnectDevice(router2,router2.Interfaces[0]);

        // WAN2.ConnectDevice(router2,router2.Interfaces[0]);
        // WAN2.ConnectDevice(router3,router3.Interfaces[0]);

        // WAN3.ConnectDevice(router3,router3.Interfaces[0]);
        // WAN3.ConnectDevice(router4,router4.Interfaces[0]);

        // WAN4.ConnectDevice(router4,router4.Interfaces[0]);
        // WAN4.ConnectDevice(target,target.Interface);


        me.SendPacket(target.Interface.IpAdress,[255]);


        
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