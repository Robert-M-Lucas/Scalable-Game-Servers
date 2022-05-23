namespace NoGuiClient;

using Shared;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class GameServerClient {
    Socket? Handler;

    public int Run(string IP, int Port) {
        return Connect(IP, Port);
        // NetworkController.ConnectToLobby()
    }

    int Connect(string IP, int Port) {
        byte[] server_buffer = new byte[1024];

        IPAddress HostIpA = IPAddress.Parse(IP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, Port);

        Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            Handler.Connect(RemoteEP);
        }
        catch (SocketException e){
            Program.logger.LogError("Failed to connect to target Game Server " + IP + ":" + Port);
            Program.logger.LogError(e.ToString());
            return 0;
        }

        if (Handler.RemoteEndPoint is null) {Program.logger.LogError("Remote end point is null"); return 0; }

        Program.logger.LogDebug("Socket connected to " + Handler.RemoteEndPoint.ToString());

        // Send connection data
        Handler.Send(ArrayExtentions.Merge(new byte[10], Encoding.ASCII.GetBytes(Program.ClientName)));

        Program.logger.LogInfo("Waiting for second player to join");

        try {
            while (true) {
                byte[] recv = new byte[14];
                Handler.Receive(recv, 0, 14, 0);
                uint turn = recv[2];
                uint player = recv[3];
                const int offset = 4;
                int[] board = new int[9];

                for (int i = 0; i < 9; i++) {
                    board[i] = recv[i + offset];
                }

                int winner = recv[13];

                RenderBoard(board);
                Program.logger.LogInfo("");

                if (player == 0) { Program.logger.LogInfo("You are O"); }
                else { Program.logger.LogInfo("You are X"); }

                if (winner != 255) { 
                    if (winner == 0) { Program.logger.LogInfo("O wins"); }
                    else { Program.logger.LogInfo("X wins"); }
                    Handler.Shutdown(SocketShutdown.Both);
                    return 1;
                }

                if (turn == 0) { Program.logger.LogInfo("O's turn"); }
                else { Program.logger.LogInfo("X's turn"); }

                if (player != turn) { continue; }

                int c = ConsoleInputUtil.GetInt(1, 9, "Choose a number (1 - 9): ") - 1;

                byte[] to_send = new byte[3];
                to_send[0] = (byte) (uint) 3;
                to_send[1] = (byte) (uint) 0;

                to_send[2] = (byte) (uint) c;

                Handler.Send(to_send, 0, 3, 0);

                /*
                int c = ConsoleInputUtil.ChooseOption(new string[] {"Echo name", "Get counter", "Quit Game", "Quit"});
                if (c == 0) {EchoName(Handler);}
                else if (c == 1) {GetCounter(Handler);}
                else if (c == 2) {Handler.Shutdown(SocketShutdown.Both); return 1; }
                else if (c == 3) {Handler.Shutdown(SocketShutdown.Both); return 2; }
                */
            }
        }
        catch (SocketException se) {
            Program.logger.LogError("Game Server disconnected client");
            Program.logger.LogError(se.ToString());
            return 0;
        }
    }

    void RenderBoard(int[] board) {
        for (int y = 0; y < 3; y++) {
            string row = "";
            for (int x = 0; x < 3; x++) {
                int piece = board[(3*y) + x];
                if (piece == 255) { row += ((y*3)+x+1).ToString(); }
                else if (piece == 0) { row += "O"; }
                else if (piece == 1) { row += "X"; }
                row += " ";
            }
            Program.logger.LogInfo(row);
        }
    }
}