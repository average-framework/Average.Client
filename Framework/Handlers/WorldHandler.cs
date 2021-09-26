using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.DataModels;
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

        [ClientEvent("world:set_world")]
        private void OnSetWorld(uint weather, int hours, int minutes, int seconds) => _worldService.OnSetWorld(new WorldData
        {
            Weather = (Weather)weather,
            Time = new TimeSpan(hours, minutes, seconds)
        });

        [ClientEvent("world:set_time")]
        private void OnSetTime(int hours, int minutes, int seconds, int transitionTime)
        {
            _worldService.OnSetTime(new TimeSpan(hours, minutes, seconds), transitionTime);
        }

        [ClientEvent("world:set_weather")]
        private void OnSetWeather(uint weather, float transitionTime)
        {
            Logger.Debug("Set next weather: " + weather + ", " + transitionTime);
            _worldService.OnSetWeather((Weather)weather, transitionTime);
        }
    }
}
