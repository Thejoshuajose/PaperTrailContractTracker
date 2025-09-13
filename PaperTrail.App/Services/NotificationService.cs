using Microsoft.Toolkit.Uwp.Notifications;
using PaperTrail.Core.Services;

namespace PaperTrail.App.Services;

public class NotificationService : INotificationService
{
    public Task ShowAsync(string title, string message)
    {
        new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .Show();
        return Task.CompletedTask;
    }
}
