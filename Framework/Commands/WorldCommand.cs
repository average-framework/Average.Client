﻿using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Enums;
using System;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Commands
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
            _worldService.SetNetworkedTime(new TimeSpan(hours, minutes, seconds), transitionTime);
        }

        [ClientCommand("world:set_weather")]
        private void OnSetWeather(string weatherName, float transitionTime)
        {
            _worldService.SetNetworkedWeather(weatherName, transitionTime);
        }

        [ClientCommand("world:set_next_weather")]
        private void OnSetNextWeather(float transitionTime)
        {
            _worldService.SetNetworkedNextWeather(transitionTime);
        }

        [ClientCommand("world.enable_snow")]
        private void OnEnableSnow()
        {
            _worldService.SetNetworkedWeather(Weather.Snowlight.ToString(), 0f);

            Call(0xF02A9C330BBFC5C7, 3);
            Call(0xF6BEE7E80EC5CA40, 4000f);
            Call((uint)GetHashKey("FORCE_SNOW_PASS"), true);
        }

        [ClientCommand("world.disable_snow")]
        private void OnDisableSnow()
        {
            _worldService.SetNetworkedWeather(Weather.Sunny.ToString(), 0f);

            Call(0xF02A9C330BBFC5C7, 0);
            Call(0xF6BEE7E80EC5CA40, 0f);
            Call((uint)GetHashKey("FORCE_SNOW_PASS"), false);
        }
    }
}