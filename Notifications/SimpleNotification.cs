using SDK.Client.Interfaces;
using static SDK.Client.GameAPI;

namespace Average.Client.Notifications
{
    public class SimpleNotification : INotification
    {
        public string Id { get; } = RandomString();
        public string Title { get; }
        public string Content { get; }
        public int CurrentDuration { get; set; }
        public int Duration { get; }
        public bool IsCreated { get; set; }

        public SimpleNotification(string title, string content, int duration)
        {
            Title = title;
            Content = content;
            Duration = duration;
        }
    }
}
