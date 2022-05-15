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

    public Server(){
        AcceptClientThread = new Thread(AcceptClients);
    }

    public void Start() {
        Program.logger.LogInfo("Game Server start");
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
            Program.logger.LogInfo("Client accepted");

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
            Program.logger.LogWarning("Player failed to send connection data");
            socket.Shutdown(SocketShutdown.Both);
            return;
        }

        t.Join();

        GamePlayer player = new GamePlayer(socket, Encoding.ASCII.GetString(buffer));
        Players.Add(player);

        // Start recieving data from client
        socket.BeginReceive(player.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), player);
        Program.fill_level = (uint) Players.Count();
        Program.logger.LogInfo($"Player {player} connected. Player count: {Program.fill_level}/{Program.MaxGameServerFill}");
        
    }

    private void RemovePlayer(GamePlayer player) {
        Players.Remove(player);
        Program.fill_level = (uint) Players.Count();
        Program.logger.LogInfo($"Player {player} disconnected. Player count: {Program.fill_level}/{Program.MaxGameServerFill}");
        try {
            player.socket.Shutdown(SocketShutdown.Both);
        }
        catch (Exception e) {
            Program.logger.LogDebug("Error shutting down socket");
            Program.logger.LogDebug(e);
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

            Program.logger.LogInfo($"Recieving packet from player {player} of len {packet_len}");

            if (player.buffer_cursor >= packet_len){
                OnRecieve(player, ArrayExtentions.Slice(player.buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(player.buffer, (int) packet_len, player.buffer_cursor));
                player.buffer = new_buffer;
                player.buffer_cursor = player.buffer_cursor - (int) packet_len;
            }
            else {
                Program.logger.LogInfo($"{player.buffer_cursor}/{packet_len} received from {player.PlayerName}");
            }
        }
        player.socket.BeginReceive(player.buffer, player.buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), player);
    }

    void OnRecieve(GamePlayer player, byte[] data) {
        uint packet_type = (uint) data[2];

        switch (packet_type) {
            case 1:
                Program.logger.LogInfo($"Player {player} requested name echo, replying");
                byte[] name_buffer = new byte[13];
                name_buffer[0] = (byte) (uint) 13;
                name_buffer[1] = (byte) (uint) 0;
                name_buffer[2] = (byte) (uint) 1;
                ArrayExtentions.Merge(name_buffer, Encoding.ASCII.GetBytes(player.PlayerName), 3);
                player.socket.Send(name_buffer);
                break;
            case 2:
                Program.logger.LogInfo($"Player {player} requested counter, replying {player.player_counter_test}");
                byte[] counter_buffer = new byte[4];
                counter_buffer[0] = (byte) (uint) 4;
                counter_buffer[1] = (byte) (uint) 0;
                counter_buffer[2] = (byte) (uint) 2;
                counter_buffer[3] = (byte) player.player_counter_test;
                player.player_counter_test++;
                player.socket.Send(counter_buffer);
                break;
            default:
                Program.logger.LogError($"Player {player} requested requestID {packet_type} which doesn't exist");
                break;
        }
    }

    ~Server(){Stop();}
    public void Stop(){
        Program.logger.LogWarning("Stopping Game Server");
        if (AcceptClientThread is not null) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        Program.logger.LogInfo("Lobby Game stopped");
    }
}