using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Enums;
using System;

namespace Average.Client.Framework.Handlers
{
    internal class WorldHandler : IHandler
    {
        private readonly WorldService _worldService;

        public WorldHandler(WorldService worldService)
        {
            _worldService = worldService;
        }

        [ClientEvent("world:set_time")]
        private void OnTimeUpdated(int hours, int minutes, int seconds, int transitionTime)
        {
            _worldService.SetTime(new TimeSpan(hours, minutes, seconds), transitionTime);
        }

        [ClientEvent("world:set_weather")]
        private void OnSetWeather(uint weather, float transitionTime)
        {
            Logger.Error("Weather: " + weather + ", " + transitionTime);

            _worldService.SetWeather(weather, transitionTime);
        }
    }
}
