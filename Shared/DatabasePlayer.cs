namespace Shared;

public class InvalidPlayerException : Exception
{
    public InvalidPlayerException() { }

    public InvalidPlayerException(string message) : base(message) { }

    public InvalidPlayerException(string message, Exception inner) : base(message, inner) { }
}

public class DatabasePlayer {
    public string PlayerName;
    public string PlayerPassword;
    public int PlayerID;
    public int PlayerCurrencyAmount;

    public DatabasePlayer (string player_name, string player_password, int player_id, int player_currency_amount) {
        PlayerName = player_name;
        PlayerPassword = player_password;
        PlayerID = player_id;
        PlayerCurrencyAmount = player_currency_amount;
    }

    public override string ToString()
    {
        return $"[Name: {PlayerName}, Password: {PlayerPassword}, ID: {PlayerID}, CurrencyAmount: {PlayerCurrencyAmount}]";
    }
}