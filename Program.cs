using System.Net;
using System.Runtime.Intrinsics.Wasm;

public class Program
{
    public static void Main(string[] args)
    {



        //Sieć
        Network LAN = new Network("LAN");
        Network WAN1 = new Network("WAN1");
        // Network WAN2 = new Network("WAN2");
        // Network WAN3 = new Network("WAN3");
        // Network WAN4 = new Network("WAN4");

        //============== HOSTY W LAN ===========
        Host me = new Host("My computer",
            MacAdress: [11, 22, 33, 44, 55, 66],
            IpAdress: [192, 23, 1, 10],
            mask:[255, 255, 255, 0]
        );

        Host myGirlfriend = new Host("Her computer",
            MacAdress: [12, 22, 33, 44, 55, 66],
            IpAdress: [192, 23, 1, 20],
            mask: [255, 255, 255, 0]
        );

        Host myFlatmate = new Host("His computer",
            MacAdress: [13, 24, 35, 46, 57, 68],
            IpAdress: [192, 23, 1, 34],
            mask: [255, 255, 255, 0]
        );

        //======= TARGET ========
        Host target = new Host("Google", [88, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF], [8, 8, 8, 8],[255,255,255,0]);

        // ========== ROUTER 1 (DOMOWY ROUTER ================
        Router router1 = new Router("Home Gateaway");

        //incoming interface
        var router1_eth0 = router1.AddInterface("eth0",
            IpAdress: [192, 23, 1, 1], // <-- ta sama sieć w której są hosty 
            MacAdress: [0xB3, 0x31, 0x11, 0x33, 0x33, 0x33],
            mask: [255, 255, 255, 0]
        );
        //outgoing interface ale on sie nie rowna gateaway, gateaway to incoming interface w nastepnym routerze
        var router1_eth1 = router1.AddInterface("eth1",
            IpAdress: [100, 101, 0, 10],
            MacAdress: [0xAA, 0xBB, 0xCC, 0x00, 0x01, 0x02],
            mask: [255, 255, 255, 0]
        );
        

        // ========== ROUTER 2 (DALEJ ROUTER) ================
        Router router2 = new Router("Router ISO");

        var router2_eth0 = router2.AddInterface("eth0",
            IpAdress:[100,101,0,1], //<-- ta sama sieć w której był router1
            MacAdress: [0xB4, 0x41, 0x11, 0x44, 0x44, 0x44],
            mask: [255, 255, 255, 0]
        );

        var router2_eth1 = router2.AddInterface("eth1",
            IpAdress: [10, 0, 0, 1],
            MacAdress: [0xAA, 0xBB, 0xCC, 0x00, 0x01, 0x02],
            mask: [255, 255, 255, 0]
        );


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
        LAN.ConnectDevice(router1, router1_eth0);
    
        me.RoutingTable.AddRoute(new Route([192, 23, 0, 0], [255, 255, 0, 0], null, me.Interface)); //lokalna mała sieć 
        me.RoutingTable.SetDefaultGateway(router1_eth0.IpAdress, me.Interface);



        WAN1.ConnectDevice(router1, router1_eth1);
        router1.RoutingTable.AddRoute(new Route([200, 2, 200, 0], [255, 255, 255, 0], null, router1_eth1));
        router1.RoutingTable.SetDefaultGateway(router2_eth0.IpAdress, outgoingInterface: router1_eth1);

        // WAN1.ConnectDevice(router2,router2.Interfaces[0]);


        // WAN2.ConnectDevice(router2,router2.Interfaces[0]);
        // WAN2.ConnectDevice(router3,router3.Interfaces[0]);

        // WAN3.ConnectDevice(router3,router3.Interfaces[0]);
        // WAN3.ConnectDevice(router4,router4.Interfaces[0]);

        // WAN4.ConnectDevice(router4,router4.Interfaces[0]);
        // WAN4.ConnectDevice(target,target.Interface);


        me.SendPacket(target.Interface.IpAdress,[255]);


    }
}