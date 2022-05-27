namespace ServerSpooler;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Shared;

public class ServerConnectTimeoutException : Exception
{
    public ServerConnectTimeoutException() { }

    public ServerConnectTimeoutException(string message) : base(message) { }

    public ServerConnectTimeoutException(string message, Exception inner) : base(message, inner) { }
}

public static class Listener{
    public static Socket? LoadBalancerSocket;
    public static Socket? MatchmakerSocket;

    public static Socket? TempSocket;

    public static Socket? ListenerSocket;

    public static Socket AcceptClient(){
        Logger.LogInfo("Accepting client");

        if (ListenerSocket is null) {
            Logger.LogDebug("Listener socket does not exist, creating");

            IPAddress ipAddress = IPAddress.Any;

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Program.config.ServerSpoolerPort);

            ListenerSocket = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

            ListenerSocket.Bind(localEndPoint);
        }

        ListenerSocket.Listen(1);

        Stopwatch s = new Stopwatch();
        s.Start();
        Logger.LogInfo("Accepting connection");

        Thread t = new Thread(() => {
            TempSocket = ListenerSocket.Accept();
        });
        t.Start();

        while (s.ElapsedMilliseconds < Program.MaxServerConnectTime){
            if (!t.IsAlive) { break; }
            Thread.Sleep(10);
        }
        
        if (t.IsAlive) { t.Interrupt(); throw new ServerConnectTimeoutException(); }

        t.Join();

        Logger.LogInfo("Connection established");

        if (TempSocket is null) { throw new NullReferenceException(); }

        return TempSocket;
    }

    public static void Exit() {

    }
}