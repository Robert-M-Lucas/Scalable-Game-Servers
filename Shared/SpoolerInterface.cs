namespace Shared;

using System.Net;
using System.Net.Sockets;

public abstract class SpoolerInterface 
{
    protected Socket SpoolerSocket;

    byte[] buffer = new byte[1024];
    int buffer_cursor = 0;

    Logger logger;

    Action<string> OnSpoolerDisconnect;

    public SpoolerInterface(string SpoolerIP, int SpoolerPort, Logger _logger, Action<string> onSpoolerDisconnect) {
        OnSpoolerDisconnect = onSpoolerDisconnect;
        logger = _logger;

        IPAddress HostIpA = IPAddress.Parse(SpoolerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, SpoolerPort);

        SpoolerSocket = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        SpoolerSocket.Connect(RemoteEP);
        logger.LogInfo("Starting Spooler receive");
        SpoolerSocket.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null);
    }

    public abstract void OnRecieve(byte[] message);

    private void ReadCallback(IAsyncResult ar)
    {
        try {
            buffer_cursor += SpoolerSocket.EndReceive(ar);
        }
        catch (SocketException se) {
            OnSpoolerDisconnect(se.Message);
        }

        if (buffer_cursor >= 2) {
            uint packet_len = (uint) buffer[0] + (uint) (buffer[1]<<8);

            logger.LogDebug($"Recieving packet from Spooler of len {packet_len}");

            if (buffer_cursor >= packet_len){
                OnRecieve(ArrayExtentions.Slice(buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(buffer, (int) packet_len, buffer_cursor));
                buffer = new_buffer;
                buffer_cursor = buffer_cursor - (int) packet_len;
            }
            else {
                logger.LogDebug($"{buffer_cursor}/{packet_len} received");
            }
        }
        SpoolerSocket.BeginReceive(buffer, buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), null);
    }
}