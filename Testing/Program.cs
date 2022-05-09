namespace Testing;

using System.IO;

public static class Program {
    public static void Main(string[] args) {
        Directory.CreateDirectory("Logs");
        File.WriteAllLines("Logs\\log.log", new string[] {"asd"});
    }
}