using System;
using System.Threading;
using System.Windows.Forms;

public class DemoForm : Form
{
    private Button btnUiCrash = new Button(){ Text = "UI 예외"};
    private Button btnBgCrash = new Button(){ Text = "백그라운드 예외"};

    public DemoForm()
    {
        Text = "Crash & Log Demo";
        Width = 360; Height = 180;
        btnUiCrash.SetBounds(20, 20, 140, 40);
        btnBgCrash.SetBounds(180, 20, 140, 40);
        Controls.Add(btnUiCrash);
        Controls.Add(btnBgCrash);

        btnUiCrash.Click += (_, __) => throw new InvalidOperationException("UI thread crash test");
        btnBgCrash.Click += (_, __) => new Thread(() => throw new ApplicationException("Background crash test")).Start();

        Load += (_, __) => Log.Info("DemoForm loaded.");
    }
}
