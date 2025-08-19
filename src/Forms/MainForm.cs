using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrashLogStarterKit
{
    public partial class MainForm : Form
    {
        private readonly CrashReporter _reporter;

        public MainForm(CrashReporter reporter)
        {
            //InitializeComponent();
            _reporter = reporter;
            Text = "CrashLog Starter Kit - Demo";

            var btnUi = new Button { Text = "UI 예외", Dock = DockStyle.Top, Height = 40 };
            btnUi.Click += (s, e) => throw new InvalidOperationException("UI thread exception test");

            var btnTask = new Button { Text = "Task 예외", Dock = DockStyle.Top, Height = 40 };
            btnTask.Click += async (s, e) =>
            {
                await Task.Run(() => throw new Exception("Background task exception"));
                GC.Collect(); GC.WaitForPendingFinalizers();
            };

            var btnLog = new Button { Text = "로그 남기기", Dock = DockStyle.Top, Height = 40 };
            //btnLog.Click += (s, e) => _reporter.Info("User clicked log");

            Controls.Add(btnLog);
            Controls.Add(btnTask);
            Controls.Add(btnUi);
        }
    }
}