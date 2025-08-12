using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public static class ExceptionHandler
{
    public static void OnUIThreadException(object sender, ThreadExceptionEventArgs e)
    {
        try
        {
            Log.Error("[UI] Unhandled exception", e.Exception);
            ShowDialog(e.Exception, isUi:true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"UI 예외 처리 중 오류: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void OnNonUIThreadException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("Unknown non-UI exception");
            Log.Error("[Non-UI] Unhandled exception", ex);
            ShowDialog(ex, isUi:false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Non-UI 예외 처리 중 오류: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void ShowDialog(Exception ex, bool isUi)
    {
        var sb = new StringBuilder();
        sb.AppendLine(isUi ? "UI 스레드에서 처리되지 않은 예외가 발생했습니다." : "백그라운드 스레드에서 처리되지 않은 예외가 발생했습니다.");
        sb.AppendLine();
        sb.AppendLine($"Message : {ex.Message}");
        sb.AppendLine($"Type    : {ex.GetType().FullName}");
        sb.AppendLine($"Source  : {ex.Source}");
        sb.AppendLine($"Target  : {ex.TargetSite}");
        sb.AppendLine();
        sb.AppendLine("StackTrace:");
        sb.AppendLine(ex.StackTrace);

        MessageBox.Show(sb.ToString(), "처리되지 않은 예외", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
