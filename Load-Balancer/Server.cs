namespace LoadBalancer;

using Shared;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class Server{
    public Socket[] Players = new Socket[Program.MaxQueueLen];

    public Thread AcceptClientThread;

    public Server(){
        AcceptClientThread = new Thread(AcceptClients);
    }

    public void Start() {
        Console.WriteLine("Load Balancer start, press enter to stop");
        AcceptClientThread.Start();
        Console.ReadLine();
        Stop();
    }

    public void AcceptClients(){
        Console.WriteLine("Load Balancer Client Accept Thread Start");

        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Program.Port);

        try
        {
            Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            listener.Bind(localEndPoint);

            listener.Listen(100);

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                Socket Handler = listener.Accept();
                Console.WriteLine("Client connecting");

                // Incoming data from the client.
                byte[] rec_bytes = new byte[1024];
                int total_rec = 0;

                while (total_rec < 4)
                {
                    byte[] partial_bytes = new byte[1024];
                    int bytesRec = Handler.Receive(rec_bytes);

                    total_rec += bytesRec;

                    // string _out2 = "";
                    // for (int i = 0; i < rec_bytes.Length; i++){
                    //     _out2 += rec_bytes[i].ToString() + ":";
                    // }
                    // Debug.Log(_out2);

                    Tuple<byte[], int> cleared = ArrayExtentions.ClearEmpty(rec_bytes);
                    rec_bytes = cleared.Item1;
                    total_rec -= cleared.Item2;

                    ArrayExtentions.Merge(
                        rec_bytes,
                        ArrayExtentions.Slice(partial_bytes, 0, bytesRec),
                        total_rec
                    );
                }

                int packet_len = PacketManager.GetPacketLength(
                    ArrayExtentions.Slice(rec_bytes, 0, 4)
                );

                while (total_rec < packet_len)
                {
                    byte[] partial_bytes = new byte[1024];
                    int bytesRec = Handler.Receive(partial_bytes);

                    total_rec += bytesRec;
                    ArrayExtentions.Merge(
                        rec_bytes,
                        ArrayExtentions.Slice(partial_bytes, 0, bytesRec),
                        total_rec
                    );
                }

                ClientConnectRequestPacket initPacket = new ClientConnectRequestPacket(
                    PacketManager.Decode(ArrayExtentions.Slice(rec_bytes, 0, packet_len))
                );

                // Version mismatch
                if (initPacket.Version != Program.Version)
                {
                    Handler.Shutdown(SocketShutdown.Both);
                    continue;
                }

                AddPlayer(Handler);
            }
        }
        catch (ThreadInterruptedException) { Console.WriteLine("Accept thread terminating"); }
        catch (Exception e)
        {
            Console.WriteLine("Accept thread error");
            Console.WriteLine(e);
        }
    }

    void AddPlayer(Socket player){
        for (int i = 0; i < Players.Length; i++){
            if (Players[i] is null){
                Players[i] = player;
                return;
            }
        }

        player.Shutdown(SocketShutdown.Both);
    }

    void UpdateClients(){

    }

    void TransferClients(){

    }

    ~Server(){Stop();}
    public void Stop(){
        Console.WriteLine("Stopping Load Balancer");
        if (!(AcceptClientThread is null)) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        Console.WriteLine("Load balancer stopped");
    }
}