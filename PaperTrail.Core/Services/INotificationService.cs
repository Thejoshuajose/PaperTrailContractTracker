namespace PaperTrail.Core.Services;

public interface INotificationService
{
    Task ShowAsync(string title, string message);
}
