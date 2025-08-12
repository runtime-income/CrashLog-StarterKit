using System;

public static class Log
{
    public static void Info(string message) => RollingFileLogger.Instance.Write("INFO", message);
    public static void Warn(string message) => RollingFileLogger.Instance.Write("WARN", message);
    public static void Error(string message, Exception ex = null) => RollingFileLogger.Instance.Write("ERROR", message, ex);
}
