using System;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += ExceptionHandler.OnUIThreadException;
        AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.OnNonUIThreadException;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DemoForm());
    }
}
