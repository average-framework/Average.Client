using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using System.Collections.Generic;
using static Average.Client.Framework.Services.DoorService;

namespace Average.Client.Framework.Handlers
{
    internal class DoorHandler : IHandler
    {
        private readonly DoorService _doorService;

        public DoorHandler(DoorService doorService)
        {
            _doorService = doorService;
        }

        [ClientEvent("door:init")]
        private void OnInit(List<DoorModel> doors)
        {
            Logger.Error("On Init doors: " + doors.Count);

            _doorService.OnInit(doors);
        }

        [ClientEvent("door:set_state")]
        private void OnSetDoorState(uint hash, int isLocked)
        {
            _doorService.OnSetDoorState(hash, isLocked);
        }
    }
}
