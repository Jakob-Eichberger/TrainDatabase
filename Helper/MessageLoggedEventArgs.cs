using System;

namespace Helper
{
    public class MessageLoggedEventArgs : EventArgs
    {
        public MessageLoggedEventArgs(string message, MessageTypeEnum type, object value = null) : base()
        {
            Message = message;
            Type = type;
            Value = value;
        }

        public MessageLoggedEventArgs(MessageTypeEnum type, object value = null) : base()
        {
            Type = type;
            Value = value;
        }

        public DateTime DateTime { get; } = DateTime.Now;

        public string? Message { get; }

        public MessageTypeEnum Type { get; }

        public object? Value { get; }
    }
}
