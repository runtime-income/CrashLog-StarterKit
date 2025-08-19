using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CrashLog_StarterKit.Services
{
    public sealed class FileLogSink : IDisposable
    {
        private readonly string _baseDir;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public FileLogSink(string baseDir = null)
        {
            _baseDir = baseDir ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CrashLogStarterKit", "logs");

            Directory.CreateDirectory(_baseDir);
        }

        public void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
            var path = Path.Combine(_baseDir, $"app-{DateTime.Now:yyyyMMdd}.log");
            SafeAppend(path, line);
        }

        public string WriteCrash(string body)
        {
            var name = $"crash-{DateTime.Now:yyyyMMdd-HHmmssfff}.log";
            var path = Path.Combine(_baseDir, name);
            SafeAppend(path, body + Environment.NewLine);
            SafeAppend(Path.Combine(_baseDir, "crash.index"), path + Environment.NewLine);
            return path;
        }

        private void SafeAppend(string path, string text)
        {
            _lock.EnterWriteLock();
            try
            {
                File.AppendAllText(path, text, Encoding.UTF8);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose() => _lock?.Dispose();
    }
}