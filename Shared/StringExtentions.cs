namespace Shared;

public static class StringExtentions {
    public static string DeNullString(string? str){
        if (str is null) { return ""; }
        return str;
    }
}