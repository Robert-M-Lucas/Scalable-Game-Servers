namespace NoGuiClient;

using Shared;
using System;

public static class Program {
    public static string LoadBalancerIP = "";
    public static int LoadBalancerPort = -1;
    public static string MatchmakerIP = "";
    public static int MatchmakerPort = -1;

    public static string Version = "";

    public static string ClientName = "";

    public static Logger logger = new Logger("NoGui-Client", false);

    public static ConfigObj config;

    public static void Main(string[] args) {
        try {
            ProtectedMain(args);
        }
        catch (Exception e) {
            logger.LogError(e.ToString());
        }

        logger.CleanUp();
    }
    
    public static void ProtectedMain(string[] args) {
        //if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }
        Console.Title = "Client";

        LoadBalancerIP = "127.0.0.1";
        MatchmakerIP = "127.0.0.1";

        try { 
            config = Config.GetConfig("config.json");
            LoadBalancerPort = config.LoadBalancerPort;
            MatchmakerPort = config.MatchmakerPort;
        }
        catch (BadConfigFormatException) { logger.LogError("Incorrect formatting of config.json"); return; }
        
        int choice = ConsoleInputUtil.ChooseOption(new string[] {"Connect", "Quit"}, true);
        if (choice == 1) { return; }

        bool good_name = false;
        while (!good_name) {
            Console.Write("Enter name (1-10 characters): "); ClientName = StringExtentions.DeNullString(Console.ReadLine());

            if (ClientName == "") {continue;}
            if (ClientName.Length > 10) {continue;}

            foreach (char l in ClientName) {
                if (l != ' ') { good_name = true; break; }
            }
        }
        Console.Title = $"Client - {ClientName}";
        NetworkController.Start();
    }
}