using Microsoft.AspNetCore.SignalR;

namespace WinFormsWebHost;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var dark = ThemeHelper.IsDarkMode();
        await Clients.All.SendAsync("ReceiveTheme", dark ?
            nameof(AppTheme.Dark) : nameof(AppTheme.Light));
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}