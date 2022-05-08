namespace ServerSpooler;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

public class ServerConnectTimeoutException : Exception
{
    public ServerConnectTimeoutException() { }

    public ServerConnectTimeoutException(string message) : base(message) { }

    public ServerConnectTimeoutException(string message, Exception inner) : base(message, inner) { }
}

public static class Listener{
    public static Socket? LoadBalancerSocket;

    public static Socket? TempSocket;

    public static Socket? ListenerSocket;

    public static Socket AcceptClient(){
        if (ListenerSocket is null) {
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
        Console.WriteLine("Accepting connection");

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

        Console.WriteLine("Connection established");

        if (TempSocket is null) { throw new NullReferenceException(); }

        return TempSocket;
    }

    public static void Exit() {

    }
}