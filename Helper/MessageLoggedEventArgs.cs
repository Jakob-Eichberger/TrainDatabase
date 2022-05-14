using System;

namespace Helper
{
    public class MessageLoggedEventArgs : EventArgs
    {
        public MessageLoggedEventArgs(string message, MessageTypeEnum type) : base()
        {
            Message = message;
            Type = type;
        }

        public DateTime DateTime { get; } = DateTime.Now;

        public string? Message { get; }

        public MessageTypeEnum Type { get; }
    }
}
