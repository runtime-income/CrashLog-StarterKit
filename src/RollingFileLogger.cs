using System;
using System.IO;
using System.Text;
using System.Threading;

public sealed class RollingFileLogger
{
    private static readonly Lazy<RollingFileLogger> _instance = new Lazy<RollingFileLogger>(() => new RollingFileLogger());
    public static RollingFileLogger Instance => _instance.Value;

    private readonly object _lock = new object();
    private readonly string _logDir;
    private readonly string _baseName = "app";   // app.log, app.1.log ...
    private readonly long _maxBytes = 5 * 1024 * 1024; // 5 MB
    private readonly int _maxFiles = 5;

    private RollingFileLogger()
    {
        _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDir);
    }

    public void Write(string level, string message, Exception ex = null)
    {
        var line = FormatLine(level, message, ex);
        lock (_lock)
        {
            var path = Path.Combine(_logDir, $"{_baseName}.log");
            using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                sw.WriteLine(line);
            }
            TryRotate(path);
        }
    }

    private string FormatLine(string level, string message, Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        sb.Append(" ");
        sb.Append(level);
        sb.Append(" ");
        sb.Append(Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3));
        sb.Append(" | ");
        sb.Append(message);
        if (ex != null)
        {
            sb.Append(" | ");
            sb.Append(ex.GetType().FullName);
            sb.Append(" : ");
            sb.Append(ex.Message);
            sb.Append("\n");
            sb.Append(ex.StackTrace);
        }
        return sb.ToString();
    }

    private void TryRotate(string path)
    {
        var fi = new FileInfo(path);
        if (fi.Exists && fi.Length > _maxBytes)
        {
            for (int i = _maxFiles - 1; i >= 1; i--)
            {
                var src = Path.Combine(_logDir, $"{_baseName}.{i}.log");
                var dst = Path.Combine(_logDir, $"{_baseName}.{i + 1}.log");
                if (File.Exists(dst)) File.Delete(dst);
                if (File.Exists(src)) File.Move(src, dst);
            }
            var first = Path.Combine(_logDir, $"{_baseName}.1.log");
            if (File.Exists(first)) File.Delete(first);
            File.Move(path, first);
        }
    }
}
