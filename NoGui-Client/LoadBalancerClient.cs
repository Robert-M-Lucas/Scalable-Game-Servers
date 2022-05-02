namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;

public class LoadBalancerClient{
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

        return true;
    }
}