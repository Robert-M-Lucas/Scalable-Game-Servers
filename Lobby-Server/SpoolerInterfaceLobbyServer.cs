namespace LobbyServer;

using Shared;
using System.Net;
using System.Net.Sockets;

public class SILobbyServer: SpoolerInterface
{
    public SILobbyServer(string SpoolerIP, int SpoolerPort): base(SpoolerIP, SpoolerPort) {}

    public override void OnRecieve(byte[] message) {
        Console.WriteLine("Spooler requested update");
        SpoolerSocket.Send(new byte[] {(byte) Program.fill_level});
    }
}
