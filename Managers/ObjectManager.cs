using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using SDK.Client.Interfaces;
using SDK.Client.Models;
using SDK.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class ObjectManager : IObjectManager
    {
        bool enableDithering;
        float ditheringDistance;
        float lodMaxDistance;
        int ditheringUpdateInterval;
        int renderUpdateInterval;

        public List<ObjectModel> registeredProps = new List<ObjectModel>();

        JObject baseConfig;

        public ObjectManager()
        {
            #region Configuration

            baseConfig = SDK.Client.Configuration.Parse("config.json");

            enableDithering = (bool)baseConfig["Streaming"]["EnableDithering"];
            ditheringDistance = (float)baseConfig["Streaming"]["DitheringDistance"];
            lodMaxDistance = (float)baseConfig["Streaming"]["LodMaxDistance"];
            ditheringUpdateInterval = (int)baseConfig["Streaming"]["DitheringUpdateInterval"];
            renderUpdateInterval = (int)baseConfig["Streaming"]["RenderUpdateInterval"];

            #endregion

            lodMaxDistance += ditheringDistance;

            #region Thread

            Main.threadManager.StartThread(DitheringUpdate);
            Main.threadManager.StartThread(Update);

            #endregion

            #region Event

            Main.eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            #endregion
        }

        #region Command

        [Command("object.create")]
        private async void CreateCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                var pos = GetEntityCoords(PlayerPedId(), true, true);
                CreateRegisteredEntity((uint)GetHashKey("p_waterpump01x"), pos, Vector3.Zero, true);
            }
        }

        [Command("object.mass_create")]
        private async void MassCreateCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                var pos = GetEntityCoords(PlayerPedId(), true, true);

                for (int i = 0; i < int.Parse(args[0].ToString()); i++)
                {
                    pos += new Vector3(2f, 0f, 0f);
                    CreateRegisteredEntity((uint)GetHashKey("p_waterpump01x"), pos, Vector3.Zero, true);
                    Main.logger.Warn("[Object] Create object: " + i);
                }
            }
        }

        #endregion

        private async Task DitheringUpdate()
        {
            if (enableDithering)
            {
                for (int i = 0; i < registeredProps.Count; i++)
                {
                    var prop = registeredProps.ElementAt(i);
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
                Main.threadManager.StopThread(DitheringUpdate);
            }

            await BaseScript.Delay(ditheringUpdateInterval);
        }

        protected async Task Update()
        {
            for (int i = 0; i < registeredProps.Count; i++)
            {
                var prop = registeredProps[i];
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
                        if (!registeredProps[i].IsSpawned)
                        {
                            registeredProps[i].Handle = CreateEntity(prop.Model, prop.Position, prop.Rotation, prop.IsPlacedOnGround);
                            registeredProps[i].IsSpawned = true;
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

                        registeredProps[i].Handle = 0;
                        registeredProps[i].IsSpawned = false;
                    }
                }
            }

            await BaseScript.Delay(renderUpdateInterval);
        }

        public ObjectModel GetRegisteredEntity(string uniqueIndex) => registeredProps.Find(x => x.UniqueIndex == uniqueIndex);

        public bool RegisteredEntityExist(string uniqueIndex) => registeredProps.Exists(x => x.UniqueIndex == uniqueIndex);

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

            registeredProps.Add(obj);

            return obj;
        }

        public ObjectModel CreateRegisteredEntity(uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateEntity(model, position, rotation, placeOnGround);
            var obj = new ObjectModel(entity, model, position, rotation, placeOnGround);
            obj.UniqueIndex = RandomString();
            obj.IsSpawned = true;

            registeredProps.Add(obj);

            return obj;
        }

        public void DeleteRegisteredEntity(string uniqueIndex)
        {
            var index = registeredProps.FindIndex(x => x.UniqueIndex == uniqueIndex);

            if (registeredProps.Count > 0 && index >= 0)
            {
                var prop = registeredProps[index];
                var entity = prop.Handle;

                while (DoesEntityExist(entity))
                {
                    DeleteEntity(ref entity);
                    DeleteObject(ref entity);
                }

                registeredProps.RemoveAt(index);
            }
        }

        #region Event

        protected void OnResourceStop(string resourceName)
        {
            if (resourceName == Constant.RESOURCE_NAME)
            {
                for (int i = 0; i < registeredProps.Count; i++)
                {
                    var entity = registeredProps[i].Handle;

                    DeleteEntity(ref entity);
                    DeleteObject(ref entity);
                }

                registeredProps.Clear();
            }
        }

        #endregion
    }
}
