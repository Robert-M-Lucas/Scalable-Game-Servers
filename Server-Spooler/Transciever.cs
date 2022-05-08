namespace ServerSpooler;

using Shared;
using System.Net;
using System.Net.Sockets;

public static class Transciever {
    public static void LoadBalancerTranscieve() {
        const int PacketLenLen = 2;
        // 2bytes [Packet Len] byte.byte.byte.byte:2bytes [IP] byte [Player count]
        // All bytes unsigned
        byte[] buffer = new byte[1024];
        uint cursor = PacketLenLen;

        foreach (LobbyData lobby in Program.LobbyServers){
            ArrayExtentions.Merge(buffer, lobby.ip.IP, (int) cursor);
            cursor += 4;
            ArrayExtentions.Merge(buffer, lobby.ip.Port, (int) cursor);
            cursor += 2;
            ArrayExtentions.Merge(buffer, new byte[] {(byte) lobby.FillLevel}, (int) cursor);
            cursor += 1;
        }

        buffer[0] = (byte) (cursor);
        buffer[1] = (byte) ((cursor)>>8);

        if (Listener.LoadBalancerSocket is null) {throw new NullReferenceException(); }
        Listener.LoadBalancerSocket.Send(ArrayExtentions.Slice(buffer, 0, (int) cursor));

        // Expecting 2 byte uint for response (queue len)
        byte[] recv = new byte[2];
        Listener.LoadBalancerSocket.Receive(recv);
        Program.LoadBalancerQueueLen =((uint) recv[0]) + (((uint) (recv[1]<<8)));
        // Console.WriteLine($"Load balancer queue len = {Program.LoadBalancerQueueLen}");
    }

    public static void LobbyServersTranscieve() {
        Timer t = new Timer();
        foreach (LobbyData lobby in Program.LobbyServers) {
            if (lobby.socket is null) { Console.WriteLine("LOBBY SOCKET IS NULL!"); continue; }
            lobby.socket.Send(new byte[] {2, 0});
            byte[] recv = new byte[1];
            lobby.socket.Receive(recv);
            lobby.FillLevel = recv[0];
            lobby.response_time = t.GetMsAndReset();
        }
    }
}