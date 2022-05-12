namespace LobbyServer;

using System.Net;
using System.Net.Sockets;

public class LobbyPlayer {
    public Socket socket;

    public string PlayerName;

    # region net
    public int buffer_cursor;
    public byte[] buffer = new byte[1024];
    # endregion

    // TODO: Remove this after testing
    public int player_counter_test = 0;

    public LobbyPlayer(Socket _socket, string player_name) {
        socket = _socket;
        PlayerName = player_name;
    }

    public override string ToString()
    {
        if (socket.RemoteEndPoint is null) { return $"[{PlayerName}]:[Null]"; }
        IPEndPoint remoteEndPoint = (IPEndPoint) socket.RemoteEndPoint;
        return $"[{PlayerName}]:[{remoteEndPoint.Address}:{remoteEndPoint.Port}]";
    }
}