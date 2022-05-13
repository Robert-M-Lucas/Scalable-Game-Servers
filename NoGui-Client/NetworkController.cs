namespace NoGuiClient;

using Shared;

public static class NetworkController{
    public static void Start(){
        ConnectToLoadBalancer();
    }    

    public static void ConnectToLoadBalancer() {
        Program.logger.LogInfo("Connecting to load balancer");
        ByteIP? ip = LoadBalancerClient.Run();
        if (ip is not null) {
            // Transfer successful
            ConnectToLobby(ip.strIP, (int) ip.iPort);
        }
        else {
            LoadBalancerConnectFailed();
        }
    }

    public static void LoadBalancerConnectFailed()
    {
        int c = ConsoleInputUtil.ChooseOption(new string[] {"Reconnect to Load Balancer", "Quit"});
        if (c == 0) {ConnectToLoadBalancer();}
    }

    public static void ConnectToLobby(string IP, int Port) 
    {
        new LobbyServerClient().Run(IP, Port);
    }

    public static void LobbyConnectFailed() {
        int c = ConsoleInputUtil.ChooseOption(new string[] {"Reconnect to Load Balancer", "Quit"});
        if (c == 0) {ConnectToLoadBalancer();}
    }

    public static void ConnectToMatchmaker() {}

    public static void ConnectToMatch(string IP, int Port) {}
}