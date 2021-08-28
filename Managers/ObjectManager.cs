using CitizenFX.Core;
using SDK.Client.Interfaces;
using SDK.Client.Models;
using SDK.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDK.Client.Diagnostics;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;
using Newtonsoft.Json.Linq;

namespace Average.Client.Managers
{
    public class ObjectManager : InternalPlugin, IObjectManager
    {
        private bool enableDithering;
        private float ditheringDistance;
        private float lodMaxDistance;
        private int ditheringUpdateInterval;
        private int renderUpdateInterval;

        private List<ObjectModel> _registeredProps = new List<ObjectModel>();

        public override void OnInitialized()
        {
            #region Configuration

            JObject baseConfig = SDK.Client.Configuration.ParseToObject("config.json");

            enableDithering = (bool)baseConfig["Streaming"]["EnableDithering"];
            ditheringDistance = (float)baseConfig["Streaming"]["DitheringDistance"];
            lodMaxDistance = (float)baseConfig["Streaming"]["LodMaxDistance"];
            ditheringUpdateInterval = (int)baseConfig["Streaming"]["DitheringUpdateInterval"];
            renderUpdateInterval = (int)baseConfig["Streaming"]["RenderUpdateInterval"];

            lodMaxDistance += ditheringDistance;

            #endregion

            #region Thread

            Thread.StartThread(DitheringUpdate);
            Thread.StartThread(Update);

            #endregion

            #region Event

            Main.eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            #endregion
        }

        #region Command

        [Command("object.create")]
        private async void CreateCommand(int source, List<object> args, string raw)
        {
            if (await Permission.HasPermission("owner"))
            {
                var pos = GetEntityCoords(PlayerPedId(), true, true);
                CreateRegisteredEntity((uint)GetHashKey("p_waterpump01x"), pos, Vector3.Zero, true);
            }
        }

        [Command("object.mass_create")]
        private async void MassCreateCommand(int source, List<object> args, string raw)
        {
            if (await Permission.HasPermission("owner"))
            {
                var pos = GetEntityCoords(PlayerPedId(), true, true);

                for (int i = 0; i < int.Parse(args[0].ToString()); i++)
                {
                    pos += new Vector3(2f, 0f, 0f);
                    CreateRegisteredEntity((uint)GetHashKey("p_waterpump01x"), pos, Vector3.Zero, true);
                    Log.Warn("[Object] Create object: " + i);
                }
            }
        }

        #endregion

        private async Task DitheringUpdate()
        {
            if (enableDithering)
            {
                for (int i = 0; i < _registeredProps.Count; i++)
                {
                    var prop = _registeredProps.ElementAt(i);
                    var pos = GetEntityCoords(PlayerPedId(), true, true);
                    var distance = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, prop.Position.X, prop.Position.Y, prop.Position.Z, true);

                    if (DoesEntityExist(prop.Handle))
                    {
                        if (distance <= lodMaxDistance)
                        {
                            var invertDitheringDistance = lodMaxDistance - ditheringDistance;

                            if (distance >= invertDitheringDistance && distance <= lodMaxDistance)
                            {
                                var newDistance = (distance - invertDitheringDistance);
                                var invertDistance = ditheringDistance - newDistance;
                                var percentage = (invertDistance / ditheringDistance) * 255f;

                                SetEntityAlpha(prop.Handle, (int)percentage, false);
                            }
                            else
                            {
                                SetEntityAlpha(prop.Handle, 255, false);
                            }
                        }
                    }
                }
            }
            else
            {
                Thread.StopThread(DitheringUpdate);
            }

            await BaseScript.Delay(ditheringUpdateInterval);
        }

        private async Task Update()
        {
            for (int i = 0; i < _registeredProps.Count; i++)
            {
                var prop = _registeredProps[i];
                var pos = GetEntityCoords(PlayerPedId(), true, true);
                var distance = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, prop.Position.X, prop.Position.Y, prop.Position.Z, true);

                if (distance <= lodMaxDistance)
                {
                    if (!RegisteredEntityExist(prop.UniqueIndex))
                    {
                        CreateRegisteredEntity(prop.Model, prop.Position, prop.Rotation, prop.IsPlacedOnGround);
                    }
                    else
                    {
                        if (!_registeredProps[i].IsSpawned)
                        {
                            _registeredProps[i].Handle = CreateEntity(prop.Model, prop.Position, prop.Rotation, prop.IsPlacedOnGround);
                            _registeredProps[i].IsSpawned = true;
                        }
                    }
                }
                else
                {
                    if (RegisteredEntityExist(prop.UniqueIndex))
                    {
                        var entity = prop.Handle;

                        DeleteEntity(ref entity);
                        DeleteObject(ref entity);

                        _registeredProps[i].Handle = 0;
                        _registeredProps[i].IsSpawned = false;
                    }
                }
            }

            await BaseScript.Delay(renderUpdateInterval);
        }

        public ObjectModel GetRegisteredEntity(string uniqueIndex) => _registeredProps.Find(x => x.UniqueIndex == uniqueIndex);

        public bool RegisteredEntityExist(string uniqueIndex) => _registeredProps.Exists(x => x.UniqueIndex == uniqueIndex);

        private int CreateEntity(uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateObject(model, position.X, position.Y, position.Z, false, false, false, false, false);
            FreezeEntityPosition(entity, true);
            SetEntityRotation(entity, rotation.X, rotation.Y, rotation.Z, 1, true);

            if (enableDithering)
                SetEntityAlpha(entity, 0, false);

            if (placeOnGround)
                PlaceObjectOnGroundProperly(entity, 1);

            return entity;
        }

        public ObjectModel CreateRegisteredEntityWithCustomUniqueIndex(string uniqueIndex, uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateEntity(model, position, rotation, placeOnGround);
            var obj = new ObjectModel(entity, model, position, rotation, placeOnGround);
            obj.UniqueIndex = uniqueIndex;
            obj.IsSpawned = true;

            _registeredProps.Add(obj);

            return obj;
        }

        public ObjectModel CreateRegisteredEntity(uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateEntity(model, position, rotation, placeOnGround);
            var obj = new ObjectModel(entity, model, position, rotation, placeOnGround);
            obj.UniqueIndex = RandomString();
            obj.IsSpawned = true;

            _registeredProps.Add(obj);

            return obj;
        }

        public void DeleteRegisteredEntity(string uniqueIndex)
        {
            var index = _registeredProps.FindIndex(x => x.UniqueIndex == uniqueIndex);

            if (_registeredProps.Count > 0 && index >= 0)
            {
                var prop = _registeredProps[index];
                var entity = prop.Handle;

                while (DoesEntityExist(entity))
                {
                    DeleteEntity(ref entity);
                    DeleteObject(ref entity);
                }

                _registeredProps.RemoveAt(index);
            }
        }

        #region Event

        private void OnResourceStop(string resourceName)
        {
            if (resourceName == Constant.RESOURCE_NAME)
            {
                for (int i = 0; i < _registeredProps.Count; i++)
                {
                    var entity = _registeredProps[i].Handle;

                    DeleteEntity(ref entity);
                    DeleteObject(ref entity);
                }

                _registeredProps.Clear();
            }
        }

        #endregion
    }
}
