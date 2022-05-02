namespace Shared;

using Shared;

public static class ConsoleInputUtil{
    public static void WaitForEnter() {
        Console.ReadLine();
    }

    public static int ChooseOption(string[] options, bool enforce = false) {
        int ret = -1;
        while (true) {
            for (int i = 0; i < options.Length; i++){
                Console.WriteLine(i+1 + ". " + options[i]);
            }
            Console.Write("Input: ");
            string _return = StringExtentions.DeNullString(Console.ReadLine());

            if (int.TryParse(_return, out ret)) {  
                if (ret > 0 && ret <= options.Length) { ret -= 1; enforce = false; }
            }

            if (!enforce) { break; }
        }
        return ret;
    }

    public static void ChooseOptionAsync(string[] options, Action<string> action) {
        for (int i = 0; i < options.Length; i++){
            Console.WriteLine(i + ". " + options[i]);
        }
        Console.Write("Input: ");

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