namespace Shared;

using Shared;

public static class ConsoleInputUtil{
    public static void WaitForEnter() {
        Console.ReadLine();
    }

    public static int GetInt(string input_prompt = "Choose a number: ") {
        int number = -1;
        string input = "a";
        while (!int.TryParse(input, out number)) {
            Console.Write(input_prompt);
            string? _result = Console.ReadLine();
            if (_result is null) {
                continue;
            }
            else {
                input = (string) _result;
            }
        }
        return number;
    }

    public static int GetInt(int min, int max, string input_prompt = "Choose a number: ") {
        int number = -1;
        string input = "a";
        while (!int.TryParse(input, out number) || number < min || number > max) {
            Console.Write(input_prompt);
            string? _result = Console.ReadLine();
            if (_result is null) {
                continue;
            }
            else {
                input = (string) _result;
            }
        }
        return number;
    }

    public static int ChooseOption(string[] options, bool enforce = false) {
        int ret = -1;
        while (true) {
            for (int i = 0; i < options.Length; i++){
                Console.WriteLine(i+1 + ". " + options[i]);
            }
            Console.Write("Input: ");
            string _return = StringExtentions.NullToEmptyString(Console.ReadLine());

            if (int.TryParse(_return, out ret)) {  
                if (ret > 0 && ret <= options.Length) { ret -= 1; enforce = false; }
            }

            if (!enforce) { break; }
        }
        return ret;
    }

    public static void ChooseOptionAsync(string[] options, Action<string> action) {
        for (int i = 0; i < options.Length; i++) {
            Console.WriteLine(i + ". " + options[i]);
        }
        Console.Write("Input: ");

        new Thread(() => WaitForReadLine(action)).Start();
    }

    public static void WaitForReadLine(Action<string> action){
        string? _result = Console.ReadLine();
        string result;
        if (_result is null) {
            result = "";
        }
        else {
            result = (string) _result;
        }

        action(result);
    }
}