namespace GameServer;

using System.Net;
using System.Net.Sockets;

public class GamePlayer {
    private static int player_id_counter = 0;

    public int ID;
    public Socket socket;

    public string PlayerName;

    # region net
    public int buffer_cursor;
    public byte[] buffer = new byte[1024];
    # endregion

    // TODO: Remove this after testing
    public uint player_counter_test = 0;

    public GamePlayer(Socket _socket, string player_name) {
        socket = _socket;
        PlayerName = player_name;
        ID = player_id_counter;
        player_counter_test++;
    }

    public override string ToString()
    {
        if (socket.RemoteEndPoint is null) { return $"[{PlayerName}]:[Null]"; }
        IPEndPoint remoteEndPoint = (IPEndPoint) socket.RemoteEndPoint;
        return $"[{PlayerName}]:[{remoteEndPoint.Address}:{remoteEndPoint.Port}]";
    }
}