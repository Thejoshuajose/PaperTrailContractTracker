using Microsoft.Toolkit.Uwp.Notifications;
using PaperTrail.Core.Services;
using Windows.UI.Notifications;

namespace PaperTrail.App.Services;

public class NotificationService : INotificationService
{
    public Task ShowAsync(string title, string message)
    {
        var content = new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .GetToastContent();

        ToastNotificationManagerCompat
            .CreateToastNotifier()
            .Show(new ToastNotification(content.GetXml()));

        return Task.CompletedTask;
    }
}
