using Microsoft.VisualBasic.Logging;
using System;
using System.Text;
using System.Windows.Forms;

namespace CrashLogStarterKit
{
    public enum CrashBehavior
    {
        Continue,          // �α׸� ����� ��� ����
        Exit,              // �׻� ��� ����
        ExitIfTerminating, // .NET�� �����Ϸ��� ��Ȳ�� ���� ���� (UnhandledException: isTerminating=true)
        PromptThenExit     // ����ڿ��� �˸��� ���� (���ϸ� ���� ����)
    }

    /// <summary>
    ///  CrashReporter�� ���ܰ� �߻��ϸ� probe.Snapshot()�� ȣ���ؼ� ȯ�� ������ �α׿� ÷��
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
                // �α� ���д� �� ������ ���� ���� �ʵ��� ����
            }

            switch (_behavior)
            {
                case CrashBehavior.Continue:
                    // �ƹ��͵� �� �ϰ� ��� ����
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
            Environment.Exit(code); // ��� ����
        }
    }
}