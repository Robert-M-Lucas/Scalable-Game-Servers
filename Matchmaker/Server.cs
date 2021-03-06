namespace Matchmaker;

using Shared;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class Server {
    public ConcurrentQueue<Socket> Players = new ConcurrentQueue<Socket>();

    public Thread AcceptClientThread;

    public Server(){
        AcceptClientThread = new Thread(AcceptClients);
    }

    public void Start() {
        Logger.LogInfo("Matchmaker server start");
        AcceptClientThread.Start();
    }

    public void AcceptClients(){
        Logger.LogInfo("Matchmaker Client Accept Thread Start");

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
            Socket TempSocket = listener.Accept();
            uint queue_len = (uint) Players.Count;
            if (queue_len >= Program.MaxQueueLen){
                TempSocket.Send(new byte[] {(byte) 3}); // Connection reject
                Logger.LogWarning("Connection rejected, queue full");
                continue;
            }
            TempSocket.Send(new byte[] {(byte) 0, (byte) queue_len, (byte) (queue_len>>8)}); // Sending queue pos
            Players.Enqueue(TempSocket);
            Logger.LogInfo($"Client connected, queue len: {Players.Count}");
        }
    }

    // void UpdateClients(){
    //     
    // }

    public void TransferClient(ByteIP ip){
        Socket? socket;

        if (!Players.TryDequeue(out socket)) { Logger.LogError("Failed to dequeue client"); return; }

        byte[] to_send = new byte[7];
        to_send[0] = (byte) (uint) 1;
        ArrayExtentions.Merge(to_send, ip.IP, 1);
        ArrayExtentions.Merge(to_send, ip.Port, 5);

        try {
            socket.Send(to_send);
        } catch (SocketException se) {
            Logger.LogWarning("Failed to transfer client, probably due to client disconnect");
            Logger.LogWarning(se);
            socket.Shutdown(SocketShutdown.Both);
            return;
        }

        Logger.LogImportant($"Player sent to {ip.strIP}:{ip.iPort}");

        // Shutdown socket 1 second later
        Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => { 
            Logger.LogDebug("Shutting down client socket");
            socket.Shutdown(SocketShutdown.Both); 
        });

        Logger.LogDebug("Client transfer done");
    }

    ~Server(){Stop();}
    public void Stop(){
        Logger.LogWarning("Stopping Matchmaker");
        if (AcceptClientThread is not null) {try{AcceptClientThread.Interrupt();} catch (Exception e) {Console.WriteLine(e);} }
        Logger.LogInfo("Matchmaker stopped");
    }
}