using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace WinFormsVue.WinForms;

public class AppHub(ILogger<AppHub> logger, ThemeHelper themeHelper) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var dark = themeHelper.IsDarkMode();
        await Clients.All.SendAsync("ReceiveTheme", dark ?
            nameof(AppTheme.Dark) : nameof(AppTheme.Light));
    }

    public void SendCommand(string commandType)
    {
        logger.LogInformation("Command {Command} invoked", commandType);
        Clients.All.SendAsync("receiveReport", RandomColor(), commandType);
    }

    private static string RandomColor()
    {
        string[] colors = ["red", "green", "blue", "yellow", "orange"];
        return colors[Random.Shared.Next(colors.Length)];
    }
}