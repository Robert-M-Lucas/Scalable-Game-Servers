namespace LoadBalancer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SpoolerInterface 
{
    Socket SpoolerSocket;

    byte[] buffer = new byte[1024];
    int buffer_cursor = 0;

    public SpoolerInterface(string SpoolerIP, int SpoolerPort){
        IPAddress HostIpA = IPAddress.Parse(SpoolerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, SpoolerPort);

        SpoolerSocket = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        SpoolerSocket.Connect(RemoteEP);
        Console.WriteLine("Starting Spooler receive");
        SpoolerSocket.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null);
    }

    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        buffer_cursor += SpoolerSocket.EndReceive(ar);

        if (buffer_cursor >= 2) {
            uint packet_len = (uint) buffer[0] + (uint) (buffer[1]<<8);
            // Console.WriteLine($"Recieving packet from Spooler of len {packet_len}");
            if (buffer_cursor >= packet_len){
                // Console.WriteLine("Handle packet");
                // for (int i = 0; i < buffer_cursor; i++) {
                //     Console.Write((uint) buffer[i]); Console.Write(";");
                // }
                // Console.WriteLine();
                Console.WriteLine("Recieving servers");
                List<Tuple<ByteIP, uint>> NewLobbyServerList = new List<Tuple<ByteIP, uint>>();

                for (int i = 2; i < packet_len; i+=7) {
                    ByteIP ip = ByteIP.BytesToIP(ArrayExtentions.Slice(buffer, i, i+6));
                    uint fill_level = (uint) buffer[i+6];
                    Console.WriteLine($"Recieved server: {ip} - {fill_level}");
                    NewLobbyServerList.Add(new Tuple<ByteIP, uint>(ip, fill_level));
                }
                Console.WriteLine("Recieve server done");
                Program.LobbyServers = NewLobbyServerList;

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(buffer, (int) packet_len, buffer_cursor));
                buffer = new_buffer;
                buffer_cursor = buffer_cursor - (int) packet_len;

                uint num = 20321;
                SpoolerSocket.Send(new byte[] {(byte) num, (byte) (num>>8)});
            }
            else {
                // Console.WriteLine($"{buffer_cursor}/{packet_len} received");
            }
        }
        SpoolerSocket.BeginReceive(buffer, buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), null);
    }
}

