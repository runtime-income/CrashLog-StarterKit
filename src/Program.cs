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
        //고해상도(HiDPI)에서 폰트/컨트롤이 뿌옇지 않도록 DPI 인식 모드를 시스템 기준으로 설정합니다.
        //(Windows가 처음 로그인할 때 결정한 DPI를 따릅니다.)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        //윈도우 테마(비주얼 스타일)를 사용합니다.
        Application.EnableVisualStyles();  

        //새로 생성되는 컨트롤들의 기본 텍스트 렌더링 엔진을 TextRenderer(GDI) 쪽으로 둡니다.
        //성능/호환성 측면에서 WinForms 권장 기본값
        Application.SetCompatibleTextRenderingDefault(false);

        //파일에 로그를 쓰는 구현
        var logSink = new FileLogSink();

        //예외를 받아 메시지 구성 → 환경 정보 수집(EnvironmentProbe) → 파일로 기록 → (필요 시)
        //사용자에게 안내까지 담당하는 중심 클래스
        var reporter = new CrashReporter(logSink, new EnvironmentProbe());

        //WinForms 메시지 루프에서 발생한 UI 스레드 예외를 기본 크래시 대화상자 대신 Application.ThreadException 이벤트로 보내도록 설정합니다.
        //(이 줄이 있어야 아래 ThreadException 핸들러가 확실히 동작합니다.)
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        //버튼 클릭, 폼 이벤트, 타이머 등 UI 스레드에서 던져진 예외가 여기로 옵니다.
        //보통 사용자가 “눌렀을 때 바로 터지는” 오류는 다 여기로 들어온다고 보면 됩니다.
        Application.ThreadException += (s, e) => reporter.Handle("UIThreadException", e.Exception);

        //Task.Run, async 작업에서 예외가 발생했는데 그 Task의 await/ Wait() / Result로 관찰되지 않은 채 가비지 컬렉션에 수거될 때 뜨는 이벤트입니다.
        //e.SetObserved()를 호출해 “예외를 관찰했다”고 표시하면 .NET이 프로세스를 종료하는 기본 동작을(버전에 따라) 억제합니다.예방 차원에서 모범 사례예요.
        //즉, “백그라운드 Task에서 난 예외를 아무도 안 챙겼을 때”의 마지막 안전망입니다
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            e.SetObserved();
            reporter.Handle("TaskUnobserved", e.Exception);
        };

        //어떤 스레드에서든 처리되지 않은 예외가 애플리케이션 경계를 벗어나면 여기로 옵니다(최후의 보루).
        //isTerminating가 true면 .NET이 프로세스를 종료하려는 상황입니다.
        //여기서는 로그를 최대한 남기고 정리하는 것 외에는 정상 지속 실행을 기대하면 안 됩니다.

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("Unknown fatal error.");
            reporter.Handle("DomainUnhandled", ex, isTerminating: e.IsTerminating);
        };

        Application.Run(new MainForm(reporter));
    }
}