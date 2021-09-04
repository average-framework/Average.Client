using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Models;
using SDK.Client.Prompts;
using SDK.Client.Utils;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class DoorManager : InternalPlugin, IDoorManager
    {
        private class DefaultDoorOverride : IDoorOverride
        {
            private readonly IPlugin _plugin;

            private uint unlockKey = (uint)Control.LootAmmo;
            private uint breakKey = (uint)Control.Loot2;
            
            private StandardPrompt openDoorPrompt;
            private StandardPrompt breakDoorPrompt;

            private InternalPlugin _script;
            
            public DefaultDoorOverride(IPlugin plugin)
            {
                _plugin = plugin;
                _script = (InternalPlugin) _plugin;
            }
            
            public async Task OnNearOfDoor(Door door)
            {
                openDoorPrompt = new StandardPrompt(0, (door.IsLocked ? "Déverrouiller" : "Vérrouiller") + " la porte",
                    (uint) Control.Sprint, 3f, 2f, () =>
                        _script.Door.IsAuthorizedToSetDoorState(_script.Character.Current.Job.Name, door).GetAwaiter()
                            .GetResult(), () =>
                    {
                        var pos = GetEntityCoords(PlayerPedId(), true, true);
                        var distance = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, door.Position.X, door.Position.Y,
                            door.Position.Z, true);
                        return distance <= 3f;
                    },
                    () =>
                    {
                        var raycast = GetTarget(PlayerPedId(), openDoorPrompt.VisibilityRange + 1f);

                        if (raycast.Hit)
                        {
                            var entityPos = GetEntityCoords(raycast.EntityHit, true, true);
                            var doorInfo = _script.Door.GetDoorModel(entityPos);

                            if (doorInfo != null)
                                return _script.Door.IsAuthorizedToSetDoorState(_script.Character.Current.Job.Name, door)
                                    .GetAwaiter().GetResult();

                            return false;
                        }

                        return false;
                    });
                openDoorPrompt.OnStandardModeCompletedHandler += async prompt =>
                {
                    await PlayClipset2("mech_doors@locked@door_knob@generic@handle_r@hand_r@try_door", "lockpick", 7,
                        4000, 1f, 1f);
                    await BaseScript.Delay(2000);

                    door.IsLocked = !door.IsLocked;
                    _script.Door.NetworkSetDoorState(door.Position);

                    await BaseScript.Delay(1000);

                    prompt.Text = (door.IsLocked ? "Déverrouiller" : "Vérrouiller") + " la porte";

                    breakDoorPrompt.IsVisible = door.IsLocked;
                    breakDoorPrompt.IsEnabled = door.IsLocked;

                    prompt.IsRunningCompleted = false;
                };
                _script.Prompt.Create(openDoorPrompt);

                breakDoorPrompt = new StandardPrompt(0, "Enfoncer la porte", (uint) Control.Revive, 3f, 2f, () => true,
                    () =>
                    {
                        var pos = GetEntityCoords(PlayerPedId(), true, true);
                        var distance = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, door.Position.X, door.Position.Y,
                            door.Position.Z, true);
                        return distance <= 3f;
                    },
                    () =>
                    {
                        var raycast = GetTarget(PlayerPedId(), breakDoorPrompt.VisibilityRange + 1f);

                        if (raycast.Hit)
                        {
                            var entityPos = GetEntityCoords(raycast.EntityHit, true, true);
                            var doorInfo = _script.Door.GetDoorModel(entityPos);

                            if (doorInfo != null)
                                return _script.Door.IsAuthorizedToSetDoorState(_script.Character.Current.Job.Name, door)
                                    .GetAwaiter().GetResult();

                            return false;
                        }

                        return false;
                    });
                breakDoorPrompt.OnStandardModeCompletedHandler += async prompt =>
                {
                    // break the door
                    await PlayClipset2("mech_doors@locked@door_knob@generic@handle_r@hand_r@try_door", "kick_success",
                        7, 2000, 1f, 1f);
                    await BaseScript.Delay(1000);

                    var probability = GetRandomIntInRange(0, 100);

                    if (probability <= 10)
                    {
                        // open the door
                        door.IsLocked = !door.IsLocked;
                        openDoorPrompt.Text = door.IsLocked ? "Déverrouiller" : "Vérrouiller";
                        prompt.IsVisible = false;
                        prompt.IsEnabled = false;
                        _script.Notification.Schedule("PORTE", "La porte à cèder", 5000);
                        _script.Door.NetworkSetDoorState(door.Position);

                        await BaseScript.Delay(1000);
                    }

                    prompt.IsRunningCompleted = false;
                };
                _script.Prompt.Create(breakDoorPrompt);

                breakDoorPrompt.IsVisible = door.IsLocked && door.CanForce;
                breakDoorPrompt.IsEnabled = door.IsLocked && door.CanForce;
            }

            public async Task OnFarOfDoor()
            {
                _script.Prompt.Delete(openDoorPrompt);
                _script.Prompt.Delete(breakDoorPrompt);
            }
        }

        private List<Door> _doors;
        private List<DoorInfo> _doorsInfo;

        private bool _lastIsNear;
        private bool _isNear;

        public override void OnInitialized()
        {
            _doorsInfo = Configuration.Parse<List<DoorInfo>>("configs/doors_info.json");

            OverrideDoorSystem(new DefaultDoorOverride(this));
            
            Task.Factory.StartNew(async () =>
            {
                #region Event

                Rpc.Event("Door.GetAll").On<List<Door>>(doors =>
                {
                    _doors = doors;
                    _doors.ForEach(x => SetDoorStateLocally(x.Position, x.IsLocked));
                }).Emit();

                #endregion

                await IsReady();
                await Character.IsReady();
                await Storage.IsReady();
                await Storage.IsItemsInfoReady();

                Thread.StartThread(Update);
            });
        }

        public async Task IsReady()
        {
            while (_doors == null) await BaseScript.Delay(0);
        }

        public void NetworkSetDoorState(Vector3 position) => Event.EmitServer("Door.SetDoorState", position);
        
        public async Task<bool> IsAuthorizedToSetDoorState(string jobName, Door door)
        {
            if (await Permission.HasPermission("admin")) return true;
            return jobName == door.JobName;
        }

        public DoorInfo GetDoorModel(Vector3 position)
        {
            return _doorsInfo.Find(x =>
              Math.Round(x.X) == Math.Round(position.X) &&
              Math.Round(x.Y) == Math.Round(position.Y) &&
              Math.Round(x.Z) == Math.Round(position.Z));
        }

        private IDoorOverride _doorSystem;
        
        public void OverrideDoorSystem(IDoorOverride door)
        {
            _doorSystem = door;
        }
        
        public async Task PlayerOpeningDoorAnimation()
        {
            await PlayClipset2("mech_doors@locked@door_knob@generic@handle_r@hand_r@try_door", "lockpick", 7, 4000, 1f, 1f);
            await BaseScript.Delay(1000);
        }

        private async Task Update()
        {
            var pos = GetEntityCoords(PlayerPedId(), true, true);
            var nearestDoor = _doors.Find(x =>
                GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, x.Position.X, x.Position.Y, x.Position.Z, true) <= x.Range);

            if (nearestDoor == null)
            {
                if (_isNear)
                {
                    _isNear = false;
                    await _doorSystem.OnFarOfDoor();
                }
            
                await BaseScript.Delay(250);
            }
            else
            {
                if (!_isNear)
                {
                    _isNear = true;
                    await _doorSystem.OnNearOfDoor(nearestDoor);
                }
            }
        }

        public void SetDoorStateLocally(Vector3 position, bool isLocked)
        {
            var door = _doors.Find(x =>
                Math.Round(x.Position.X) == Math.Round(position.X) &&
                Math.Round(x.Position.Y) == Math.Round(position.Y) &&
                Math.Round(x.Position.Z) == Math.Round(position.Z));

            if (door == null) return;
            
            door.IsLocked = isLocked;
            // Hud.SetHelpText(Lang.Current["Client.DoorManager.IsAuthorizedLeft"], Lang.Current["Client.DoorManager.IsAuthorizedKey"], Lang.Current["Client.DoorManager.IsAuthorizedRight"].ToString().Replace("{0}", isLocked ? Lang.Current["Client.DoorManager.IsNotLocked"] : Lang.Current["Client.DoorManager.IsLocked"]));
            
            Function.Call((Hash)0xD99229FE93B46286, uint.Parse(GetDoorModel(position).Hash), 1, 0, 0, 0, 0, 0);
            DoorSystemSetDoorState(uint.Parse(GetDoorModel(position).Hash), isLocked);   
        }

        #region Events

        [ClientEvent("Door.SetDoorState")]
        private void SetDoorStateEvent(Vector3 position, bool isLocked)
        {
            SetDoorStateLocally(position, isLocked);
        }

        #endregion
    }
}