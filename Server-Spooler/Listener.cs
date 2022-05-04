namespace ServerSpooler;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

public static class Listener{
    public static Socket? LoadBalancerSocket;

    public static Socket AcceptClient(){
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
        // while (s.ElapsedMilliseconds < Program.MaxServerConnectTime * 1000){
        //     if (listener.)
        // }
        Socket Handler = listener.Accept();
        
        Console.WriteLine("Connection established");
        
        return Handler;
    }
}