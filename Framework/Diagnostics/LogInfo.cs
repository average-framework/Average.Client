using System;

namespace Average.Client.Framework.Diagnostics
{
    internal class LogInfo
    {
        public string Message { get; private set; }
        public LogLevel Level { get; private set; }
        public DateTime Date { get; private set; }

        public LogInfo(string message, LogLevel logLevel)
        {
            Message = message;
            Level = logLevel;
            Date = DateTime.Now.ToLocalTime();
        }
    }
}
