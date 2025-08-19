using System;
using System.Text;
using System.Windows.Forms;

namespace CrashLog_StarterKit.Services
{
    public sealed class CrashReporter
    {
        private readonly FileLogSink _log;
        private readonly EnvironmentProbe _probe;
        private int _handling;

        public CrashReporter(FileLogSink log, EnvironmentProbe probe)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _probe = probe ?? throw new ArgumentNullException(nameof(probe));
        }

        public void Info(string msg) => _log.Write("INFO", msg);

        public void Handle(string source, Exception ex, bool isTerminating = false)
        {
            if (ex == null) ex = new Exception("Unknown exception");

            if (System.Threading.Interlocked.Exchange(ref _handling, 1) == 1)
            {
                _log.Write("ERROR", $"Reentrant crash from {source}: {ex}");
                return;
            }

            try
            {
                var env = _probe.Collect();
                var body = BuildReport(source, ex, env, isTerminating);
                var path = _log.WriteCrash(body);

                using (var dlg = new UI.CrashDialog(body, path))
                {
                    dlg.ShowDialog();
                }
            }
            catch (Exception inner)
            {
                try { _log.Write("FATAL", $"CrashReporter failed: {inner}"); } catch { }
            }
            finally
            {
                _handling = 0;
                if (isTerminating)
                {
                    try { Application.Exit(); } catch { }
                }
            }
        }

        private static string BuildReport(string source, Exception ex, string env, bool isTerminating)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==== Crash Report ====");
            sb.AppendLine($"Time   : {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Source : {source}");
            sb.AppendLine($"Terminating: {isTerminating}");
            sb.AppendLine();
            sb.AppendLine("---- Environment ----");
            sb.AppendLine(env);
            sb.AppendLine();
            sb.AppendLine("---- Exception ----");
            sb.AppendLine(ex.ToString());
            sb.AppendLine();
            var ie = ex.InnerException;
            while (ie != null)
            {
                sb.AppendLine(ie.ToString());
                ie = ie.InnerException;
            }
            return sb.ToString();
        }
    }
}