using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using Average.Shared.Events;
using System;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Services
{
    internal class WorldService : IService
    {
        public TimeSpan Time { get; set; }
        public Weather Weather { get; set; }

        public event EventHandler<WorldTimeEventArgs> TimeChanged;
        public event EventHandler<WorldWeatherEventArgs> WeatherChanged;

        public WorldService()
        {

        }

        internal void SetTime(TimeSpan time, int transitionTime)
        {
            Call(0x669E223E64B1903C, time.Hours, time.Minutes, time.Seconds, transitionTime, true);

            Time = time;
            TimeChanged?.Invoke(this, new WorldTimeEventArgs(time, transitionTime));
        }

        internal void SetWeather(uint weather, float transitionTime)
        {
            Call(0xD74ACDF7DB8114AF, false);
            Call(0x59174F1AFE095B5A, weather, true, true, true, transitionTime, false);

            Weather = (Weather)weather;
            WeatherChanged?.Invoke(this, new WorldWeatherEventArgs((Weather)weather, transitionTime));
        }
    }
}
