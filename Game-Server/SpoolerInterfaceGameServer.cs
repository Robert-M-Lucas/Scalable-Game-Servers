namespace GameServer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SIGameServer: SpoolerInterface
{
    public SIGameServer(string SpoolerIP, int SpoolerPort, Logger _logger): base(SpoolerIP, SpoolerPort, _logger) {}

    public override void OnRecieve(byte[] message) {
        Program.logger.LogDebug($"Spooler requested update, sending {Program.fill_level}");
        SpoolerSocket.Send(new byte[] {(byte) Program.fill_level});
    }
}