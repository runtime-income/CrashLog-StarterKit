using Microsoft.VisualBasic.Logging;
using System;
using System.Text;
using System.Windows.Forms;

namespace CrashLogStarterKit
{
    public enum CrashBehavior
    {
        Continue,          // 로그만 남기고 계속 실행
        Exit,              // 항상 즉시 종료
        ExitIfTerminating, // .NET이 종료하려는 상황일 때만 종료 (UnhandledException: isTerminating=true)
        PromptThenExit     // 사용자에게 알리고 종료 (원하면 생략 가능)
    }

    /// <summary>
    ///  CrashReporter는 예외가 발생하면 probe.Snapshot()을 호출해서 환경 정보를 로그에 첨부
    /// </summary>
    public sealed class CrashReporter
    {
        private readonly ILogSink _sink;
        private readonly IEnvironmentProbe _probe;
        private readonly CrashBehavior _behavior;

        public CrashReporter(ILogSink sink, IEnvironmentProbe probe, CrashBehavior behavior)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _probe = probe ?? throw new ArgumentNullException(nameof(probe));
            _behavior = behavior;
        }

        public void Handle(string source, Exception ex, bool isTerminating = false)
        {
            try
            {
                var snapshot = _probe.Snapshot();
                _sink.Write(DateTime.Now, source, ex, snapshot);
                _sink.Flush();
            }
            catch
            {
                // 로깅 실패는 앱 생존에 영향 주지 않도록 무시
            }

            switch (_behavior)
            {
                case CrashBehavior.Continue:
                    // 아무것도 안 하고 계속 진행
                    TryShowMessage(source, ex, isTerminating);
                    break;

                case CrashBehavior.ExitIfTerminating:
                    if (isTerminating)
                        ForceExit(1);
                    break;

                case CrashBehavior.Exit:
                    TryShowMessage(source, ex, isTerminating);
                    ForceExit(1);
                    break;

                case CrashBehavior.PromptThenExit:
                    TryShowMessage(source,ex, isTerminating);
                    ForceExit(1);
                    break;
            }
        }

        private void TryShowMessage(string source, Exception ex, bool isTerminating = false)
        {
            try
            {
                var env = _probe.Collect();
                var body = BuildReport(source, ex, env, isTerminating);
               
                using (var dlg = new CrashDialog(body))
                {
                    dlg.ShowDialog();
                }
            }
            catch (Exception inner)
            {
               
            }
            finally
            {
         
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
        private void ForceExit(int code)
        {
            try { _sink.Flush(); } catch { }
            Environment.Exit(code); // 즉시 종료
        }
    }
}