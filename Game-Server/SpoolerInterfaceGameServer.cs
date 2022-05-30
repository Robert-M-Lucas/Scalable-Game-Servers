namespace GameServer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SIGameServer: SpoolerInterface
{
    public SIGameServer(string SpoolerIP, int SpoolerPort, Action<string, string> onSpoolerDisconnect): base(SpoolerIP, SpoolerPort, onSpoolerDisconnect) {}

    public override void OnRecieve(byte[] message) {
        Logger.LogDebug($"Spooler requested update, sending {Program.fill_level}");
        SpoolerSocket.Send(new byte[] {(byte) Program.fill_level});
    }
}
