namespace Shared;

using System.Collections.Concurrent;
using System.Threading;
using System;
using System.IO;

public class Logger {
    public ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
    public string LogName = "";
    public Thread LogThread;

    public bool debug_logged = false;

    const ConsoleColor DEFAULT_COLOUR = ConsoleColor.White;
    const string INFO_TEXT =    "INFO   ";
    const ConsoleColor INFO_COLOUR = ConsoleColor.Cyan;
    const string WARNING_TEXT = "WARNING";
    const ConsoleColor WARNING_COLOUR = ConsoleColor.DarkYellow;
    const string ERROR_TEXT =   "ERROR  ";
    const ConsoleColor ERROR_COLOUR = ConsoleColor.DarkRed;
    const string DEBUG_TEXT =   "DEBUG  ";
    const ConsoleColor DEBUG_COLOUR = ConsoleColor.Green;

    public Logger(string log_name, bool debug = false) {
        debug_logged = debug;
        LogName = "Logs\\" + log_name + DateTime.Now.ToString(" [dd-MM-yy HH;mm;ss]") + ".log";
        LogInfo("Logging started");
        LogThread = new Thread(LogLoop);
        LogThread.Start();
    }

    void LogLoop() {
        Directory.CreateDirectory("Logs");
        StreamWriter sw = File.AppendText(LogName);
        try {
            while (true) { 
                LogAll(sw);
                Thread.Sleep(10);
                // Console.WriteLine("Loop");
            }
        }
        catch (ThreadInterruptedException) {
            // Console.WriteLine("Interrupt");
            LogAll(sw);
            sw.Close();
            return;
        }
    }

    void LogAll(StreamWriter sw) {
        while (LogQueue.Count > 0) {
            string log_line = "";
            if (LogQueue.TryDequeue(out log_line)) {
                sw.WriteLine(log_line);
            }
            else {
                // Console.WriteLine("Wait");
                Thread.Sleep(10);
            }
        }
    }

    public void CleanUp() {
        LogWarning("Shutting down logging");
        LogThread.Interrupt();
        LogThread.Join();
    }

    public void LogInfo(string log_text) {
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + INFO_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = INFO_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogWarning(string log_text) {
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + WARNING_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = WARNING_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogError(string log_text) {
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + ERROR_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = ERROR_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogDebug(string log_text) {
        if (!debug_logged) {return;}

        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + DEBUG_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = DEBUG_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }
}