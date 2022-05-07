namespace ServerSpooler;

using Shared;
using System.Net;
using System.Net.Sockets;

public static class Transciever {
    public static void LoadBalancerTranscieve(){
        const int PacketLenLen = 2;
        // 2bytes [Packet Len] byte.byte.byte.byte:2bytes [IP] byte [Player count]
        // All bytes unsigned
        byte[] buffer = new byte[1024];
        uint cursor = PacketLenLen;

        foreach (Tuple<ByteIP, uint> lobby in Program.LobbyServers){
            ArrayExtentions.Merge(buffer, lobby.Item1.IP, (int) cursor);
            cursor += 4;
            ArrayExtentions.Merge(buffer, lobby.Item1.Port, (int) cursor);
            cursor += 2;
            ArrayExtentions.Merge(buffer, new byte[] {(byte) lobby.Item2}, (int) cursor);
            cursor += 1;
        }

        buffer[0] = (byte) (cursor);
        buffer[1] = (byte) ((cursor)>>8);

        if (Listener.LoadBalancerSocket is null) {throw new NullReferenceException(); }
        Listener.LoadBalancerSocket.Send(ArrayExtentions.Slice(buffer, 0, (int) cursor));

        // Expecting 2 byte uint for response (queue len)
        byte[] recv = new byte[2];
        Listener.LoadBalancerSocket.Receive(recv);

        Console.WriteLine($"Load balancer queue len = {((uint) recv[0]) + ((uint) (recv[1]<<8))}");
    }
}