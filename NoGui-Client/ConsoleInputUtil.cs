namespace NoGuiClient;

public static class ConsoleInputUtil{
    public static void WaitForEnter() {
        Console.ReadLine();
    }

    public static string ChooseOption(string[] options) {
        for (int i = 0; i < options.Length; i++){
            Console.WriteLine(i + ". " + options[i]);
        }

        string? _return = Console.ReadLine();
        if (_return is null){
            return "";
        }
        else{
            return (string) _return;
        }
    }

    public static void ChooseOptionAsync(string[] options, Action<string> action) {
        for (int i = 0; i < options.Length; i++){
            Console.WriteLine(i + ". " + options[i]);
        }

        new Thread(() => WaitForReadLine(action)).Start();
    }

    public static void WaitForReadLine(Action<string> action){
        string? _result = Console.ReadLine();
        string result;
        if (_result is null){
            result = "";
        }
        else{
            result = (string) _result;
        }

        action(result);
    }
}