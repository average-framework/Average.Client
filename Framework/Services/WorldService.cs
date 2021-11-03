using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using Average.Shared.Events;
using System;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Services
{
    internal class WorldService : IService
    {
        private readonly EventService _eventService;

        public TimeSpan Time { get; set; }
        public Weather Weather { get; set; }

        public event EventHandler<WorldTimeEventArgs> TimeChanged;
        public event EventHandler<WorldWeatherEventArgs> WeatherChanged;

        public WorldService(EventService eventService)
        {
            _eventService = eventService;
        }

        internal void SetNetworkedTime(TimeSpan time, int transitionTime)
        {
            _eventService.EmitServer("world:set_time", time.Hours, time.Minutes, time.Seconds, transitionTime);
        }

        internal void SetNetworkedWeather(string weatherName, float transitionTime)
        {
            if (Enum.TryParse(weatherName, true, out Weather weather))
            {
                _eventService.EmitServer("world:set_weather", (uint)weather, transitionTime);
            }
        }

        internal void SetNetworkedNextWeather(float transitionTime)
        {
            _eventService.EmitServer("world:set_next_weather", transitionTime);
        }

        internal void SetTime(TimeSpan time, int transitionTime)
        {
            Call(0x669E223E64B1903C, time.Hours, time.Minutes, time.Seconds, transitionTime, true);

            TimeChanged?.Invoke(this, new WorldTimeEventArgs(time, transitionTime));
        }

        internal void SetWeather(uint weather, float transitionTime)
        {
            Call(0xD74ACDF7DB8114AF, false);
            Call(0x59174F1AFE095B5A, weather, true, true, true, transitionTime, false);

            WeatherChanged?.Invoke(this, new WorldWeatherEventArgs((Weather)weather, transitionTime));
        }
    }
}
