using Microsoft.Toolkit.Uwp.Notifications;
using PaperTrail.Core.Services;
using Windows.UI.Notifications;
using System.Windows;

namespace PaperTrail.App.Services;

public class NotificationService : INotificationService
{
    public Task ShowAsync(string title, string message)
    {
        var content = new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .GetToastContent();

        // Use ToastNotificationManager for WPF, and marshal to UI thread if needed
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Use GetContent() to get the XML string, then load it into an XmlDocument
            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(content.GetContent());
            var toast = new ToastNotification(xmlDoc);
            var notifier = ToastNotificationManager.CreateToastNotifier("PaperTrail.App");
            notifier.Show(toast);
        });

        return Task.CompletedTask;
    }
}
