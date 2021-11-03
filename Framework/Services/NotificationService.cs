using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Services
{
    internal class NotificationService : IService
    {
        public interface INotification
        {
            string Id { get; }
            int CurrentDuration { get; set; }
            int Duration { get; }
            int FadeInDuration { get; }
            int FadeOutDuration { get; }
            bool IsCreated { get; set; }
        }

        public class NotificationModel : INotification
        {
            public string Id { get; } = RandomString();
            public string Ico { get; }
            public string Count { get; set; }
            public int CurrentDuration { get; set; }
            public int Duration { get; }
            public int FadeInDuration { get; }
            public int FadeOutDuration { get; }
            public bool IsCreated { get; set; }

            public NotificationModel(string count, string ico, int duration, int fadeInDuration, int fadeOutDuration)
            {
                Count = count;
                Ico = ico;
                Duration = duration;
                FadeInDuration = fadeInDuration;
                FadeOutDuration = fadeOutDuration;
            }
        }

        private readonly UIService _uiService;

        private readonly List<INotification> _queue = new();

        public NotificationService(UIService uiService)
        {
            _uiService = uiService;
        }

        internal void OnClientWindowInitialized()
        {
            _uiService.LoadFrame("notification");
            _uiService.SetZIndex("notification", 10000);
        }

        [Thread]
        private async Task Update()
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                var notification = _queue[i];

                if (!notification.IsCreated)
                {
                    notification.IsCreated = true;

                    switch (notification)
                    {
                        case NotificationModel n:
                            _uiService.SendNui("notification", "create", new
                            {
                                id = n.Id,
                                ico = n.Ico,
                                count = n.Count,
                                fadeInDuration = n.FadeInDuration
                            });
                            break;
                    }
                }
            }

            await BaseScript.Delay(250);
        }

        [Thread]
        private async Task UpdateState()
        {
            if (_queue.Count > 0)
            {
                Show();
            }
            else
            {
                Hide();
            }

            for (int i = 0; i < _queue.Count; i++)
            {
                var notification = _queue[i];

                if (notification.IsCreated)
                {
                    notification.CurrentDuration += 100;

                    if (notification.CurrentDuration >= notification.Duration)
                    {
                        await Remove(notification);
                    }
                }
            }

            await BaseScript.Delay(100);
        }

        private async Task Remove(INotification notification)
        {
            _uiService.SendNui("notification", "remove", new
            {
                id = notification.Id,
                fadeOutDuration = notification.FadeOutDuration
            });

            await BaseScript.Delay(notification.FadeOutDuration);

            _queue.Remove(notification);
        }

        public void Schedule(string count, string ico, int duration, int fadeInDuration, int fadeOutDuration)
        {
            _queue.Add(new NotificationModel(count, ico, duration, fadeInDuration, fadeOutDuration));
        }

        internal void Show()
        {
            _uiService.SendNui("notification", "show");
        }

        internal void Hide()
        {
            _uiService.SendNui("notification", "hide");
        }
    }
}
