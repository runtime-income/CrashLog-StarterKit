using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrashLog_StarterKit.Services;
using CrashLog_StarterKit.UI;
using CrashLog_StarterKit.Forms;
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        //���ػ�(HiDPI)���� ��Ʈ/��Ʈ���� �ѿ��� �ʵ��� DPI �ν� ��带 �ý��� �������� �����մϴ�.
        //(Windows�� ó�� �α����� �� ������ DPI�� �����ϴ�.)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        //������ �׸�(���־� ��Ÿ��)�� ����մϴ�.
        Application.EnableVisualStyles();  

        //���� �����Ǵ� ��Ʈ�ѵ��� �⺻ �ؽ�Ʈ ������ ������ TextRenderer(GDI) ������ �Ӵϴ�.
        //����/ȣȯ�� ���鿡�� WinForms ���� �⺻��
        Application.SetCompatibleTextRenderingDefault(false);

        //���Ͽ� �α׸� ���� ����
        var logSink = new FileLogSink();

        //���ܸ� �޾� �޽��� ���� �� ȯ�� ���� ����(EnvironmentProbe) �� ���Ϸ� ��� �� (�ʿ� ��)
        //����ڿ��� �ȳ����� ����ϴ� �߽� Ŭ����
        var reporter = new CrashReporter(logSink, new EnvironmentProbe());

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
}