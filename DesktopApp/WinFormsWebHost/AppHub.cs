using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace WinFormsWebHost;

public class AppHub(ILogger<AppHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var dark = ThemeHelper.IsDarkMode();
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