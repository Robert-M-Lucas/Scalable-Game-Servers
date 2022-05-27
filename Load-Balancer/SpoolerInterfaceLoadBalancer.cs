namespace LoadBalancer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SILoadBalancer: SpoolerInterface
{
    public SILoadBalancer(string SpoolerIP, int SpoolerPort, Logger _logger, Action<string>? _onSpoolerDisconnectAction): 
    base(SpoolerIP, SpoolerPort, _logger) {}

    public override void OnRecieve(byte[] message) {
        // Console.WriteLine();
        Program.logger.LogDebug("Recieving servers from spooler");
        List<Tuple<ByteIP, uint>> NewLobbyServerList = new List<Tuple<ByteIP, uint>>();

        for (int i = 2; i < message.Length; i+=7) {
            ByteIP ip = ByteIP.BytesToIP(ArrayExtentions.Slice(message, i, i+6));
            uint fill_level = (uint) message[i+6];
            Program.logger.LogDebug($"Recieved server: {ip} - {fill_level}");
            NewLobbyServerList.Add(new Tuple<ByteIP, uint>(ip, fill_level));
        }
        Program.logger.LogDebug("Recieve server done");
        Program.LobbyServers = NewLobbyServerList;

        Program.logger.LogDebug("Replying to spooler");
        uint num = 0;
        if (Program.server is not null) { num = (uint) Program.server.Players.Count; }
        SpoolerSocket.Send(new byte[] {(byte) num, (byte) (num>>8)});
    }
}

