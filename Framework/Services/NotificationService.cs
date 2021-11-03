using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Services
{
    internal class NotificationService : IService
    {
        internal interface INotification
        {
            string Id { get; }
            int CurrentDuration { get; set; }
            int Duration { get; }
            int FadeInDuration { get; }
            int FadeOutDuration { get; }
            bool IsCreated { get; set; }
        }

        internal enum NotificationCountType : int
        {
            Decrease,
            Increase
        }

        internal class NotificationIcoModel : INotification
        {
            public string Id { get; } = RandomString();
            public string IcoPath { get; }
            public NotificationCountType CountType { get; set; }
            public int Count { get; set; }
            public int CurrentDuration { get; set; }
            public int Duration { get; }
            public int FadeInDuration { get; }
            public int FadeOutDuration { get; }
            public bool IsCreated { get; set; }

            public NotificationIcoModel(NotificationCountType countType, int count, string icoPath, int duration, int fadeInDuration, int fadeOutDuration)
            {
                CountType = countType;
                Count = count;
                IcoPath = icoPath;
                Duration = duration;
                FadeInDuration = fadeInDuration;
                FadeOutDuration = fadeOutDuration;
            }
        }

        internal class NotificationStoreModel : INotification
        {
            public string Id { get; } = RandomString();
            public NotificationCountType CountType { get; set; }
            public string Text { get; set; }
            public int Count { get; set; }
            public int CurrentDuration { get; set; }
            public int Duration { get; }
            public int FadeInDuration { get; }
            public int FadeOutDuration { get; }
            public bool IsCreated { get; set; }

            public NotificationStoreModel(NotificationCountType countType, string text, int count, int duration, int fadeInDuration, int fadeOutDuration)
            {
                CountType = countType;
                Text = text;
                Count = count;
                Duration = duration;
                FadeInDuration = fadeInDuration;
                FadeOutDuration = fadeOutDuration;
            }
        }

        internal class NotificationHelpTextModel : INotification
        {
            public string Id { get; } = RandomString();
            public string Text { get; set; }
            public int CurrentDuration { get; set; }
            public int Duration { get; }
            public int FadeInDuration { get; }
            public int FadeOutDuration { get; }
            public bool IsCreated { get; set; }

            public NotificationHelpTextModel(string text, int duration, int fadeInDuration, int fadeOutDuration)
            {
                Text = text;
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

        #region Thread

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
                        case NotificationIcoModel n:
                            _uiService.SendNui("notification", "create", new
                            {
                                id = n.Id,
                                ico = n.IcoPath,
                                count = GetNotificationCountTypeString(n.CountType) + n.Count,
                                fadeInDuration = n.FadeInDuration,
                                type = n.GetType().Name
                            });
                            break;
                        case NotificationStoreModel n:
                            _uiService.SendNui("notification", "create", new
                            {
                                id = n.Id,
                                text = GetNotificationCountTypeString(n.CountType) + n.Count + " " + n.Text,
                                fadeInDuration = n.FadeInDuration,
                                type = n.GetType().Name
                            });
                            break;
                        case NotificationHelpTextModel n:
                            _uiService.SendNui("notification", "create", new
                            {
                                id = n.Id,
                                text = n.Text,
                                fadeInDuration = n.FadeInDuration,
                                type = n.GetType().Name
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
                //Hide();
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

        #endregion

        #region Logic

        private string GetNotificationCountTypeString(NotificationCountType countType)
        {
            switch (countType)
            {
                case NotificationCountType.Decrease:
                    return "-";
                case NotificationCountType.Increase:
                    return "+";
            }

            return default;
        }

        private async Task Remove(INotification notification)
        {
            _uiService.SendNui("notification", "remove", new
            {
                id = notification.Id,
                fadeOutDuration = notification.FadeOutDuration,
                type = notification.GetType().Name
            });

            Logger.Error("wait: " + notification.FadeOutDuration);

            await BaseScript.Delay(notification.FadeOutDuration);

            Logger.Error("remove");
            _queue.Remove(notification);
        }

        public void Schedule(INotification notification)
        {
            _queue.Add(notification);
        }

        internal void Show()
        {
            _uiService.SendNui("notification", "show");
        }

        internal void Hide()
        {
            _uiService.SendNui("notification", "hide");
        }

        #endregion
    }
}
