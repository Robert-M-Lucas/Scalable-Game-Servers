namespace Matchmaker;
using System;
using Shared;

public static class Program{
    public static string IP = "";
    public static int Port = -1;

    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static void Main(string[] args){
        if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }

        try { // Do all config handling in here
            ConfigObj? config = Config.GetConfig(args[0]);
            if (config is null) {throw new NullReferenceException();}
            if (config.Addresses is null) {throw new NullReferenceException();}
            if (config.Addresses.Matchmaker is null) {throw new NullReferenceException();}
            if (config.Addresses.Matchmaker.IP is null) {throw new NullReferenceException();}
            if (config.Addresses.Matchmaker.Port is null) {throw new NullReferenceException();}

            IP = config.Addresses.Matchmaker.IP;
            Port = (int) config.Addresses.Matchmaker.Port;

        }
        catch (NullReferenceException) { Console.WriteLine("Null reference exception, probable incorrect formatting of config.json"); return; }
        catch (Exception e) { Console.WriteLine("Unhandled exception: " + e); Console.WriteLine("Probable incorrect formatting of config.json"); return; }
    }
}