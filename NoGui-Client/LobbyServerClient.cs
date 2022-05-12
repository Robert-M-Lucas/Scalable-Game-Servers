namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class LobbyServerClient {
    Socket? Handler;

    public void Run(string IP, int Port){
        if (!Connect(IP, Port)) { NetworkController.LobbyConnectFailed(); }
        // NetworkController.ConnectToLobby()
    }

    bool Connect(string IP, int Port){
        byte[] server_buffer = new byte[1024];

        IPAddress HostIpA = IPAddress.Parse(IP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, Port);

        Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            Handler.Connect(RemoteEP);
        }
        catch (SocketException e){
            Program.logger.LogError("Failed to connect to target load balancer " + Program.LoadBalancerIP + ":" + Program.LoadBalancerPort);
            Program.logger.LogError(e.ToString());
            return false;
        }

        if (Handler.RemoteEndPoint is null) {Program.logger.LogError("Remote end point is null"); return false; }

        Program.logger.LogDebug("Socket connected to " + Handler.RemoteEndPoint.ToString());

        // Send connection data
        Handler.Send(ArrayExtentions.Merge(new byte[10], Encoding.ASCII.GetBytes(Program.ClientName)));

        try {
            while (true) {
                int c = ConsoleInputUtil.ChooseOption(new string[] {"Echo name", "Quit"});
                if (c == 0) {EchoName(Handler);}
                else if (c == 1) {Handler.Shutdown(SocketShutdown.Both); return true; }
            }
        }
        catch (SocketException se) {
            Program.logger.LogError("Load Balancer disconnected client");
            Program.logger.LogError(se.ToString());
            return false;
        }
    }

    void EchoName(Socket socket) {
        Program.logger.LogInfo("Sending echo request");
        socket.Send(new byte[] {3, 0, 1});
        byte[] buffer = new byte[13];
        Program.logger.LogInfo("Waiting for echo response");
        socket.Receive(buffer, 0, 13, 0);
        string name = Encoding.ASCII.GetString(ArrayExtentions.Slice(buffer, 3, 13));
        Program.logger.LogImportant($"Recieved name {name}");
    }
}