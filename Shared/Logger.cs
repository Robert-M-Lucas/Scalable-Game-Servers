namespace Shared;

using System.Collections.Concurrent;
using System.Threading;
using System;
using System.IO;

public class LoggerNotAvailableException: Exception {
    public LoggerNotAvailableException() { }

    public LoggerNotAvailableException(string message) : base(message) { }

    public LoggerNotAvailableException(string message, Exception inner) : base(message, inner) { }
}

public static class Logger {
    private static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
    private static string LogName = "";
    private static Thread? LogThread;

    public static bool debug_printed = false;
    public static bool debug_logged = false;

    const ConsoleColor DEFAULT_COLOUR = ConsoleColor.White;
    const string INFO_TEXT =    "INFO ";
    const ConsoleColor INFO_COLOUR = ConsoleColor.Cyan;
    const string IMPORTANT_INFO_TEXT =    "INFO+";
    const ConsoleColor IMPORTANT_INFO_COLOUR = ConsoleColor.Magenta;
    const string WARNING_TEXT = "WARN ";
    const ConsoleColor WARNING_COLOUR = ConsoleColor.DarkYellow;
    const string ERROR_TEXT =   "ERROR";
    const ConsoleColor ERROR_COLOUR = ConsoleColor.DarkRed;
    const string DEBUG_TEXT =   "DEBUG";
    const ConsoleColor DEBUG_COLOUR = ConsoleColor.Green;

    static bool shutdown = false;
    static bool initialised = false;

    public static void InitialiseLogger(string log_name, bool debug = false) {
        if (initialised) { return; }
        initialised  = true;
        debug_printed = debug;
        LogName = "Logs\\" + log_name + DateTime.Now.ToString(" [dd-MM-yy HH.mm.ss]") + ".log";
        
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

        LogThread = new Thread(LogLoop);
        LogThread.Start();
        LogInfo("Logging started");
    }

    static void LogLoop() {
        Directory.CreateDirectory("Logs");
        StreamWriter sw = File.AppendText(LogName);
        sw.AutoFlush = true;
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

    static void LogAll(StreamWriter sw) {
        while (LogQueue.Count > 0) {
            string? log_line = "";
            if (LogQueue.TryDequeue(out log_line)) {
                sw.WriteLine(log_line);
            }
            else {
                Thread.Sleep(10);
            }
        }
    }

    public static void CleanUp() {
        if (!initialised | shutdown) {
            Console.WriteLine("Logging already shutdown or not initialised");
        }
        shutdown = true;
        LogWarning("Shutting down logging");

        LogThread?.Interrupt();
        LogThread?.Join();
    }

    static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.IsTerminating) {
            LogError("FATAL UNHANDLED EXCEPTION");
        }
        else {
            LogError("UNHANDLED EXCEPTION (NON FATAL)");
        }
        Exception e = (Exception) args.ExceptionObject;
        LogError(e);
        CleanUp();
    }

    public static void LogInfo(object log_text_obj) {
        if (log_text_obj is null) {return;}
        if (!initialised | shutdown) { throw new LoggerNotAvailableException("Logger shut down or not initialised"); }
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + INFO_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = INFO_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public static void LogImportant(object log_text_obj) {
        if (log_text_obj is null) {return;}
        if (!initialised | shutdown) { throw new LoggerNotAvailableException("Logger shut down or not initialised"); }
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + IMPORTANT_INFO_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = IMPORTANT_INFO_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public static void LogWarning(object log_text_obj) {
        if (log_text_obj is null) {return;}
        if (!initialised | shutdown) { throw new LoggerNotAvailableException("Logger shut down or not initialised"); }
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + WARNING_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = WARNING_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public static void LogError(object log_text_obj) {
        if (log_text_obj is null) { return; }
        if (!initialised | shutdown) { throw new LoggerNotAvailableException("Logger shut down or not initialised"); }
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + ERROR_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = ERROR_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public static void LogDebug(object log_text_obj) {
        if (!debug_logged) { return; }
        if (log_text_obj is null) { return; }
        if (!initialised | shutdown) { throw new LoggerNotAvailableException("Logger shut down or not initialised"); }
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + DEBUG_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        if (debug_printed) {
            Console.ForegroundColor = DEBUG_COLOUR;
            Console.WriteLine(text);
            Console.ForegroundColor = DEFAULT_COLOUR;
        }
    }
}