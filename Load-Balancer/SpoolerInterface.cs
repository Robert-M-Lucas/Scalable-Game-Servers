namespace LoadBalancer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SpoolerInterface 
{
    Socket SpoolerSocket;

    byte[] buffer = new byte[1024];
    byte[] long_buffer = new byte[1024];

    public SpoolerInterface(string SpoolerIP, int SpoolerPort){
        IPAddress HostIpA = IPAddress.Parse(SpoolerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, SpoolerPort);

        SpoolerSocket = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        SpoolerSocket.Connect(RemoteEP);
        SpoolerSocket.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null);
    }

    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        int bytesRead = SpoolerSocket.EndReceive(ar);

        if (bytesRead > 0)
        {
            ArrayExtentions.Merge(
                CurrentPlayer.long_buffer,
                CurrentPlayer.buffer,
                CurrentPlayer.long_buffer_size
            );
            CurrentPlayer.long_buffer_size += bytesRead;

            ReprocessBuffer:

            if (
                CurrentPlayer.current_packet_length == -1
                && CurrentPlayer.long_buffer_size >= PacketBuilder.PacketLenLen
            )
            {
                CurrentPlayer.current_packet_length = PacketBuilder.GetPacketLength(
                    CurrentPlayer.long_buffer
                );
            }

            if (CurrentPlayer.current_packet_length != -1
                && long_buffer_size >= CurrentPlayer.current_packet_length)
            {
                ArrayExtentions.Slice(long_buffer, 0, current_packet_length);

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(
                    new_buffer,
                    ArrayExtentions.Slice(
                        long_buffer,
                        current_packet_length,
                        1024
                    ),
                    0
                );
                long_buffer = new_buffer;
                long_buffer_size -= current_packet_length;
                current_packet_length = -1;
                if (long_buffer_size > 0)
                {
                    goto ReprocessBuffer;
                }
            }
        }
        SpoolerSocket.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null); // Listen again
    }   
}

