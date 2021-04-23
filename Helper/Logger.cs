using System;
using System.IO;

namespace Helper
{
    public class Logger
    {
        public static void Log(string message, LoggerType type)
        {
            string path = Directory.GetCurrentDirectory() + @"\Log";
            switch (type)
            {
                case LoggerType.Information:
                    path += $@"\Information";
                    break;
                case LoggerType.Warning:
                    path += $@"\Warning";
                    break;
                case LoggerType.Error:
                    path += $@"\Error";
                    break;
            }
            Directory.CreateDirectory(path);
            path += $@"\{DateTime.UtcNow.Date:dd-MM-yyyy}.txt";
            File.AppendAllText(path, $"{DateTime.UtcNow:dd.MM.yyyy h:mm:ss}: '{message}'\n");
        }
    }
}
