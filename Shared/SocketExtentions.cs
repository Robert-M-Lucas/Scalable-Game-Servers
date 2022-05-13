namespace Shared;

using System.Net;
using System.Net.Sockets;

public static class SocketExtentions {
    public static bool SocketConnected(Socket s, int microseconds)
    {   
        bool part1 = s.Poll(microseconds, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if (part1 && part2) {
            return false;
        }
        else {
            return true;
        }
    }
}