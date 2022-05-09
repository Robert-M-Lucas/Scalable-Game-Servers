namespace LoadBalancer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SILoadBalancer: SpoolerInterface
{
    public SILoadBalancer(string SpoolerIP, int SpoolerPort, Logger _logger): base(SpoolerIP, SpoolerPort, _logger) {}

    public override void OnRecieve(byte[] message) {
        // Console.WriteLine();
        Program.logger.LogInfo("Recieving servers from spooler");
        List<Tuple<ByteIP, uint>> NewLobbyServerList = new List<Tuple<ByteIP, uint>>();

        for (int i = 2; i < message.Length; i+=7) {
            ByteIP ip = ByteIP.BytesToIP(ArrayExtentions.Slice(message, i, i+6));
            uint fill_level = (uint) message[i+6];
            Program.logger.LogDebug($"Recieved server: {ip} - {fill_level}");
            NewLobbyServerList.Add(new Tuple<ByteIP, uint>(ip, fill_level));
        }
        Program.logger.LogInfo("Recieve server done");
        Program.LobbyServers = NewLobbyServerList;

        Program.logger.LogInfo("Replying to spooler");
        uint num = 0;
        if (!(Program.server is null)) { num = (uint) Program.server.Players.Count; }
        SpoolerSocket.Send(new byte[] {(byte) num, (byte) (num>>8)});
    }
}

