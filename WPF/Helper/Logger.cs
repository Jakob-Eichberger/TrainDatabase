using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace WPF_Application.Helper
{
    public class Logger
    {
        public static void Log(string message = "", bool isError = false, Exception exception = null!, [CallerMemberName] string functionName = default!)
        {
            string path = Directory.GetCurrentDirectory() + @"\Log";
            Directory.CreateDirectory(path);
            path += $@"\{DateTime.UtcNow.Date:dd-MM-yyyy}.txt";
            File.AppendAllText(path, $"{(isError ? "ERROR in function " : "Message from function ")}({functionName ?? " - "})({DateTime.UtcNow:dd.MM.yyyy h:mm:ss}): '{message}'\n{(isError ? $"{exception}\n\n\n" : "")}");
        }
    }
}
