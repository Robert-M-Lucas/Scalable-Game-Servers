namespace LoadBalancer;

using System;
using Shared;

public static class Program{
    public static int Port = -1;

    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static string Version = "";
    public static int MaxQueueLen = 0;

    public static void Main(string[] args){
        if (args.Length < 4) { Console.WriteLine("Args must be: [Version] [Server Spooler IP] [Server Spooler Port] [Load Balancer Port]"); return; }

        Version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { Console.WriteLine("Spooler port incorrectly formatted"); return; }
        if (!int.TryParse(args[3], out Port)) { Console.WriteLine("Port incorrectly formatted"); return; }

        new Server().Start();
    }
}