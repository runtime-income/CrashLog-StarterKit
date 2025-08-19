using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CrashLog_StarterKit.UI
{
    public partial class CrashDialog : Form
    {
        private readonly string _path;

        public CrashDialog(string report, string savedPath)
        {
           // InitializeComponent();
            _path = savedPath;
            Text = "⚠ 프로그램 오류 발생";
            Width = 900; Height = 650;

            var txt = new TextBox
            {
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both, WordWrap = false, Text = report
            };

            var btnCopy = new Button { Text = "복사", Width = 80 };
            btnCopy.Click += (s, e) => { Clipboard.SetText(report); MessageBox.Show("복사됨"); };

            var btnOpen = new Button { Text = "로그 열기", Width = 100 };
            btnOpen.Click += (s, e) =>
            {
                try { Process.Start("explorer.exe", $"/select,\"{_path}\""); } catch { }
            };

            var btnClose = new Button { Text = "닫기", Width = 80 };
            btnClose.Click += (s, e) => Close();

            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45 };
            panel.Controls.AddRange(new Control[] { btnCopy, btnOpen, btnClose });

            Controls.Add(txt);
            Controls.Add(panel);
        }
    }
}