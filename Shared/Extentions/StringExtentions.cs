namespace Shared;

public static class StringExtentions {
    public static string NullToEmptyString(string? str){
        if (str is null) { return ""; }
        return str;
    }
}