using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static Average.Client.Framework.GameAPI;
using static Average.Client.Framework.Services.InputService;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class DoorService : IService
    {
        private readonly InputService _inputService;
        private readonly EventService _eventService;

        private List<DoorModel> _doors;

        private bool _isInit;

        public class DoorModel
        {
            public Vector3 Position { get; set; }
            public float Range { get; set; }
            public bool IsLocked { get; set; }

            [JsonIgnore]
            public Action<DoorModel> OpenAction { get; set; }
            [JsonIgnore]
            public Action<DoorModel, bool> NearAction { get; set; }

            public DoorModel(Vector3 position, float range, bool isLocked, Action<DoorModel, bool> nearAction, Action<DoorModel> openAction)
            {
                Position = position;
                Range = range;
                IsLocked = isLocked;
                NearAction = nearAction;
                OpenAction = openAction;
            }
        }

        public DoorService(InputService inputService, EventService eventService)
        {
            _inputService = inputService;
            _eventService = eventService;

            // Inputs
            _inputService.RegisterKey(new Input((Control)0x8CC9CD42,
            condition: () =>
            {
                if (!_isInit) return false;

                var ped = PlayerPedId();
                var coords = GetEntityCoords(ped, true, true);
                var exists = _doors.ToList().Exists(x => Vector3.Distance(coords, x.Position) < x.Range);
                return exists;
            },
            onStateChanged: (state) =>
            {
                Logger.Debug($"Client can {(state ? "open" : "not open")}/close the door");
            },
            onKeyReleased: () =>
            {
                if (!_isInit) return;

                var ped = PlayerPedId();
                var coords = GetEntityCoords(ped, true, true);
                var door = _doors.ToList().Find(x => Vector3.Distance(coords, x.Position) < x.Range);

                if (door == null) return;

                SetDoorState(ref door);

                Logger.Debug($"Client can open/close door: " + (door != null));
            }));
        }

        internal void OnInit(List<DoorModel> doors)
        {
            _doors = doors;
            _isInit = true;
        }

        internal void SetDoorState(ref DoorModel door)
        {
            _eventService.EmitServer("door:set_state", door.Position);
        }

        internal void OnSetDoorState(uint hash, int isLocked)
        {
            Call(0xD99229FE93B46286, hash, 1, 0, 0, 0, 0, 0);
            Call(0x6BAB9442830C7F53, (dynamic)hash, isLocked);
        }
    }
}
