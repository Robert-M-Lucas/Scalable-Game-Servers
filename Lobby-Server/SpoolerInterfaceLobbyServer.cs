namespace LobbyServer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SILobbyServer: SpoolerInterface
{
    public SILobbyServer(string SpoolerIP, int SpoolerPort, Logger _logger): base(SpoolerIP, SpoolerPort, _logger) {}

    public override void OnRecieve(byte[] message) {
        Program.logger.LogDebug("Spooler requested update");
        SpoolerSocket.Send(new byte[] {(byte) Program.fill_level});
    }
}
