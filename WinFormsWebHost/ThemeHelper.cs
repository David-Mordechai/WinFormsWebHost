using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WinFormsWebHost;

public enum AppTheme
{
    Light,
    Dark
}

public static class ThemeHelper
{
    public static bool IsDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int lightTheme)
            {
                return lightTheme == 0;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attr,
        ref int attrValue,
        int attrSize);

    private const int DwmWaUseImmersiveDarkMode = 20; // For Win10 1809 and later

    public static void UseImmersiveDarkMode(IntPtr handle, bool enabled)
    {
        if (!IsWindows10OrGreater(17763)) return; // build 17763 = Win10 1809
        var useImmersiveDarkMode = enabled ? 1 : 0;
        DwmSetWindowAttribute(handle,
            DwmWaUseImmersiveDarkMode,
            ref useImmersiveDarkMode,
            sizeof(int));
    }

    private static bool IsWindows10OrGreater(int build = -1)
    {
        var v = Environment.OSVersion.Version;
        return (v.Major >= 10 && v.Build >= build);
    }
}