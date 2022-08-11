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
    public class LogService
    {
        public event EventHandler<MessageLoggedEventArgs>? OnMessageLogged = default!;

        public void Log(LogLevel level, string? message, Exception? exception, [CallerMemberName] string callerName = "") => OnMessageLogged?.Invoke(this, new(level, message, exception, callerName));

        public void Log(LogLevel level, Exception? exception, [CallerMemberName] string callerName = "") => OnMessageLogged?.Invoke(this, new(level, null, exception, callerName));

        public void Log(LogLevel level, string? message, [CallerMemberName] string callerName = "") => OnMessageLogged?.Invoke(this, new(level, message, null, callerName));
    }
}
