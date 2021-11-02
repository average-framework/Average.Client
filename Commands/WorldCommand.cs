using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Enums;
using System;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Commands
{
    internal class WorldCommand : ICommand
    {
        private readonly WorldService _worldService;
        private readonly EventService _eventService;

        public WorldCommand(WorldService worldService, EventService eventService)
        {
            _worldService = worldService;
            _eventService = eventService;
        }

        [ClientCommand("world:set_time")]
        private void OnSetTime(int hours, int minutes, int seconds, int transitionTime)
        {
            _eventService.EmitServer("world:set_time", hours, minutes, seconds, transitionTime);
        }

        [ClientCommand("world:set_weather")]
        private void OnSetWeather(string weatherName, float transitionTime)
        {
            if (Enum.TryParse(weatherName, true, out Weather weather))
            {
                _eventService.EmitServer("world:set_weather", (uint)weather, transitionTime);
            }
        }

        [ClientCommand("world:set_next_weather")]
        private void OnSetNextWeather(float transitionTime)
        {
            _eventService.EmitServer("world:set_next_weather", transitionTime);
        }

        [ClientCommand("world.enable_snow")]
        private void OnEnableSnow()
        {
            _worldService.SetWeather((uint)Weather.Snowlight, 0);

            Call(0xF02A9C330BBFC5C7, 3);
            Call(0xF6BEE7E80EC5CA40, 4000f);
            Call((uint)GetHashKey("FORCE_SNOW_PASS"), true);
        }

        [ClientCommand("world.disable_snow")]
        private void OnDisableSnow()
        {
            _worldService.SetWeather((uint)Weather.Sunny, 0);

            Call(0xF02A9C330BBFC5C7, 0);
            Call(0xF6BEE7E80EC5CA40, 0f);
            Call((uint)GetHashKey("FORCE_SNOW_PASS"), false);
        }
    }
}
