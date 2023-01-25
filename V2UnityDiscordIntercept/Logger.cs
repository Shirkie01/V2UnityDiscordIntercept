using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace V2UnityDiscordIntercept
{
    internal static class Logger
    {
        public static void Log(string message)
        {
            var method = new StackFrame(1).GetMethod();
            string msg = $"[{DateTime.UtcNow}] - {method.DeclaringType}.{method.Name} - {message}\r\n";
            LogToFile(msg);
        }

        private static void LogToFile(string message)
        {
            Directory.CreateDirectory("Logs");
            File.AppendAllText($"Logs/{Plugin.Username}.txt", message);
        }
    }
}
