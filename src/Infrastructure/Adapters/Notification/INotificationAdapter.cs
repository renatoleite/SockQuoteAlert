using Infrastructure.Adapters.Notification.Models;

namespace Infrastructure.Adapters.Notification
{
    public interface INotificationAdapter
    {
        Task SendAsync(NotificationContent notification, CancellationToken token);
    }
}
