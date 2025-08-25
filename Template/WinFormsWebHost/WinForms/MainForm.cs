using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.WinForms;

namespace WinFormsWebHost.WinForms;

public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly Settings _settings;
    private readonly ThemeHelper _themeHelper;

    private readonly WebView2 _webView = new()
    {
        Dock = DockStyle.Fill
    };

    public MainForm(ILogger<MainForm> logger, IHubContext<AppHub> hubContext, Settings settings, ThemeHelper themeHelper)
    {
        _logger = logger;
        _hubContext = hubContext;
        _settings = settings;
        _themeHelper = themeHelper;
        InitializeComponent();
        Load += MainForm_Load;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            Controls.Add(_webView);

            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Navigate($"http://localhost:{_settings.FrontedPort}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load WebView2");
            Close();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        SetTheme();
    }

    protected override void WndProc(ref Message m)
    {
        const int wmSettingChange = 0x001A;

        if (m.Msg == wmSettingChange)
        {
            SetTheme();
        }

        base.WndProc(ref m);
    }

    private void SetTheme()
    {
        var dark = _themeHelper.IsDarkMode();
        ThemeHelper.UseImmersiveDarkMode(Handle, dark);
        BackColor = dark ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
        ForeColor = dark ? Color.White : SystemColors.ControlText;

        _hubContext.Clients.All.SendAsync("ReceiveTheme", dark ?
            nameof(AppTheme.Dark) : nameof(AppTheme.Light));
    }
}