namespace Shared;

using System.Net;
using System.Net.Sockets;
using System.Text;

public class DatabaseDisconnectException : Exception
{
    public DatabaseDisconnectException() { }

    public DatabaseDisconnectException(string message) : base(message) { }

    public DatabaseDisconnectException(string message, Exception inner) : base(message, inner) { }
}

public class DatabaseInterface {
    Socket DatabaseSocket;

    Action<string> OnDatabaseDisconnect;

    public DatabaseInterface(string DatabaseIP, int DatabasePort, 
        Action<string>? onDatabaseDisconnectAction = null) {
        
        if (onDatabaseDisconnectAction is null) { onDatabaseDisconnectAction = (msg) => { throw new Exception(msg); }; }

        OnDatabaseDisconnect = onDatabaseDisconnectAction;

        IPAddress HostIpA = IPAddress.Parse(DatabaseIP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, DatabasePort);

        DatabaseSocket = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        DatabaseSocket.Connect(RemoteEP);
        Logger.LogInfo("Connected to Database");
    }

    public void Shutdown(bool callOnDisconnect = false) {
        DatabaseSocket.Shutdown(SocketShutdown.Both);
        if (callOnDisconnect) { OnDatabaseDisconnect(""); }
    }

    public DatabasePlayer GetDatabasePlayer(string username, string password) {
        byte[] username_bytes = ArrayExtentions.Merge(new byte[16], Encoding.ASCII.GetBytes(username)); 
        byte[] password_bytes = ArrayExtentions.Merge(new byte[16], Encoding.ASCII.GetBytes(password));

        byte[] full_packet = new byte[36];
        full_packet[0] = (byte) (uint) 36;
        full_packet[1] = (byte) (uint) 0;
        full_packet[2] = (byte) (uint) 0;
        full_packet[3] = (byte) (uint) 0;
        full_packet = ArrayExtentions.Merge(full_packet, username_bytes, 4);
        full_packet = ArrayExtentions.Merge(full_packet, password_bytes, 20);

        byte[] response_type = new byte[2];
        try {
            DatabaseSocket.Send(full_packet);
            DatabaseSocket.Receive(response_type, 0, 2, 0);
        }
        catch (SocketException se) {
            OnDatabaseDisconnect(se.ToString());
            throw new DatabaseDisconnectException(se.ToString());
        }

        if ((uint) response_type[0] == 0) {
            byte[] full_response = new byte[8];
            DatabaseSocket.Receive(full_response);
            ArrayExtentions.PrintByteArr(full_response);
            
            // Console.WriteLine(ArrayExtentions.ByteArrToUint(ArrayExtentions.Slice(full_response, 0, 4)))

            return new DatabasePlayer(username, password, BitConverter.ToInt32(ArrayExtentions.Slice(full_response, 0, 4)), (int) BitConverter.ToInt32(ArrayExtentions.Slice(full_response, 4, 8)));
        }
        else {
            return new DatabasePlayer("", "", -1, 0);
        }
    }
}