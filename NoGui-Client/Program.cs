namespace NoGuiClient;

using Shared;

public static class Program{
    public static string LoadBalancerIP = "";
    public static int LoadBalancerPort = -1;
    public static string MatchmakerIP = "";
    public static int MatchmakerPort = -1;


    public static void Main(string[] args){
        if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }

        try { // Do all config handling in here
            ConfigObj? config = Config.GetConfig(args[0]);
            if (config is null) {throw new NullReferenceException();}
            if (config.Addresses is null) {throw new NullReferenceException();}
            if (config.Addresses.LoadBalancer is null) {throw new NullReferenceException();}
            if (config.Addresses.LoadBalancer.IP is null) {throw new NullReferenceException();}
            if (config.Addresses.LoadBalancer.Port is null) {throw new NullReferenceException();}
            if (config.Addresses.Matchmaker is null) {throw new NullReferenceException();}
            if (config.Addresses.Matchmaker.IP is null) {throw new NullReferenceException();}
            if (config.Addresses.Matchmaker.Port is null) {throw new NullReferenceException();}

            LoadBalancerIP = config.Addresses.LoadBalancer.IP;
            LoadBalancerPort = (int) config.Addresses.LoadBalancer.Port;

            MatchmakerIP = config.Addresses.Matchmaker.IP;
            MatchmakerPort = (int) config.Addresses.Matchmaker.Port;

        }
        catch (NullReferenceException) { Console.WriteLine("Null reference exception, probable incorrect formatting of config.json"); return; }
        catch (Exception e) { Console.WriteLine("Unhandled exception: " + e); Console.WriteLine("Probable incorrect formatting of config.json"); return; }

        while (true){
            Console.WriteLine("1. Connect");
            string? choice = Console.ReadLine();
            if (choice == "1"){
                break;
            }
        }
    }
}