namespace LobbyServer;

using Shared;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

public class Server {
    public List<LobbyPlayer> Players = new List<LobbyPlayer>();

    public Thread AcceptClientThread;

    public Server(){
        AcceptClientThread = new Thread(AcceptClients);
    }

    public void Start() {
        Program.logger.LogInfo("Lobby Server start");
        AcceptClientThread.Start();
    }

    public void AcceptClients(){
        Program.logger.LogInfo("Lobby Server Client Accept Thread start");

        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Program.Port);

        Socket listener = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

            listener.Bind(localEndPoint);
            listener.Listen(100);

        while (!Program.exit) {
            Program.logger.LogInfo("Waiting for client to connect");
            Socket temp_socket = listener.Accept();

            // Start wait for connection data
            new Thread(() => FullClientAcceptThread(temp_socket)).Start();
        }
    }

    public void FullClientAcceptThread(Socket socket) {
        Program.logger.LogInfo("Client connected, waiting for connection data");

        byte[] buffer = new byte[10];

        Stopwatch s = new Stopwatch();
        s.Start();

        Thread t = new Thread(() => {
            socket.Receive(buffer, 0, 10, 0);
        });
        t.Start();

        while (s.ElapsedMilliseconds < Program.MaxClientConnectionDataSendTime){
            if (!t.IsAlive) { break; }
            Thread.Sleep(10);
        }
        
        // Thread hasn't exited, client didn't send connection data in required time
        if (t.IsAlive) { 
            t.Interrupt();
            Program.logger.LogWarning("Client failed to send connection data");
            socket.Shutdown(SocketShutdown.Both);
            return;
        }

        t.Join();

        LobbyPlayer player = new LobbyPlayer(socket, Encoding.ASCII.GetString(buffer));
        Players.Add(player);

        // Start recieving data from client
        socket.BeginReceive(player.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), player);

        Program.logger.LogInfo($"Client connected, player count: {Players.Count}");
        Program.fill_level = (uint) Players.Count();
    }

    private void ReadCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is null) { throw new NullReferenceException(); }
        LobbyPlayer player = (LobbyPlayer) ar.AsyncState;

        player.buffer_cursor += player.socket.EndReceive(ar);

        if (player.buffer_cursor >= 2) {
            uint packet_len = (uint) player.buffer[0] + (uint) (player.buffer[1]<<8);

            Program.logger.LogDebug($"Recieving packet from player {player.PlayerName} of len {packet_len}");

            if (player.buffer_cursor >= packet_len){
                OnRecieve(player, ArrayExtentions.Slice(player.buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(player.buffer, (int) packet_len, player.buffer_cursor));
                player.buffer = new_buffer;
                player.buffer_cursor = player.buffer_cursor - (int) packet_len;
            }
            else {
                Program.logger.LogDebug($"{player.buffer_cursor}/{packet_len} received from {player.PlayerName}");
            }
        }
        player.socket.BeginReceive(player.buffer, player.buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), null);
    }

    void OnRecieve(LobbyPlayer player, byte[] data) {
        
    }

    ~Server(){Stop();}
    public void Stop(){
        Program.logger.LogWarning("Stopping Lobby Server");
        if (!(AcceptClientThread is null)) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        Program.logger.LogInfo("Lobby Server stopped");
    }
}