using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Helper
{
    public class Logger
    {
        public static event EventHandler<MessageLoggedEventArgs>? OnMessageLogged = default!;

        public static void LogError(Exception exception, string message)
        {
            OnMessageLogged?.Invoke($"{exception}", new MessageLoggedEventArgs($"{message}\n{exception}", LogLevel.Error));
            LogToFile(message, exception);
        }

        public static void LogInformation(string message) => OnMessageLogged?.Invoke(null, new MessageLoggedEventArgs(message, LogLevel.Information));

        public static void LogByteArray(string message, byte[] bytes) => OnMessageLogged?.Invoke(null, new MessageLoggedEventArgs($"{string.Join(" ", bytes.Select(e => $"{e:x2}"?.ToUpper()))} - {message}", LogLevel.Information));

        public static void LogWarning(string message) => OnMessageLogged?.Invoke(null, new MessageLoggedEventArgs(message, LogLevel.Warning));

        private static void LogToFile(string message, Exception exception)
        {
            Logger.LogInformation(message);
            string path = Directory.GetCurrentDirectory() + @"\Log";
            Directory.CreateDirectory(path);
            path += $@"\{DateTime.UtcNow.Date:dd-MM-yyyy}.txt";
            File.AppendAllText(path, $"({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss}): '{message}'\n\n Exception: {exception} \n\n------------------------------------------------------------------------------------------------\n");
        }
    }
}
