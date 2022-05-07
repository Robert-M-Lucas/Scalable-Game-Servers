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
            Console.WriteLine($"Recieving packet from Spooler of len {packet_len}");
            if (buffer_cursor >= packet_len){
                Console.WriteLine("Handle packet");
                for (int i = 0; i < buffer_cursor; i++) {
                    Console.Write((uint) buffer[i]); Console.Write(";");
                }
                Console.WriteLine();
                buffer_cursor = 0;
                buffer = new byte[1024];

                uint num = 20321;
                SpoolerSocket.Send(new byte[] {(byte) num, (byte) (num>>8)});
            }
            else {
                Console.WriteLine($"{buffer_cursor}/{packet_len} received");
            }
        }
        SpoolerSocket.BeginReceive(buffer, buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), null);
    }
}

