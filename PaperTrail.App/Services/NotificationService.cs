using Microsoft.Toolkit.Uwp.Notifications;
using PaperTrail.Core.Services;
using System.Windows;

namespace PaperTrail.App.Services;

public class NotificationService : INotificationService
{
    public Task ShowAsync(string title, string message)
    {
        // Display the toast notification using the toolkit's compatibility layer.
        Application.Current.Dispatcher.Invoke(() =>
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        });

        return Task.CompletedTask;
    }
}
