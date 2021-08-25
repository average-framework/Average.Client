using Average.Client.Notifications;
using CitizenFX.Core;
using SDK.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class NotificationManager : INotificationManager
    {
        private List<INotification> _queue = new List<INotification>();

        private const int UpdateStateInterval = 100;

        public NotificationManager()
        {
            #region Command

            RegisterCommand("notification.test", new Action<int, List<object>, string>(TestNotificationCommand), false);

            #endregion

            #region Event

            Main.eventManager.RegisterInternalNUICallbackEvent("window_ready", WindowReady);
            Main.eventManager.RegisterInternalNUICallbackEvent("notification/avg.ready", Ready);

            #endregion

            #region Thread

            Main.threadManager.StartThread(Update);
            Main.threadManager.StartThread(UpdateState);

            #endregion
        }

        #region NUI

        CallbackDelegate WindowReady(IDictionary<string, object> data, CallbackDelegate result)
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

        CallbackDelegate Ready(IDictionary<string, object> data, CallbackDelegate result)
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

        protected async Task UpdateState()
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

        private async void TestNotificationCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                var title = args[0].ToString();
                var content = args[1].ToString();
                var duration = int.Parse(args[2].ToString());

                Schedule(title, content, duration);
            }
        }

        #endregion
    }
}
