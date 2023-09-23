namespace Infrastructure.Adapters.Notification.Exceptions
{
    public class NotificationException : Exception
    {
        public NotificationException(string message) : base(message) { }
        public NotificationException(string message, Exception ex) : base(message, ex) { }
    }
}
