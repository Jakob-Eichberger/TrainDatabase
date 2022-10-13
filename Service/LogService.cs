using Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class LogEventBus
    {
        public event EventHandler<MessageLoggedEventArgs>? OnMessageLogged = default!;

        public void Log(LogLevel level, string? message, Exception? exception) => OnMessageLogged?.Invoke(this, new(level, message, exception));

        public void Log(LogLevel level, Exception? exception) => OnMessageLogged?.Invoke(this, new(level, null, exception));

        public void Log(LogLevel level, string? message) => OnMessageLogged?.Invoke(this, new(level, message, null));
    }
}
