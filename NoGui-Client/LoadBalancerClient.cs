namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;

public class LoadBalancerClient {
    Socket? Handler;

    public void Run(){
        if (!Connect()) { NetworkController.LoadBalancerConnectFailed(); }
        // NetworkController.ConnectToLobby()
    }

    bool Connect(){
        byte[] server_buffer = new byte[1024];

        IPAddress HostIpA = IPAddress.Parse(Program.LoadBalancerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, Program.LoadBalancerPort);

        Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            Handler.Connect(RemoteEP);
        }
        catch (SocketException e){
            Console.WriteLine("Failed to connect to target");
            Console.WriteLine(e);
            return false;
        }

        if (Handler.RemoteEndPoint is null) {Console.WriteLine("Remote end point is null"); return false; }

        Console.WriteLine("Socket connected to " + Handler.RemoteEndPoint.ToString());

        Handler.Send(ClientConnectRequestPacket.Build(0, Program.ClientName, Program.Version));

        Console.WriteLine("Waiting for response");

        while (true) {
            Console.WriteLine("Starting recieve");
            byte[] packet_type = new byte[1];
            Handler.Receive(packet_type, 0, 1, 0);
            Console.WriteLine("Recieved");

            switch (packet_type[0]){
                case (byte) 0:
                    byte[] queue_pos_recv = new byte[2];
                    Handler.Receive(queue_pos_recv, 0, 2, 0);
                    Console.WriteLine($"Position in queue: {((uint) queue_pos_recv[0]) + ((uint) (queue_pos_recv[1]<<8))}");
                    continue;
                case (byte) 1:
                    byte[] ip_and_port = new byte[6];
                    Handler.Receive(ip_and_port, 0, 6, 0);
                    ByteIP ip = ByteIP.BytesToIP(ip_and_port);
                    Console.WriteLine($"Being transfered to {ip.strIP}:{ip.iPort}");
                    return true;
                case (byte) 3:
                    Handler.Shutdown(SocketShutdown.Both);
                    Console.WriteLine("Connection rejected - queue full");
                    return false;
            }
        }
    }
}