using Notifications.Wpf.Core;

namespace ClipsArchiver.Services;

public class ToastNotificationService
{
    private static readonly NotificationManager NotificationManager = new();
    
    public static void ShowInfoNotification(string title, string message)
    {
        NotificationManager.ShowAsync(new NotificationContent
        {
            Title = title,
            Message = message,
            Type = NotificationType.Information
        }, expirationTime: TimeSpan.FromSeconds(2));
    }
    
    public static void ShowErrorNotification(string title, string message)
    {
        NotificationManager.ShowAsync(new NotificationContent
        {
            Title = title,
            Message = message,
            Type = NotificationType.Error
        }, expirationTime: TimeSpan.FromSeconds(2));
    }

    public static void ShowSuccessNotification(string title, string message)
    {
        NotificationManager.ShowAsync(new NotificationContent
        {
            Title = title,
            Message = message,
            Type = NotificationType.Success
        }, expirationTime: TimeSpan.FromSeconds(2));
    }
    
    public static void ShowWarningNotification(string title, string message)
    {
        NotificationManager.ShowAsync(new NotificationContent
        {
            Title = title,
            Message = message,
            Type = NotificationType.Warning
        }, expirationTime: TimeSpan.FromSeconds(2));
    }
}