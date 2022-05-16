namespace NoGuiClient;

using Shared;

public static class NetworkController{
    public static void Start(){
        ConnectToLoadBalancer();
    }    

    public static void ConnectToLoadBalancer() {
        Program.logger.LogInfo("Connecting to Load Balancer");
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
        int status = new LobbyServerClient().Run(IP, Port);
        if (status == 0) { LobbyConnectFailed(); }
        else if (status == 1) { ConnectToMatchmaker(); }
    }

    public static void LobbyConnectFailed() {
        int c = ConsoleInputUtil.ChooseOption(new string[] {"Reconnect to Load Balancer", "Quit"});
        if (c == 0) {ConnectToLoadBalancer();}
    }

    public static void ConnectToMatchmaker() {
        Program.logger.LogInfo("Connecting to Matchmaker");
        ByteIP? ip = MatchmakerClient.Run();
        if (ip is not null) {
            // Transfer successful
            ConnectToMatch(ip.strIP, (int) ip.iPort);
        }
        else {
            MatchmakerConnectFailed();
        }
    }

    public static void MatchmakerConnectFailed() { 
        int c = ConsoleInputUtil.ChooseOption(new string[] {"Reconnect to Matchmaker", "Reconnect to Load Balancer", "Quit"});
        if (c == 0) {ConnectToMatchmaker();}
        else if (c == 1) {ConnectToLoadBalancer();}
    }

    public static void ConnectToMatch(string IP, int Port) {
        
    }
}