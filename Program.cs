using System.Net;
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
        Network GoogleDatacenter = new Network("WAN4");

        //============== HOSTY W LAN ===========
        Host me = new Host("My computer",
            MacAdress: [11, 22, 33, 44, 55, 66],
            IpAdress: [192, 23, 1, 10],
            mask:[255, 255, 255, 0]
        );

        Host myGirlfriend = new Host("Her computer",
            MacAdress: [12, 22, 33, 44, 55, 67],
            IpAdress: [192, 23, 1, 20],
            mask: [255, 255, 255, 0]
        );

        Host myFlatmate = new Host("His computer",
            MacAdress: [13, 24, 35, 46, 57, 68],
            IpAdress: [192, 23, 1, 34],
            mask: [255, 255, 255, 0]
        );

        //======= TARGET ========
        Host target = new Host("Google",
            MacAdress: [88, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF],
            IpAdress: [8, 8, 8, 8],
            mask: [255, 255, 255, 0]
        );



        // ========== ROUTER 1 (DOMOWY ROUTER ================
        Router router1 = new Router("Home Gateaway");

        //incoming interface
        var router1_eth0 = router1.AddInterface("eth0",
            IpAdress: [192, 23, 1, 1], // <-- ta sama sieć w której są hosty 
            MacAdress: [0xB3, 0x31, 0x11, 0x33, 0x33, 0x33],
            mask: [255, 255, 255, 0]
        );
        //outgoing interface 
        //nie rowna sie gateaway, gateaway to incoming interface w nastepnym routerze
        var router1_eth1 = router1.AddInterface("eth1",
            IpAdress: [100, 101, 0, 10],
            MacAdress: [0xA1, 0xB2, 0xCC, 0x00, 0x01, 0x02],
            mask: [255, 255, 255, 0]
        );



        // ========== ROUTER 2 (DALEJ ROUTER) ================
        Router router2 = new Router("Router ISO");

        //incoming interface
        var router2_eth0 = router2.AddInterface("eth0",
            IpAdress:[100,101,0,1], //<-- ta sama sieć w której był router1
            MacAdress: [0xB4, 0x41, 0x11, 0x44, 0x44, 0x44],
            mask: [255, 255, 255, 0]
        );
        //outgoing interface
        var router2_eth1 = router2.AddInterface("eth1",
            IpAdress: [204, 20, 20, 20],
            MacAdress: [0xA2, 0xB2, 0xCC, 0x00, 0x01, 0x02],
            mask: [255, 255, 255, 0]
        );


        // Router 3 (poprawione IP)
        // byte[] router3MAC = new byte[] { 0xB5, 0x51, 0x11, 0x55, 0x55, 0x55 };
        Router router3 = new Router("Router ISO/2");

        //incoming interface
        var router3_eth0 = router3.AddInterface("eth0",
            IpAdress:[204,20,0,1], //<-- ta sama sieć w której był router2
            MacAdress: [0xC4, 0x51, 0x11, 0x55, 0x55, 0x55],
            mask: [255, 255, 255, 0]
        );
        //outgoing interface
        var router3_eth1 = router3.AddInterface("eth1",
            IpAdress: [10, 0, 0, 10],
            MacAdress: [0xAA, 0xBB, 0xCC, 0x00, 0x01, 0x02],
            mask: [255, 255, 255, 0]
        );

        // Router 4 (poprawione IP - np. Google DNS)
        Router router4 = new Router("Google Router");

        var router4_eth0 = router4.AddInterface("eth0",
            IpAdress:[10,0,0,1], //<-- ta sama sieć w której był router3
            MacAdress: [0xFF, 0x71, 0x71, 0x75, 0x75, 0x75],
            mask: [255, 0,0, 0]
        );
        //outgoing interface
        var router4_eth1 = router4.AddInterface("eth1",
            IpAdress: [8, 8, 8, 1],  //  Gateway w sieci 8.8.8.0/24
            MacAdress: [0xAA, 0xBB, 0xCC, 0x00, 0x01, 0x02],
            mask: [255, 255, 255, 0]
        );


        //========== POŁĄCZENIA ==============
        //LAN 
        LAN.ConnectDevice(me,me.Interface);
        LAN.ConnectDevice(myGirlfriend,myGirlfriend.Interface);
        LAN.ConnectDevice(myFlatmate, myFlatmate.Interface);
        LAN.ConnectDevice(router1, router1_eth0);

        //ROUTINGTABLE DLA HOSTA
        me.RoutingTable.AddRoute(new Route([192, 23, 0, 0], [255, 255, 0, 0], null, me.Interface)); //lokalna mała sieć 
        me.RoutingTable.SetDefaultGateway(router1_eth0.IpAdress, outgoingInterface: me.Interface);

        //WAN
        WAN1.ConnectDevice(router1, router1_eth1);

        //ROUTINGTABLE DLA ROUTER1
        router1.RoutingTable.AddRoute(new Route([200, 2,0, 0], [255, 255, 0, 0], null, router1_eth1));
        router1.RoutingTable.SetDefaultGateway(router2_eth0.IpAdress, outgoingInterface: router1_eth1);

        WAN1.ConnectDevice(router2, router2_eth0);

        //WAN2 
        WAN2.ConnectDevice(router2, router2_eth1);
        
        router2.RoutingTable.AddRoute(new Route([212, 3,4, 0], [255, 255, 255, 0], null, router2_eth1));
        router2.RoutingTable.SetDefaultGateway(router3_eth0.IpAdress, outgoingInterface: router2_eth1);

        WAN2.ConnectDevice(router3, router3_eth0);

        //WAN3

        WAN3.ConnectDevice(router3, router3_eth1);

        router3.RoutingTable.AddRoute(new Route([212, 3,4, 0], [255, 255, 255, 0], null, router3_eth1));
        router3.RoutingTable.SetDefaultGateway(router4_eth0.IpAdress, outgoingInterface: router3_eth1);

        WAN3.ConnectDevice(router4, router4_eth0);

        //WAN4

        GoogleDatacenter.ConnectDevice(router4, router4_eth1);

        router4.RoutingTable.AddRoute(new Route([212, 3,4, 0], [255, 255, 255, 0], null, router4_eth1));

        GoogleDatacenter.ConnectDevice(target, target.Interface);



        me.SendPacket(target.Interface.IpAdress,[255,32,242,53,44,55,204,35]);


    }
}