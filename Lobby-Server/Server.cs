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
        Logger.LogInfo("Lobby Server start");
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
            Logger.LogInfo("Client accepted");

            // Start wait for connection data
            new Thread(() => FullClientAcceptThread(temp_socket)).Start();
        }
    }

    public void FullClientAcceptThread(Socket socket) {
        Logger.LogInfo("Client connected, waiting for connection data");

        byte[] buffer = new byte[32];

        Stopwatch s = new Stopwatch();
        s.Start();

        Thread t = new Thread(() => {
            socket.Receive(buffer, 0, 32, 0);
        });
        t.Start();

        while (s.ElapsedMilliseconds < Program.MaxClientConnectionDataSendTime){
            if (!t.IsAlive) { break; }
            Thread.Sleep(10);
        }
        
        // Thread hasn't exited, client didn't send connection data in required time
        if (t.IsAlive) { 
            t.Interrupt();
            Logger.LogWarning("Client failed to send connection data");
            socket.Shutdown(SocketShutdown.Both);
            return;
        }

        t.Join();

        string username = Encoding.ASCII.GetString(ArrayExtentions.Slice(buffer, 0, 16));
        string password = Encoding.ASCII.GetString(ArrayExtentions.Slice(buffer, 16, 32));

        DatabasePlayer? databasePlayer = Program.databaseInterface?.GetDatabasePlayer(username, password);

        if (databasePlayer is null) { socket.Shutdown(SocketShutdown.Both); return; }

        if (databasePlayer.PlayerID == -1) {
            socket.Send(new byte[] {(byte) (uint) 0});
            Thread.Sleep(10);
            socket.Shutdown(SocketShutdown.Both);
            return;
        } 

        byte[] to_reply = new byte[5];
        to_reply[0] = (byte) (uint) 1;
        to_reply = ArrayExtentions.Merge(to_reply, BitConverter.GetBytes(databasePlayer.PlayerCurrencyAmount), 1);

        socket.Send(to_reply);

        LobbyPlayer player = new LobbyPlayer(socket, username);
        player.currency_amount = databasePlayer.PlayerCurrencyAmount;
        Players.Add(player);

        // Start recieving data from client
        socket.BeginReceive(player.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), player);
        Program.fill_level = (uint) Players.Count();
        Logger.LogImportant($"Client {player} connected. Player count: {Program.fill_level}/{Program.MaxLobbyFill}");
        
    }

    private void RemovePlayer(LobbyPlayer player) {
        Players.Remove(player);
        Program.fill_level = (uint) Players.Count();
        Logger.LogImportant($"Player {player} disconnected. Player count: {Program.fill_level}/{Program.MaxLobbyFill}");
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
        LobbyPlayer player = (LobbyPlayer) ar.AsyncState;
        if (!SocketExtentions.SocketConnected(player.socket, 10000)) { 
            RemovePlayer(player);
            return;
        }
        
        player.buffer_cursor += player.socket.EndReceive(ar);

        if (player.buffer_cursor >= 2) {
            uint packet_len = (uint) player.buffer[0] + (uint) (player.buffer[1]<<8);

            Logger.LogInfo($"Recieving packet from player {player} of len {packet_len}");

            if (player.buffer_cursor >= packet_len){
                OnRecieve(player, ArrayExtentions.Slice(player.buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(player.buffer, (int) packet_len, player.buffer_cursor));
                player.buffer = new_buffer;
                player.buffer_cursor = player.buffer_cursor - (int) packet_len;
            }
            else {
                Logger.LogInfo($"{player.buffer_cursor}/{packet_len} received from {player}");
            }
        }
        player.socket.BeginReceive(player.buffer, player.buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), player);
    }

    void OnRecieve(LobbyPlayer player, byte[] data) {
        uint packet_type = (uint) data[2];

        switch (packet_type) {
            case 1:
                Logger.LogInfo($"Player {player} requested name echo, replying");
                byte[] name_buffer = new byte[19];
                name_buffer[0] = (byte) (uint) 19;
                name_buffer[1] = (byte) (uint) 0;
                name_buffer[2] = (byte) (uint) 1;
                ArrayExtentions.Merge(name_buffer, Encoding.ASCII.GetBytes(player.PlayerName), 3);
                player.socket.Send(name_buffer);
                break;
            case 2:
                Logger.LogInfo($"Player {player} requested counter, replying {player.player_counter_test}");
                byte[] counter_buffer = new byte[4];
                counter_buffer[0] = (byte) (uint) 4;
                counter_buffer[1] = (byte) (uint) 0;
                counter_buffer[2] = (byte) (uint) 2;
                counter_buffer[3] = (byte) player.player_counter_test;
                player.player_counter_test++;
                player.socket.Send(counter_buffer);
                break;
            default:
                Logger.LogError($"Player {player} requested requestID {packet_type} which doesn't exist");
                break;
        }
    }

    ~Server(){Stop();}
    public void Stop(){
        Logger.LogWarning("Stopping Lobby Server");
        if (AcceptClientThread is not null) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        foreach (LobbyPlayer player in Players) {
            player.socket.Shutdown(SocketShutdown.Both);
        }
        Logger.LogInfo("Lobby Server stopped");
    }
}