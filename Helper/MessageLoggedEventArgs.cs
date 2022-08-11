using Microsoft.Extensions.Logging;
using System;

namespace Helper
{
    public class MessageLoggedEventArgs : EventArgs
    {
        public MessageLoggedEventArgs(string message, LogLevel logLevel) : base()
        {
            Message = message;
            LogLevel = logLevel;
        }

        public DateTime DateTime { get; } = DateTime.Now;

        public string? Message { get; }

        public LogLevel LogLevel { get; }
    }
}
