using System;

namespace Infrastructure.Logging;

public static class Logger
{
    private static readonly bool Enabled = true; // solo lectura

    public static void Log(string message)
    {
        if (!Enabled) return;
        Console.WriteLine("[LOG] " + DateTime.Now + " - " + message);
    }

    public static void Try(Action a)
    {
        try { a(); } catch { }
    }
}
