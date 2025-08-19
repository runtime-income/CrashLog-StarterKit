using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CrashLogStarterKit
{
    public partial class CrashDialog : Form
    {
        private readonly string _path;

        public CrashDialog(string report)
        {
           // InitializeComponent();

            Text = "⚠ 프로그램 오류 발생";
            Width = 900; Height = 650;

            var txt = new TextBox
            {
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both, WordWrap = false, Text = report
            };

            var btnClose = new Button { Text = "닫기", Width = 80 };
            btnClose.Click += (s, e) => Close();

            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45 };
            panel.Controls.AddRange(new Control[] {  btnClose });

            Controls.Add(txt);
            Controls.Add(panel);
        }
    }
}