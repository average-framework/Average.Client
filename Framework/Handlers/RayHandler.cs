using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Attributes;
using CitizenFX.Core;
using System.Collections.Generic;

namespace Average.Client.Framework.Handlers
{
    internal class RayHandler : IHandler
    {
        private readonly RayService _rayService;
        private readonly UIService _uiService;

        public RayHandler(RayService rayService, UIService uiService)
        {
            _rayService = rayService;
            _uiService = uiService;
        }

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> args, CallbackDelegate cb)
        {
            _rayService.OnClientWindowInitialized();

            return cb;
        }

        [UICallback("ray/on_click")]
        private CallbackDelegate OnClick(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var id = (string)args["id"];

            _rayService.OnClick(id);

            return cb;
        }

        [UICallback("ray/keydown")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var key = int.Parse(args["key"].ToString());

            // Touche Echap
            if (key == 27)
            {
                if (_rayService.IsOpen)
                {
                    if (_rayService.histories.Count > 0)
                    {
                        var currentGroupIndex = _rayService.histories.FindIndex(x => x.Name == _rayService.currentGroup.Name);

                        if (currentGroupIndex > 0)
                        {
                            var parent = _rayService.histories[currentGroupIndex - 1];

                            _rayService.Open(parent);
                            _rayService.histories.RemoveAt(currentGroupIndex);
                        }
                        else
                        {
                            _rayService.OnPrevious();
                            _uiService.Unfocus();
                        }
                    }
                    else
                    {
                        _rayService.OnPrevious();
                        _uiService.Unfocus();
                    }
                }
            }

            return cb;
        }
    }
}
