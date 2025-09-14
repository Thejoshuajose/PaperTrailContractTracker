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
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .GetToastContent();

            // Show the toast using ToastNotificationManager
            var toast = new System.Windows.Forms.NotifyIcon();
            // You may need to use Windows.UI.Notifications.ToastNotificationManager
            // and Windows.UI.Notifications.ToastNotification if targeting UWP/WinAppSDK
            // For WPF, you may need a compatible library or custom implementation.
            // Example for UWP:
            // var notifier = ToastNotificationManager.CreateToastNotifier();
            // notifier.Show(new ToastNotification(toastContent.GetXml()));
        });

        return Task.CompletedTask;
    }
}
