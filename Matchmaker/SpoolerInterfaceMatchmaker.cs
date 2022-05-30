namespace Matchmaker;

using Shared;

public class SIMatchmaker: SpoolerInterface {
    public SIMatchmaker(string SpoolerIP, int SpoolerPort, Action<string, string>? _onSpoolerDisconnectAction): 
    base(SpoolerIP, SpoolerPort, _onSpoolerDisconnectAction) {}

    public override void OnRecieve(byte[] message) {
        // Console.WriteLine();
        Logger.LogDebug("Recieving servers from spooler");
        List<Tuple<ByteIP, uint>> NewLobbyServerList = new List<Tuple<ByteIP, uint>>();

        for (int i = 2; i < message.Length; i+=7) {
            ByteIP ip = ByteIP.BytesToIP(ArrayExtentions.Slice(message, i, i+6));
            uint fill_level = (uint) message[i+6];
            Logger.LogDebug($"Recieved server: {ip} - {fill_level}");
            NewLobbyServerList.Add(new Tuple<ByteIP, uint>(ip, fill_level));
        }
        Logger.LogDebug("Recieve server done");
        Program.GameServers = NewLobbyServerList;

        Logger.LogDebug("Replying to spooler");
        uint num = 0;
        if (Program.server is not null) { num = (uint) Program.server.Players.Count; }
        SpoolerSocket.Send(new byte[] {(byte) num, (byte) (num>>8)});
    }
}