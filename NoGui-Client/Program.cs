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
    public static string ClientPassword = "";

    public static ConfigObj config;

    public static void Main(string[] args) {
        Logger.InitialiseLogger("NoGui-Client", false);

        try {
            ProtectedMain(args);
        }
        catch (Exception e) {
            Logger.LogError(e.ToString());
        }

        Logger.CleanUp();
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
        catch (BadConfigFormatException) { Logger.LogError("Incorrect formatting of config.json"); return; }
        
        int choice = ConsoleInputUtil.ChooseOption(new string[] {"Connect", "Quit"}, true);
        if (choice == 1) { return; }

        Console.WriteLine("\nEnter a username and password.\nIf the username does not exist, a new account will be created with the given password.\n(Username not case sensitive)\n");

        bool good_name = false;
        while (!good_name) {
            Console.Write("Enter username (8-16 characters): "); ClientName = StringExtentions.NullToEmptyString(Console.ReadLine());

            if (ClientName == "") {continue;}
            if (ClientName.Length > 16) {continue;}
            if (ClientName.Length < 8) {continue;}

            good_name = true;
            foreach (char l in ClientName) {
                if (l == ' ') { 
                    Logger.LogWarning("No spaces allowed in username");
                    good_name = false;
                    break;
                }
            }
        }

        good_name = false;
        while (!good_name) {
            Console.Write("Enter password (8-16 characters): "); ClientPassword = StringExtentions.NullToEmptyString(Console.ReadLine());

            if (ClientPassword == "") {continue;}
            if (ClientPassword.Length > 16) {continue;}
            if (ClientPassword.Length < 8) {continue;}

            good_name = true;
            foreach (char l in ClientPassword) {
                if (l == ' ') { 
                    Logger.LogWarning("No spaces allowed in password");
                    good_name = false;
                    break;
                }
            }

            Console.Write("Confirm password: ");
            if (ClientPassword != StringExtentions.NullToEmptyString(Console.ReadLine())) {
                good_name = false;
                continue;
            }
        }

        Console.Title = $"Client - {ClientName}";
        ClientName = ClientName.ToLower();
        NetworkController.Start();
    }
}