namespace Shared;

using System.Net;
using System.Net.Sockets;

public abstract class SpoolerInterface 
{
    protected Socket SpoolerSocket;

    byte[] buffer = new byte[1024];
    int buffer_cursor = 0;

    Action<string, string> OnSpoolerDisconnectAction;

    public SpoolerInterface(string SpoolerIP, int SpoolerPort, 
        Action<string, string>? onSpoolerDisconnectAction = null) {
        
        if (onSpoolerDisconnectAction is null) { onSpoolerDisconnectAction = (msg, exceptionString) => { throw new Exception(exceptionString); }; }

        OnSpoolerDisconnectAction = onSpoolerDisconnectAction;

        IPAddress HostIpA = IPAddress.Parse(SpoolerIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, SpoolerPort);

        SpoolerSocket = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        SpoolerSocket.Connect(RemoteEP);
        try { Logger.LogInfo("Starting Spooler receive"); } catch (LoggerNotAvailableException e) {Console.WriteLine(e.ToString());}
        SpoolerSocket.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null);
    }

    public abstract void OnRecieve(byte[] message);

    private void ReadCallback(IAsyncResult ar)
    {
        try {
            buffer_cursor += SpoolerSocket.EndReceive(ar);
        }
        catch (SocketException se) {
            OnSpoolerDisconnectAction("Server Spooler connection broken" ,se.ToString());
            return;
        }

        if (buffer_cursor >= 2) {
            uint packet_len = (uint) buffer[0] + (uint) (buffer[1]<<8);

            try {Logger.LogDebug($"Recieving packet from Spooler of len {packet_len}"); } catch (LoggerNotAvailableException e) {Console.WriteLine(e.ToString());}

            if (buffer_cursor >= packet_len){
                OnRecieve(ArrayExtentions.Slice(buffer, 0, (int) packet_len));

                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(buffer, (int) packet_len, buffer_cursor));
                buffer = new_buffer;
                buffer_cursor = buffer_cursor - (int) packet_len;
            }
            else {
                try {Logger.LogDebug($"{buffer_cursor}/{packet_len} received"); } catch (LoggerNotAvailableException e) {Console.WriteLine(e.ToString());}
            }
        }
        SpoolerSocket.BeginReceive(buffer, buffer_cursor, 1024, 0, new AsyncCallback(ReadCallback), null);
    }
}