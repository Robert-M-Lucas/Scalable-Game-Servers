namespace LoadBalancer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SILoadBalancer: SpoolerInterface
{
    public SILoadBalancer(string SpoolerIP, int SpoolerPort): base(SpoolerIP, SpoolerPort) {}

    public override void OnRecieve(byte[] message) {
        // Console.WriteLine();
        Console.WriteLine("Recieving servers");
        List<Tuple<ByteIP, uint>> NewLobbyServerList = new List<Tuple<ByteIP, uint>>();

        for (int i = 2; i < message.Length; i+=7) {
            ByteIP ip = ByteIP.BytesToIP(ArrayExtentions.Slice(message, i, i+6));
            uint fill_level = (uint) message[i+6];
            Console.WriteLine($"Recieved server: {ip} - {fill_level}");
            NewLobbyServerList.Add(new Tuple<ByteIP, uint>(ip, fill_level));
        }
        Console.WriteLine("Recieve server done");
        Program.LobbyServers = NewLobbyServerList;

        uint num = 20321;
        SpoolerSocket.Send(new byte[] {(byte) num, (byte) (num>>8)});
    }
}

