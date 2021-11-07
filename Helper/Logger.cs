using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Helper
{
    public class Logger
    {
        public static void Log(string message = "", Exception exception = null!, [CallerMemberName] string functionName = default!)
        {
            Console.WriteLine(message);
            string path = Directory.GetCurrentDirectory() + @"\Log";
            Directory.CreateDirectory(path);
            path += $@"\{DateTime.UtcNow.Date:dd-MM-yyyy}.txt";
            File.AppendAllText(path, $"ERROR in function '{functionName ?? " - "}' ({DateTime.UtcNow:dd.MM.yyyy h:mm:ss}): '{message}'\n\n Exception: {exception.ToString() ?? "-"} \n\n StackTrace: {exception.StackTrace ?? "-"}\n\n------------------------------------------------------------------------------------------------\n");
        }
    }
}
