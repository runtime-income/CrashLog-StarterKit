using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CrashLogStarterKit
{
    public interface ILogSink
    {
        void Write(DateTime ts, string source, Exception ex, EnvironmentSnapshot snapshot);
        void Flush();
    }

    public sealed class FileLogSink : ILogSink
    {
        private readonly object _lock = new object();
        private readonly string _dir;
        private readonly string _appName;

        public FileLogSink(string appName = "CrashLogStarterKit")
        {
            _appName = string.IsNullOrWhiteSpace(appName) ? "CrashLogStarterKit" : appName;
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dir = Path.Combine(root, _appName, "logs");
            Directory.CreateDirectory(_dir);
        }

        public void Write(DateTime ts, string source, Exception ex, EnvironmentSnapshot snapshot)
        {
            var file = Path.Combine(_dir, $"{_appName}-{ts:yyyyMMdd}.log");
            var sb = new StringBuilder();

            sb.AppendLine(new string('=', 80));
            sb.AppendLine($"Time      : {ts:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Source    : {source}");
            sb.AppendLine($"Message   : {ex?.Message}");
            sb.AppendLine($"Type      : {ex?.GetType().FullName}");
            sb.AppendLine($"Stack     : {ex?.StackTrace}");
            if (ex?.InnerException != null)
            {
                sb.AppendLine("-- Inner Exception ------------------------------------------------------");
                sb.AppendLine($"Message   : {ex.InnerException.Message}");
                sb.AppendLine($"Type      : {ex.InnerException.GetType().FullName}");
                sb.AppendLine($"Stack     : {ex.InnerException.StackTrace}");
            }
            sb.AppendLine("-- Environment -----------------------------------------------------------");
            sb.AppendLine($"OS        : {snapshot.OSVersion}");
            sb.AppendLine($"Arch      : {snapshot.ProcessArchitecture}");
            sb.AppendLine($".NET      : {snapshot.FrameworkDescription}");
            sb.AppendLine($"ProcBits  : {snapshot.Is64BitProcess}");
            sb.AppendLine($"CPU Count : {snapshot.ProcessorCount}");
            sb.AppendLine($"WorkingMB : {snapshot.WorkingSetMB}");
            sb.AppendLine($"DPI Mode  : {snapshot.DPIMode}");
            sb.AppendLine($"Monitors  : {snapshot.ScreenInfo}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            lock (_lock)
            {
                File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
            }
        }

        public void Flush()
        {
            // File.AppendAllText는 즉시 기록하므로 별도 Flush 불필요
            // 필요한 경우 스트림 캐시 사용하도록 변경 가능
        }
    }
}
