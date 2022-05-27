namespace GameServer;

using Shared;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

public class Server {
    public List<GamePlayer> Players = new List<GamePlayer>();

    public Thread AcceptClientThread;

    public Game? game;

    public Server(){
        AcceptClientThread = new Thread(AcceptClients);
    }

    public void Start() {
        Logger.LogInfo("Game Server start");
        AcceptClientThread.Start();
    }

    public void AcceptClients(){
        Logger.LogInfo("Lobby Server Client Accept Thread start");

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
            Logger.LogInfo("Waiting for client to connect");
            Socket temp_socket = listener.Accept();

            if (Players.Count >= Program.MaxGameServerFill) {
                temp_socket.Shutdown(SocketShutdown.Both);
                Logger.LogWarning("Client accepted and immediately kicked because server full");
            }

            Logger.LogInfo("Client accepted");

            // Start wait for connection data
            new Thread(() => FullClientAcceptThread(temp_socket)).Start();
        }
    }

    public void FullClientAcceptThread(Socket socket) {
        Logger.LogInfo("Client connected, waiting for connection data");

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
            Logger.LogWarning("Player failed to send connection data");
            socket.Shutdown(SocketShutdown.Both);
            return;
        }

        t.Join();

        GamePlayer player = new GamePlayer(socket, Encoding.ASCII.GetString(buffer));
        Players.Add(player);

        // Start recieving data from client
        socket.BeginReceive(player.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), player);
        Program.fill_level = (uint) Players.Count();
        Logger.LogImportant($"Player {player} connected. Player count: {Program.fill_level}/{Program.MaxGameServerFill}");
        ClientFullyAccepted(player);
    }

    private void ClientFullyAccepted(GamePlayer player) {
        if (game is null && Players.Count >= 2) {
            game = new Game(Players[0].ID, Players[1].ID, this);
            game.UpdateAllClients();
        }
    }

    private void RemovePlayer(GamePlayer player) {
        if (Players.Contains(player)) {
            Players.Remove(player);
        }
        else {
            return;
        }
        
        Program.fill_level = (uint) Players.Count();
        Logger.LogImportant($"Player {player} disconnected. Player count: {Program.fill_level}/{Program.MaxGameServerFill}");
        try {
            player.socket.Shutdown(SocketShutdown.Both);
        }
        catch (Exception e) {
            Logger.LogDebug("Error shutting down socket");
            Logger.LogDebug(e);
        }
    }

    private void ReadCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is null) { throw new NullReferenceException(); }
        GamePlayer player = (GamePlayer) ar.AsyncState;
        
        if (!SocketExtentions.SocketConnected(player.socket, 10000)) { 
            RemovePlayer(player);
            return;
        }
        
        player.buffer_cursor += player.socket.EndReceive(ar);

        if (player.buffer_cursor >= 2) {
            uint packet_len = (uint) player.buffer[0] + (uint) (player.buffer[1]<<8);

            Logger.LogDebug($"Recieving packet from player {player} of len {packet_len}");

            if (player.buffer_cursor >= packet_len){
                OnRecieve(player, ArrayExtentions.Slice(player.buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(player.buffer, (int) packet_len, player.buffer_cursor));
                player.buffer = new_buffer;
                player.buffer_cursor = player.buffer_cursor - (int) packet_len;
            }
            else {
                Logger.LogDebug($"{player.buffer_cursor}/{packet_len} received from {player.PlayerName}");
            }
        }
        player.socket.BeginReceive(player.buffer, player.buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), player);
    }

    void OnRecieve(GamePlayer player, byte[] data) {
        uint packet_type = (uint) data[2];

        if (game is null) {
            Logger.LogWarning($"Client {player} tried to send packet before game initialised");
            // Send game hasn't started packet to client
            return;
        }

        try {
            game.HandlePacket(packet_type, data, player);
        }
        catch (Exception e) {
            Logger.LogError("Error handling game packet");
            Logger.LogError(e);
        }
    }

    public void ResetGame() {
        Logger.LogImportant("Resetting game");
        while (Players.Count > 0) { RemovePlayer(Players[0]); }

        game = null;
    }

    ~Server(){Stop();}

    public void Stop(){
        Logger.LogWarning("Stopping Game Server");
        if (AcceptClientThread is not null) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        Logger.LogInfo("Lobby Game stopped");
    }
}