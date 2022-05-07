namespace LoadBalancer;

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
        Console.WriteLine("Load Balancer server start");
        AcceptClientThread.Start();
    }

    public void AcceptClients(){
        Console.WriteLine("Load Balancer Client Accept Thread Start");

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
            Console.WriteLine("Waiting for client to connect");
            Socket TempSocket = listener.Accept();
            uint queue_len = (uint) Players.Count;
            if (queue_len >= Program.MaxQueueLen){
                TempSocket.Send(new byte[] {(byte) 3}); // Connection reject
                Console.WriteLine("Connection rejected, queue full");
                continue;
            }
            TempSocket.Send(new byte[] {(byte) 0, (byte) queue_len, (byte) (queue_len>>8)}); // Sending queue pos
            Players.Enqueue(TempSocket);
            Console.WriteLine($"Client connected, queue len: {Players.Count}");
        }
    }

    // void UpdateClients(){
    //     
    // }

    public void TransferClient(ByteIP ip){
        Socket? socket;
        if (!Players.TryDequeue(out socket)) { Console.WriteLine("Failed to dequeue"); return; }
        byte[] to_send = new byte[7];
        to_send[0] = (byte) (uint) 1;
        ArrayExtentions.Merge(to_send, ip.IP, 1);
        ArrayExtentions.Merge(to_send, ip.Port, 5);
        socket.Send(to_send);
        Console.WriteLine($"Player sent to {ip.strIP}:{ip.iPort}");
        socket.Shutdown(SocketShutdown.Both);
    }

    ~Server(){Stop();}
    public void Stop(){
        Console.WriteLine("Stopping Load Balancer");
        if (!(AcceptClientThread is null)) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        Console.WriteLine("Load balancer stopped");
    }
}