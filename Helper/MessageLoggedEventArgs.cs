using Microsoft.Extensions.Logging;
using System;

namespace Helper
{
    public class MessageLoggedEventArgs : EventArgs
    {
        public MessageLoggedEventArgs(LogLevel logLevel, string? message, Exception? exception, string callingName) : base()
        {
            Message = message;
            LogLevel = logLevel;
            Exception = exception;
            CallingName = callingName;
        }


        public DateTime DateTime { get; } = DateTime.Now;

        public string? Message { get; }

        public Exception? Exception { get; set; }

        public string CallingName { get; set; }

        public LogLevel LogLevel { get; }
    }
}
