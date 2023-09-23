namespace Infrastructure.Adapters.Notification.Configs
{
    public class NotificationConfiguration
    {
        public string SendsGridConnectionString { get; set; }
        public string SendFrom { get; set; }
        public string SendTo { get; set; }
    }
}
