namespace NoGuiClient;

using Shared;

public static class Program {
    public static string LoadBalancerIP = "";
    public static int LoadBalancerPort = -1;
    public static string MatchmakerIP = "";
    public static int MatchmakerPort = -1;

    public static string Version = "";

    public static string ClientName = "";

    public static void Main(string[] args) {
        //if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }

        LoadBalancerIP = "127.0.0.1";

        try { 
            ConfigObj config = Config.GetConfig("config.json"); 
            LoadBalancerPort = config.LoadBalancerPort;
        }
        catch (BadConfigFormatException) { Console.WriteLine("Incorrect formatting of config.json"); return; }
        
        int choice = ConsoleInputUtil.ChooseOption(new string[] {"Connect", "Quit"}, true);
        if (choice == 1) { return; }

        Console.Write("Enter name: "); ClientName = StringExtentions.DeNullString(Console.ReadLine());

        NetworkController.Start();

        Console.ReadLine();
    }
}