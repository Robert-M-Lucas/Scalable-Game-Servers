namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;

public static class LoadBalancerClient {
    //Socket? Handler;

    public static ByteIP? Run(){
        return Connect();
        // NetworkController.ConnectToLobby()
    }

    static ByteIP? Connect(){
        byte[] server_buffer = new byte[1024];

        IPAddress HostIpA = IPAddress.Parse(Program.LoadBalancerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, Program.LoadBalancerPort);

        Socket Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            Handler.Connect(RemoteEP);
        }
        catch (SocketException e){
            Logger.LogError("Failed to connect to target Load Balancer " + Program.LoadBalancerIP + ":" + Program.LoadBalancerPort);
            Logger.LogError(e.ToString());
            return null;
        }

        if (Handler.RemoteEndPoint is null) {Logger.LogError("Remote end point is null"); return null; }

        Logger.LogDebug("Socket connected to " + Handler.RemoteEndPoint.ToString());

        Handler.Send(ClientConnectRequestPacket.Build(0, Program.ClientName, Program.Version));

        Logger.LogDebug("Waiting for response from load balance");

        try {
            while (true) {
                Logger.LogDebug("Starting Load Balancer recieve");
                byte[] packet_type = new byte[1];
                Handler.Receive(packet_type, 0, 1, 0);
                Logger.LogDebug("Recieved");

                switch (packet_type[0]){
                    case (byte) 0:
                        byte[] queue_pos_recv = new byte[2];
                        Handler.Receive(queue_pos_recv, 0, 2, 0);
                        Logger.LogDebug($"Position in queue: {((uint) queue_pos_recv[0]) + ((uint) (queue_pos_recv[1]<<8))}");
                        continue;
                    case (byte) 1:
                        byte[] ip_and_port = new byte[6];
                        Handler.Receive(ip_and_port, 0, 6, 0);
                        ByteIP ip = ByteIP.BytesToIP(ip_and_port);
                        Logger.LogDebug($"Being transfered to {ip.strIP}:{ip.iPort}");
                        return ip;
                    case (byte) 3:
                        Handler.Shutdown(SocketShutdown.Both);
                        Logger.LogWarning("Connection rejected - queue full");
                        return null;
                }
            }
        }
        catch (SocketException se) {
            Logger.LogError("Load Balancer disconnected client");
            Logger.LogError(se.ToString());
            return null;
        }
    }
}