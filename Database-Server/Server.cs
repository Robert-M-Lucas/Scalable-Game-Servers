namespace DatabaseServer;

using Shared;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client {
    public byte[] buffer = new byte[1024];
    public int buffer_cursor;

    public Socket socket;

    public Client(Socket _socket) {
        socket = _socket;
    }

    public override string ToString()
    {
        if (socket.RemoteEndPoint is null || socket.LocalEndPoint is null) { return "null socket"; }

        return $"[{IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString())}:{((IPEndPoint)socket.RemoteEndPoint).Port.ToString()} -> " +
        $"{IPAddress.Parse(((IPEndPoint)socket.LocalEndPoint).Address.ToString())}:{((IPEndPoint)socket.LocalEndPoint).Port.ToString()}]";
    }
}

public class Server {
    public List<Client> Clients = new List<Client>();

    public Thread AcceptClientThread;

    public string ipAddress;
    public int Port;

    public Server(string ip_address, int port){
        AcceptClientThread = new Thread(AcceptClients);
        ipAddress = ip_address;
        Port = port;
    }

    public void Start() {
        Logger.LogInfo("Database Server start");
        AcceptClientThread.Start();
    }

    public void AcceptClients(){
        Logger.LogInfo("Database Server Client Accept Thread start");

        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

        Socket listener = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

            listener.Bind(localEndPoint);
            listener.Listen(100);

        while (true) {
            Logger.LogInfo("Waiting for client to connect");
            Socket socket = listener.Accept();
            Client client = new Client(socket);
            Logger.LogImportant($"Client accepted: {client}");
            socket.BeginReceive(client.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), client);
        }
    }

    private void ReadCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is null) { throw new NullReferenceException(); }
        Client client = (Client) ar.AsyncState;
        
        try {
            client.buffer_cursor += client.socket.EndReceive(ar);
        }
        catch (SocketException se) {
            Logger.LogError($"Client: {client} disconnected due to error:");
            Logger.LogError(se);
            client.socket.Shutdown(SocketShutdown.Both);
            Clients.Remove(client);
            return;
        }
        
        if (!SocketExtentions.SocketConnected(client.socket, 10000)) {
            Logger.LogError($"Client: {client} disconnected due to not responding to poll");
            client.socket.Shutdown(SocketShutdown.Both);
            Clients.Remove(client);
            return;
        }

        if (client.buffer_cursor >= 2) {
            uint packet_len = (uint) client.buffer[0] + (uint) (client.buffer[1]<<8);

            Logger.LogInfo($"Recieving packet from client {client} of len {packet_len}");

            if (client.buffer_cursor >= packet_len){
                OnRecieve(client, ArrayExtentions.Slice(client.buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(client.buffer, (int) packet_len, client.buffer_cursor));
                client.buffer = new_buffer;
                client.buffer_cursor = client.buffer_cursor - (int) packet_len;
            }
            else {
                Logger.LogInfo($"{client.buffer_cursor}/{packet_len} received from {client}");
            }
        }
        client.socket.BeginReceive(client.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), client);
    }

    void OnRecieve(Client client, byte[] data) {
        uint packet_type = (uint) data[2];

        switch (packet_type) {
            case 0:
                DatabasePlayer dbPlayer = DatabaseCommands.GetOrAddPlayer(Encoding.ASCII.GetString(ArrayExtentions.Slice(data, 4, 20)), Encoding.ASCII.GetString(ArrayExtentions.Slice(data, 20, 36)));
                if (dbPlayer.PlayerID == -1) {
                    byte[] reply = new byte[2];
                    reply[0] = (byte) (uint) 1;
                    client.socket.Send(reply);
                }
                else {
                    byte[] reply = new byte[10];
                    reply = ArrayExtentions.Merge(reply, BitConverter.GetBytes(dbPlayer.PlayerID), 2);
                    reply = ArrayExtentions.Merge(reply, BitConverter.GetBytes(dbPlayer.PlayerCurrencyAmount), 6);
                    client.socket.Send(reply);
                }
                break;
            default:
                Logger.LogError($"Client {client} requested requestID {packet_type} which doesn't exist");
                break;
        }
    }

    ~Server(){Stop();}
    public void Stop(){
        Logger.LogWarning("Stopping Database Server");
        if (AcceptClientThread is not null) {try{AcceptClientThread.Interrupt();}catch(Exception e){Console.WriteLine(e);}}
        foreach (Client client in Clients) {
            client.socket.Shutdown(SocketShutdown.Both);
        }
        Logger.LogInfo("Database Server stopped");
    }
}