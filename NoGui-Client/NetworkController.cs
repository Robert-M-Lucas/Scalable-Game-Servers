namespace NoGuiClient;

using Shared;

public static class NetworkController{
    public static void Start(){
        ConnectToLoadBalancer();
    }    

    public static void ConnectToLoadBalancer() {
        Console.WriteLine("Connecting to load balancer");
        new LoadBalancerClient().Run();
    }

    public static void LoadBalancerConnectFailed()
    {
        int c = ConsoleInputUtil.ChooseOption(new string[] {"Reconnect to Load Balancer", "Quit"});
        if (c == 0) {ConnectToLoadBalancer();}
    }

    public static void ConnectToLobby(string IP, int Port) {}

    public static void ConnectToMatchmaker() {}

    public static void ConnectToMatch(string IP, int Port) {}
}