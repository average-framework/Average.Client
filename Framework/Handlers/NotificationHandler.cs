using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Attributes;
using CitizenFX.Core;
using System.Collections.Generic;

namespace Average.Client.Framework.Handlers
{
    internal class NotificationHandler : IHandler
    {
        private readonly NotificationService _notificationService;

        public NotificationHandler(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> args, CallbackDelegate cb)
        {
            _notificationService.OnClientWindowInitialized();

            return cb;
        }
    }
}
