# Network Simulator with Ethernet, ARP, IP, TCP & UDP

A collection of network protocol simulations and algorithm implementations in C#, created for educational purposes.
This project explores low-level networking concepts, including Ethernet frame handling, ARP resolution,broadcasting, IP packet processing, and simulation of transport behavior. It demonstrates how different layers of network communication interact and how protocols work together in a stack.


## Features
- Implementation of **Ethernet frame structures and parsing**
- **Routing Tables** in each device, that provides real-life behaviour with finding **next hop** in the network
- Simulation of **ARP (Address Resolution Protocol)**
- Processing of **IP packets**
- Modular code structure for learning and experimentation

## Technologies
This project uses:
- **C# (.NET)** — core programming language for simulation logic  
- **.NET 9.0** or higher — latest .NET runtime for building and running  
- Object-oriented and modular architecture for clean separation of protocols

## Project Structure

**Protocols/** — implementations of Ethernet, ARP, IP, TCP and UDP logic <br>
**Managers/** — simulation control and workflow <br>
**Devices/** — simulated network node and interface models <br>
**Infrastructure/** — core helpers and utilities <br>
**Constants/** — reusable protocol constants and definitions <br>

## Running a Packet Simulation

The network topology is defined directly in `Program.cs`.
To simulate packet transmission, the following steps are required:

**1. Create network segments (LAN / WAN).**
```csharp
Network LAN = new Network("LAN");
Network WAN1 = new Network("WAN1");
```
> _Note: Network is responsible for broadcast(ARP,Ethernet)._

**2. Create hosts with MAC address, IP address, and subnet mask.**
```csharp
Host me = new Host(
    Name: "My computer",
    MacAdress: [11,22,33,44,55,66],
    IpAdress: [192,23,1,10],
    mask: [255,255,255,0]
);
```
> _Note: Each host has only one network interface and one routing table in this simulation._

**3. Define routers and configure their network interfaces.**
Router:
- does not have one IP
- has many interfaces (eth0, eth1, …)

```csharp
Router router1 = new Router("Home Gateway");

var eth0 = router1.AddInterface(
    "eth0",
    IpAdress: [192,23,1,1],
    MacAdress: [...],
    mask: [255,255,255,0]
);
```

4. Connect devices to networks to simulate physical links.

```csharp
LAN.ConnectDevice(me, me.Interface);
LAN.ConnectDevice(router1, router1_eth0);
```

5. Configure routing tables and default gateways for hosts and routers.

Host
- route to local network
- default gateway → router

```csharp
me.RoutingTable.AddRoute(
    new Route([192,23,0,0], [255,255,0,0], null, me.Interface)
);

me.RoutingTable.SetDefaultGateway(
    router1_eth0.IpAdress,
    outgoingInterface: me.Interface
);
```

6. Define a destination host (Example: Google).

```csharp
Host target = new Host(
    Name: "Google",
    MacAdress: [...],
    IpAdress: [8,8,8,8],
    mask: [255,255,255,0]
);
```

7. Send a packet using `SendPacket(destinationIp, payload)`.

```csharp
me.SendPacket(
    target.Interface.IpAdress,
    [255,32,242,53,44,55,204,35]
);
```


## Summary & What I Learned
This project improved my understanding of how real network stacks operate. By reading about network behaviour and all mechanisms behind it, in a book by Rickard Stevens
TCP/IP Illustrated, I tried to better understand it by implementing it in code.
<br> When I implemented protocols and payload decoding to summarize what properties they own, I realized I can try to simulate all of the behaviurs of the network. I added methods like: 
resolving IP adressess to MAC with **ARP requests**, getting next hops with Routing Tables implementing **Longest Prefix match** algorithm or transmitting packets with both **unicast and broadcast** adressing modes.
<br> Most difficult concept, not maybe to understand as it is, but to implement, was managing many interfaces in devices. Connecting them was a bit confusing at first, beacuse of a need to re-build whole architecture, which was originally designed to only connect between devices and not their interfaces.






