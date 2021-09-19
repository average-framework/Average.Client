using System;
using System.Collections.Generic;
using System.Linq;

namespace Average.Client.Framework.Diagnostics
{
    internal static class Logger
    {
        public const string Black = "^0";
        public const string DarkRed = "^1";
        public const string Green = "^2";
        public const string Yellow = "^3";
        public const string Blue = "^4";
        public const string Cyan = "^5";
        public const string Pink = "^6";
        public const string White = "^7";
        public const string Grey = "^8";
        public const string Red = "^9";

        private static List<LogInfo> _logs = new List<LogInfo>();

        internal static void Debug(string message)
        {
            WriteLog(message, LogLevel.Debug);
        }

        internal static void Error(string message)
        {
            WriteLog(message, LogLevel.Error);
        }

        internal static void Error(Exception exception)
        {
            WriteLog(exception.StackTrace, LogLevel.Error);
        }

        internal static void Error(string message, Exception exception)
        {
            WriteLog($"{message}: {exception.Message}", LogLevel.Error);
        }

        internal static void Error(string message, LogLevel logLevel)
        {
            WriteLog(message, logLevel);
        }

        internal static void Info(string message)
        {
            WriteLog(message, LogLevel.Info);
        }

        internal static void Trace(string message)
        {
            WriteLog(message, LogLevel.Trace);
        }

        internal static void Warn(string message)
        {
            WriteLog(message, LogLevel.Warn);
        }

        private static void WriteLog(string message, LogLevel level)
        {
            CitizenFX.Core.Debug.Write($"^8[{DateTime.Now.ToLocalTime().ToString("HH:mm:ss")}] ");

            switch (level)
            {
                case LogLevel.Trace:
                    CitizenFX.Core.Debug.Write("^7[Trace]");
                    break;
                case LogLevel.Debug:
                    CitizenFX.Core.Debug.Write("^6[Debug]");
                    break;
                case LogLevel.Info:
                    CitizenFX.Core.Debug.Write("^5[Info] ");
                    break;
                case LogLevel.Warn:
                    CitizenFX.Core.Debug.Write("^3[Warn] ");
                    break;
                case LogLevel.Error:
                    CitizenFX.Core.Debug.Write("^1[Error]");
                    break;
            }

            CitizenFX.Core.Debug.Write($" ^7| {message}\n");

            _logs.Add(new LogInfo(message, level));
        }

        //private static string GetSeparator()
        //{
        //    var result = "";
        //    var x = 0;
        //    var y = 0;
        //    API.GetScreenResolution(ref x, ref y);

        //    for (int i = 0; i < x - 1; i++)
        //    {
        //        result += "-";
        //    }

        //    return result;
        //}

        internal static IEnumerable<LogInfo> GetLogs(LogLevel level)
        {
            return _logs.Where(x => x.Level == level);
        }
    }
}
