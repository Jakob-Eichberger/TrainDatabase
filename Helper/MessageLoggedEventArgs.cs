using Microsoft.Extensions.Logging;
using System;

namespace Helper
{
    public class MessageLoggedEventArgs : EventArgs
    {
        public MessageLoggedEventArgs(LogLevel logLevel, string? message, Exception? exception) : base()
        {
            Message = message;
            LogLevel = logLevel;
            Exception = exception;
        }

        public MessageLoggedEventArgs(LogLevel logLevel, Exception? exception) : base()
        {
            Message = null;
            LogLevel = logLevel;
            Exception = exception;
        }

        public MessageLoggedEventArgs(LogLevel logLevel, string? message) : base()
        {
            Message = message;
            LogLevel = logLevel;
            Exception = null;
        }


        public DateTime DateTime { get; } = DateTime.Now;

        public string? Message { get; }

        public Exception? Exception { get; set; }

        public LogLevel LogLevel { get; }
    }
}
