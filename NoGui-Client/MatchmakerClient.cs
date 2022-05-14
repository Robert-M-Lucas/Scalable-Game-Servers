namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;

public static class MatchmakerClient {
    //Socket? Handler;

    public static ByteIP? Run() {
        return Connect();
        // NetworkController.ConnectToLobby()
    }

    static ByteIP? Connect() {
        byte[] server_buffer = new byte[1024];

        IPAddress HostIpA = IPAddress.Parse(Program.MatchmakerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, Program.MatchmakerPort);

        Socket Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            Handler.Connect(RemoteEP);
        }
        catch (SocketException e){
            Program.logger.LogError("Failed to connect to target Matchmaker " + Program.MatchmakerIP + ":" + Program.MatchmakerPort);
            Program.logger.LogError(e.ToString());
            return null;
        }

        if (Handler.RemoteEndPoint is null) {Program.logger.LogError("Remote end point is null"); return null; }

        Program.logger.LogDebug("Socket connected to " + Handler.RemoteEndPoint.ToString());

        Handler.Send(ClientConnectRequestPacket.Build(0, Program.ClientName, Program.Version));

        Program.logger.LogDebug("Waiting for response from Matchmaker");

        try {
            while (true) {
                Program.logger.LogDebug("Starting Matchmaker recieve");
                byte[] packet_type = new byte[1];
                Handler.Receive(packet_type, 0, 1, 0);
                Program.logger.LogDebug("Recieved");

                switch (packet_type[0]){
                    case (byte) 0:
                        byte[] queue_pos_recv = new byte[2];
                        Handler.Receive(queue_pos_recv, 0, 2, 0);
                        Program.logger.LogInfo($"Position in queue: {((uint) queue_pos_recv[0]) + ((uint) (queue_pos_recv[1]<<8))}");
                        continue;
                    case (byte) 1:
                        byte[] ip_and_port = new byte[6];
                        Handler.Receive(ip_and_port, 0, 6, 0);
                        ByteIP ip = ByteIP.BytesToIP(ip_and_port);
                        Program.logger.LogInfo($"Being transfered to {ip.strIP}:{ip.iPort}");
                        return ip;
                    case (byte) 3:
                        Handler.Shutdown(SocketShutdown.Both);
                        Program.logger.LogInfo("Connection rejected - queue full");
                        return null;
                }
            }
        }
        catch (SocketException se) {
            Program.logger.LogError("Matchmaker disconnected client");
            Program.logger.LogError(se.ToString());
            return null;
        }
    }
}