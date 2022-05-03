namespace LoadBalancer;

using System;
using Shared;

public static class Program{
    public static string IP = "";
    public static int Port = -1;

    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static string Version = "";
    public static int MaxQueueLen = 0;

    public static void Main(string[] args){
        if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }

        try { // Do all config handling in here
            ConfigObj? config = Config.GetConfig(args[0]);
            if (config is null) {throw new NullReferenceException();}
            if (config.Addresses is null) {throw new NullReferenceException();}
            if (config.Addresses.LoadBalancer is null) {throw new NullReferenceException();}
            if (config.Addresses.LoadBalancer.IP is null) {throw new NullReferenceException();}
            if (config.Addresses.LoadBalancer.Port is null) {throw new NullReferenceException();}
            if (config.Version is null) {throw new NullReferenceException();}
            if (config.LoadBalancer is null) {throw new NullReferenceException();}
            if (config.LoadBalancer.MaxQueueLen is null) {throw new NullReferenceException();}

            IP = config.Addresses.LoadBalancer.IP;
            Port = (int) config.Addresses.LoadBalancer.Port;
            Version =  config.Version;
            MaxQueueLen = (int) config.LoadBalancer.MaxQueueLen;

        }
        catch (NullReferenceException) { Console.WriteLine("Null reference exception, probable incorrect formatting of config.json"); return; }
        catch (Exception e) { Console.WriteLine("Unhandled exception: " + e); Console.WriteLine("Probable incorrect formatting of config.json"); return; }

        new Server().Start();
    }
}