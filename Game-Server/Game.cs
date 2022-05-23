namespace GameServer;

using Shared;

public class Game {
    private int[] board = new int[] {-1, -1, -1, -1, -1, -1 ,-1 ,-1, -1};

    private int[] playerID_to_turn;
    private bool turn = false;

    private int winner = -1;

    private Server server;

    public Game(int playerOneID, int playerTwoID, Server _server) {
        playerID_to_turn = new int[] {playerOneID, playerTwoID};
        server = _server;
    }

    public void HandlePacket(uint packet_type, byte[] data, GamePlayer player) {
        uint place = (uint) data[2];


        if (board[place] == -1) { board[place] = ArrayExtentions.Index(playerID_to_turn, player.ID); turn = ! turn; }

        winner = CheckForWin();
        UpdateAllClients();

        if (winner != -1) { server.ResetGame(); }
    }

    // -1 = no win, 0 = player 0, 1 = player 1
    public int CheckForWin() {
        if (board[0] != -1 && board[0] == board[1] && board[1] == board[2]) { return board[0]; }
        if (board[3] != -1 && board[3] == board[4] && board[4] == board[5]) { return board[3]; }
        if (board[6] != -1 && board[6] == board[7] && board[7] == board[8]) { return board[6]; }

        if (board[0] != -1 && board[0] == board[3] && board[3] == board[6]) { return board[0]; }
        if (board[1] != -1 && board[1] == board[4] && board[4] == board[7]) { return board[1]; }
        if (board[2] != -1 && board[2] == board[5] && board[5] == board[8]) { return board[2]; }

        if (board[0] != -1 && board[0] == board[4] && board[4] == board[8]) { return board[0]; }
        if (board[2] != -1 && board[2] == board[4] && board[4] == board[6]) { return board[2]; }

        return -1;
    }

    public void UpdateAllClients() {
        byte[] packet = new byte[14];
        packet[0] = (byte) (uint) 14;
        packet[1] = (byte) (uint) 0;

        if (!turn){
            packet[2] = (byte) (uint) 0;
        }
        else {
            packet[2] = (byte) (uint) 1;
        }

        // Sent to player[0] first //
        packet[3] = (byte) (uint) 0;

        const int offset = 4;

        for (int i = 0; i < 9; i++) {
            packet[i + offset] = (byte) board[i];
        }

        packet[13] = (byte) winner;

        server.Players[0].socket.Send(packet);

        // player 1
        packet[3] = (byte) (uint) 1;
        server.Players[1].socket.Send(packet);
    }
}