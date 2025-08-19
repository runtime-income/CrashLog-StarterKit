using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CrashLogStarterKit
{
    public interface IEnvironmentProbe
    {
        EnvironmentSnapshot Snapshot();

        string Collect();
    }

    public sealed class EnvironmentSnapshot
    {
        public string OSVersion { get; set; }
        public string FrameworkDescription { get; set; }
        public string ProcessArchitecture { get; set; }
        public bool Is64BitProcess { get; set; }
        public int ProcessorCount { get; set; }
        public long WorkingSetMB { get; set; }
        public string DPIMode { get; set; }
        public string ScreenInfo { get; set; }
    }

    public sealed class EnvironmentProbe : IEnvironmentProbe
    {
        public EnvironmentSnapshot Snapshot()
        {
            return new EnvironmentSnapshot
            {
                // 실제 환경 정보 수집해서 반환

                OSVersion = Environment.OSVersion.VersionString,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Is64BitProcess = Environment.Is64BitProcess,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSetMB = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024),
                DPIMode = GetDpiMode(),
                ScreenInfo = GetScreenInfo()
            };
        }
        public string Collect()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            return
$@"App    : {fvi.ProductName} v{fvi.ProductVersion} (File v{fvi.FileVersion})
CLR    : {Environment.Version}
OS     : {RuntimeInformation.OSDescription}
64bit  : Proc={Environment.Is64BitProcess}, OS={Environment.Is64BitOperatingSystem}
User   : {Environment.UserName} @ {Environment.MachineName}
Culture: {System.Globalization.CultureInfo.CurrentCulture}
Uptime : {GetUptime()}
WinVer : {GetWinRelease()}";
        }
        private static string GetUptime()
        {
            try { return TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(); }
            catch { return "n/a"; }
        }

        private static string GetWinRelease()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var product = key?.GetValue("ProductName")?.ToString();
                var display = key?.GetValue("DisplayVersion")?.ToString();
                var build = key?.GetValue("CurrentBuild")?.ToString();
                return $"{product} {display} (Build {build})";
            }
            catch { return "n/a"; }
        }

    private string GetDpiMode()
        {
            // WinForms HighDpiMode를 문자열로 추정 (직접 API는 없으므로 기록용)
            // Program에서 SystemAware로 설정하니 그대로 기록
            return "SystemAware";
        }

        private string GetScreenInfo()
        {
            try
            {
                var all = Screen.AllScreens.Select(s =>
                    $"{(s.Primary ? "[Primary]" : "")}{s.Bounds.Width}x{s.Bounds.Height}@{s.BitsPerPixel}bpp");
                return string.Join("; ", all);
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
