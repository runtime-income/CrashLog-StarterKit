using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CrashLog_StarterKit.Services
{
    public sealed class EnvironmentProbe
    {
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
    }
}