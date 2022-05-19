namespace GameServer;

public class Game {
    private int[] board = new int[9];

    private int[] playerID_to_turn;
    private bool turn;

    public Game(int playerOneID, int playerTwoID) {
        playerID_to_turn = new int[] {playerOneID, playerTwoID};
    }

    public void HandlePacket(uint packet_type, byte[] data) {

    }

    public void UpdateAllClients() {

    }
}