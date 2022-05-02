namespace LoadBalancer;

using System;
using Shared;

public static class Program{
    public static void Main(string[] args){
        Console.WriteLine(args[0]);
        ConfigObj? config = Config.GetConfig(args[0]);
        Console.WriteLine(config.Addresses.Load_Balancer.IP);
    }
}