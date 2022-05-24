namespace Shared;

using System.Collections.Concurrent;
using System.Threading;
using System;
using System.IO;

public class Logger {
    private ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
    private string LogName = "";
    private Thread LogThread;

    public bool debug_printed = false;
    public bool debug_logged = false;

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

    bool shutdown = false;
    static Logger? logger;

    public Logger(string log_name, bool debug = false) {
        logger = this;

        debug_printed = debug;
        LogName = "Logs\\" + log_name + DateTime.Now.ToString(" [dd-MM-yy HH.mm.ss]") + ".log";
        
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

        LogThread = new Thread(LogLoop);
        LogThread.Start();
        LogInfo("Logging started");
    }

    void LogLoop() {
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

    void LogAll(StreamWriter sw) {
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

    public void CleanUp() {
        if (shutdown) {
            Console.WriteLine("Logging already shutdown");
        }
        shutdown = true;
        LogWarning("Shutting down logging");
        LogThread.Interrupt();
        LogThread.Join();
    }

    static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (logger is null) { return; }
        if (args.IsTerminating) {
            logger.LogError("FATAL UNHANDLED EXCEPTION!");
        }
        else {
            logger.LogError("UNHANDLED EXCEPTION");
        }
        Exception e = (Exception) args.ExceptionObject;
        logger.LogError(e);
        logger.CleanUp();
    }

    public void LogInfo(object log_text_obj) {
        if (log_text_obj is null) {return;}
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + INFO_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = INFO_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogImportant(object log_text_obj) {
        if (log_text_obj is null) {return;}
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + IMPORTANT_INFO_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = IMPORTANT_INFO_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogWarning(object log_text_obj) {
        if (log_text_obj is null) {return;}
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + WARNING_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = WARNING_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogError(object log_text_obj) {
        if (log_text_obj is null) { return; }
        string? log_text = log_text_obj.ToString();
        string text = "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss:fffffff") + "] [" + ERROR_TEXT + "]: " + log_text;
        LogQueue.Enqueue(text);
        Console.ForegroundColor = ERROR_COLOUR;
        Console.WriteLine(text);
        Console.ForegroundColor = DEFAULT_COLOUR;
    }

    public void LogDebug(object log_text_obj) {
        if (!debug_logged) { return; }
        if (log_text_obj is null) { return; }
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