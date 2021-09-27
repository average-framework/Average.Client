//using Average.Client.Framework.Diagnostics;
//using Average.Client.Framework.Interfaces;
//using Average.Shared.DataModels;
//using Average.Shared.Enums;
//using System;
//using static Average.Client.Framework.GameAPI;

//namespace Average.Client.Framework.Services
//{
//    internal class WorldService : IService
//    {
//        private bool CanChangeWorld { get; set; } = true;

//        public WorldService()
//        {

//        }

//        //[Thread]
//        //private async Task Update()
//        //{
//        //    if (_canChangeWorld)
//        //    {
//        //        var transitionTime = 10000 / 60;

//        //        if (_hours == 0)
//        //            transitionTime = 0;

//        //        NetworkClockTimeOverride(_hours, _minutes, _seconds, transitionTime, true);
//        //    }
//        //}

//        internal void OnSetWorld(WorldData worldData)
//        {
//            if (CanChangeWorld)
//            {
//                Call(0x59174F1AFE095B5A, (int)worldData.Weather, true, true, true, 0f, false);
//            }
//        }

//        internal void OnSetTime(TimeSpan time, int transitionTime = 10000 / 60)
//        {
//            if (CanChangeWorld)
//            {
//                if (time.Hours == 0)
//                {
//                    transitionTime = 0;
//                }

//                Logger.Debug("Transition time: " + transitionTime);

//                Call(0x669E223E64B1903C, time.Hours, time.Minutes, time.Seconds, transitionTime, true);
//                //NetworkClockTimeOverride(time.Hours, time.Minutes, time.Seconds, transitionTime, true);
//            }
//        }

//        internal void OnSetWeather(Weather weather, float transitionTime)
//        {
//            if (CanChangeWorld)
//            {
//                Call(0xD74ACDF7DB8114AF, false);
//                Call(0x59174F1AFE095B5A, (uint)weather, true, true, true, transitionTime, false);
//            }
//        }
//    }
//}
