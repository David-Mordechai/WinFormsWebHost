using WinFormsWebHost.WinForms;

namespace WinFormsWebHost;

public class Settings
{
    public int BackendPort { get; set; } = 5000;
    public int FrontedPort { get; set; } = 5000;
    public AppTheme ThemeMode { get; set; }
}