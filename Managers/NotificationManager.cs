using Average.Client.Notifications;
using CitizenFX.Core;
using SDK.Client.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDK.Client;

namespace Average.Client.Managers
{
    public class NotificationManager : InternalPlugin, INotificationManager
    {
        private readonly List<INotification> _queue = new List<INotification>();

        private const int UpdateStateInterval = 100;

        public override void OnInitialized()
        {
            #region Event

            EventManager.RegisterInternalNuiCallbackEvent("window_ready", WindowReady);
            EventManager.RegisterInternalNuiCallbackEvent("notification/avg.ready", Ready);

            #endregion

            #region Thread

            Thread.StartThread(Update);
            Thread.StartThread(UpdateState);

            #endregion
        }

        #region NUI

        private CallbackDelegate WindowReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            // Load menu in html page
            SendNUI(new
            {
                eventName = "avg.internal.load",
                plugin = "notification",
                fileName = "index.html"
            });
            return result;
        }

        private CallbackDelegate Ready(IDictionary<string, object> data, CallbackDelegate result)
        {
            SendNUI(new
            {
                eventName = "avg.internal",
                on = "notification.open",
                plugin = "notification"
            });

            return result;
        }

        #endregion

        #region Thread

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
                        case SimpleNotification n:
                            SendNUI(new
                            {
                                eventName = "avg.internal",
                                on = "notification.create",
                                plugin = "notification",
                                id = n.Id,
                                title = n.Title,
                                content = n.Content,
                                duration = n.Duration
                            });
                            break;
                    }
                }
            }

            await BaseScript.Delay(250);
        }

        private async Task UpdateState()
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                var notification = _queue[i];

                if (notification.IsCreated)
                {
                    notification.CurrentDuration += UpdateStateInterval;

                    if (notification.CurrentDuration >= notification.Duration)
                        Delete(notification);
                }
            }

            await BaseScript.Delay(UpdateStateInterval);
        }

        #endregion

        private void Delete(INotification notification)
        {
            SendNUI(new
            {
                eventName = "avg.internal",
                on = "notification.remove",
                plugin = "notification",
                id = notification.Id,
                duration = notification.Duration
            });

            _queue.Remove(notification);
        }

        public void Schedule(string title, string content, int duration)
        {
            _queue.Add(new SimpleNotification(title, content, duration));
        }

        #region Command

        [ClientCommand("notification.test", "owner", 4)]
        private void TestNotificationCommand(string title, string content, int duration)
        {
            Schedule(title, content, duration);
        }

        #endregion
    }
}
