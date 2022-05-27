namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class LobbyServerClient {
    Socket? Handler;

    public int Run(string IP, int Port){
        return Connect(IP, Port);
        // NetworkController.ConnectToLobby()
    }

    int Connect(string IP, int Port){
        byte[] server_buffer = new byte[1024];

        IPAddress HostIpA = IPAddress.Parse(IP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, Port);

        Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            Handler.Connect(RemoteEP);
        }
        catch (SocketException e){
            Logger.LogError("Failed to connect to target Lobby Server " + IP + ":" + Port); 
            Logger.LogError(e.ToString());
            return 0;
        }

        if (Handler.RemoteEndPoint is null) {Logger.LogError("Remote end point is null"); return 0; }

        Logger.LogDebug("Socket connected to " + Handler.RemoteEndPoint.ToString());

        // Send connection data
        Handler.Send(ArrayExtentions.Merge(new byte[10], Encoding.ASCII.GetBytes(Program.ClientName)));

        try {
            while (true) {
                int c = ConsoleInputUtil.ChooseOption(new string[] {"Echo name", "Get counter", "Search for game", "Quit"});
                if (c == 0) {EchoName(Handler);}
                else if (c == 1) {GetCounter(Handler);}
                else if (c == 2) {Handler.Shutdown(SocketShutdown.Both); return 1; }
                else if (c == 3) {Handler.Shutdown(SocketShutdown.Both); return 2; }
            }
        }
        catch (SocketException se) {
            Logger.LogError("Lobby Server disconnected client");
            Logger.LogError(se.ToString());
            return 0;
        }
    }

    void EchoName(Socket socket) {
        Logger.LogDebug("Sending echo request");
        Timer t = new Timer();
        socket.Send(new byte[] {(byte) (uint) 3, (byte) (uint) 0, (byte) (uint) 1});
        byte[] buffer = new byte[13];
        Logger.LogDebug("Waiting for echo response");
        socket.Receive(buffer, 0, 13, 0);
        string name = Encoding.ASCII.GetString(ArrayExtentions.Slice(buffer, 3, 13));
        Logger.LogImportant($"Recieved name [{name}] in {t.GetMs()}ms");
    }

    void GetCounter(Socket socket) {
        Logger.LogDebug("Sending counter request");
        Timer t = new Timer();
        socket.Send(new byte[] {(byte) (uint) 3, (byte) (uint) 0, (byte) (uint) 2});
        byte[] buffer = new byte[4];
        Logger.LogDebug("Waiting for response");
        socket.Receive(buffer, 0, 4, 0);
        uint counter = buffer[3];
        Logger.LogImportant($"Recieved [{counter}] in {t.GetMs()}ms");
    }
}