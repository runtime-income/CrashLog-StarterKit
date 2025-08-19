
using CrashLogStarterKit;
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        //���ػ�(HiDPI)���� ��Ʈ/��Ʈ���� �ѿ��� �ʵ��� DPI �ν� ��带 �ý��� �������� �����մϴ�.
        //(Windows�� ó�� �α����� �� ������ DPI�� �����ϴ�.)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        //������ �׸�(���־� ��Ÿ��)�� ����մϴ�.
        Application.EnableVisualStyles();  

        //���� �����Ǵ� ��Ʈ�ѵ��� �⺻ �ؽ�Ʈ ������ ������ TextRenderer(GDI) ������ �Ӵϴ�.
        //����/ȣȯ�� ���鿡�� WinForms ���� �⺻��
        Application.SetCompatibleTextRenderingDefault(false);

        // 1) ���� �ܰ� �⺻�� ����
        var defaultBehavior = GetBuildDefaultBehavior();

        // 2) �����/ȯ�溯��/App.config�� �����
        var behavior = ResolveCrashBehavior(args, defaultBehavior);

        //���Ͽ� �α׸� ���� ����
        ILogSink logSink = new FileLogSink(appName: "CrashLogStarterKit");

        //���� ���� ȯ��(OS/���μ���/�޸�/DPI �� ���� ������)
        IEnvironmentProbe probe = new EnvironmentProbe();

        //���ܸ� �޾� �޽��� ���� �� ȯ�� ���� ����(EnvironmentProbe) �� ���Ϸ� ��� �� (�ʿ� ��)
        //����ڿ��� �ȳ����� ����ϴ� �߽� Ŭ����
        var reporter = new CrashReporter(logSink, probe, behavior);

        //WinForms �޽��� �������� �߻��� UI ������ ���ܸ� �⺻ ũ���� ��ȭ���� ��� Application.ThreadException �̺�Ʈ�� �������� �����մϴ�.
        //(�� ���� �־�� �Ʒ� ThreadException �ڵ鷯�� Ȯ���� �����մϴ�.)
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        //��ư Ŭ��, �� �̺�Ʈ, Ÿ�̸� �� UI �����忡�� ������ ���ܰ� ����� �ɴϴ�.
        //���� ����ڰ� �������� �� �ٷ� �����¡� ������ �� ����� ���´ٰ� ���� �˴ϴ�.
        Application.ThreadException += (s, e) => reporter.Handle("UIThreadException", e.Exception);

        //Task.Run, async �۾����� ���ܰ� �߻��ߴµ� �� Task�� await/ Wait() / Result�� �������� ���� ä ������ �÷��ǿ� ���ŵ� �� �ߴ� �̺�Ʈ�Դϴ�.
        //e.SetObserved()�� ȣ���� �����ܸ� �����ߴ١��� ǥ���ϸ� .NET�� ���μ����� �����ϴ� �⺻ ������(������ ����) �����մϴ�.���� �������� ��� ��ʿ���.
        //��, ����׶��� Task���� �� ���ܸ� �ƹ��� �� ì���� ������ ������ �������Դϴ�
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            e.SetObserved();
            reporter.Handle("TaskUnobserved", e.Exception);
        };

        //� �����忡���� ó������ ���� ���ܰ� ���ø����̼� ��踦 ����� ����� �ɴϴ�(������ ����).
        //isTerminating�� true�� .NET�� ���μ����� �����Ϸ��� ��Ȳ�Դϴ�.
        //���⼭�� �α׸� �ִ��� ����� �����ϴ� �� �ܿ��� ���� ���� ������ ����ϸ� �� �˴ϴ�.

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("Unknown fatal error.");
            reporter.Handle("DomainUnhandled", ex, isTerminating: e.IsTerminating);
        };

        Application.Run(new MainForm(reporter));
    }

    /// <summary>
    /// ���� �ܰ� �Ǵ� ���� �ܰ迡 ���� ���α׷� ���� ����
    /// </summary>
    /// <returns></returns>
    private static CrashBehavior GetBuildDefaultBehavior()
    {
#if DEBUG
        return CrashBehavior.Continue; // ���� �ܰ� �⺻
#else
            return CrashBehavior.Exit;     // ���� �ܰ� �⺻
#endif
    }
    /// <summary>
    /// ���� �߻� �� ��� ó������ ��å(CrashBehavior)�� �����ؼ� reporter�� �Ѱ��ִ� �κ�
    /// Continue, Exit, ExitIfTerminating, PromptThenExit �� �ϳ�
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static CrashBehavior ResolveCrashBehavior(string[] args, CrashBehavior fallback)
    {
        // 1) �����: /crash=exit | continue | exitIfTerminating | prompt
        foreach (var a in args)
        {
            if (a.StartsWith("/crash=", StringComparison.OrdinalIgnoreCase))
            {
                var v = a.Substring("/crash=".Length);
                if (TryParseBehavior(v, out var b)) return b;
            }
        }

        // 2) ȯ�溯��: APP_CRASH_BEHAVIOR
        var env = Environment.GetEnvironmentVariable("APP_CRASH_BEHAVIOR");
        if (!string.IsNullOrWhiteSpace(env) && TryParseBehavior(env, out var b2))
            return b2;

        // 3) App.config: <add key="CrashBehavior" value="exit" />
        try
        {
            var cfg = System.Configuration.ConfigurationManager.AppSettings["CrashBehavior"];
            if (!string.IsNullOrWhiteSpace(cfg) && TryParseBehavior(cfg, out var b3))
                return b3;
        }
        catch { /* ConfigurationManager �̻�� ���� */ }

        

        // 4) ���� �⺻��
        return fallback;

        // return CrashBehavior.Exit; // �⺻: ���� �� ��� ����
    }

    private static bool TryParseBehavior(string raw, out CrashBehavior behavior)
    {
        behavior = CrashBehavior.Exit;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var v = raw.Trim().ToLowerInvariant();
        switch (v)
        {
            case "continue":
                behavior = CrashBehavior.Continue; return true;
            case "exit":
                behavior = CrashBehavior.Exit; return true;
            case "exitifterminating":
            case "exit_if_terminating":
            case "exit-if-terminating":
                behavior = CrashBehavior.ExitIfTerminating; return true;
            case "prompt":
            case "promptthenexit":
            case "prompt_then_exit":
                behavior = CrashBehavior.PromptThenExit; return true;
            default:
                return false;
        }
    }
}