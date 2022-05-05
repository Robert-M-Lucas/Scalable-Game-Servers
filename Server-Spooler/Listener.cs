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

public class Listener{
    public Socket? LoadBalancerSocket;

    public Socket? TempSocket;

    public Socket AcceptClient(){
        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Program.ServerSpoolerPort);

        Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

        listener.Bind(localEndPoint);
        listener.Listen(1);

        Stopwatch s = new Stopwatch();
        s.Start();
        Console.WriteLine("Accepting connection");

        Thread t = new Thread(() => {
            TempSocket = listener.Accept();
        });
        t.Start();

        while (s.ElapsedMilliseconds < Program.MaxServerConnectTime * 1000){
            if (!t.IsAlive) { break; }
            Thread.Sleep(100);
            Console.WriteLine($"{s.ElapsedMilliseconds}/{Program.MaxServerConnectTime * 1000}");
        }
        
        if (t.IsAlive) { t.Interrupt(); throw new ServerConnectTimeoutException(); }

        t.Join();

        Console.WriteLine("Connection established");

        if (TempSocket is null) { throw new NullReferenceException(); }

        return TempSocket;
    }

    public void Exit() {

    }
}