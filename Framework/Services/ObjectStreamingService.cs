using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Models;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class ObjectStreamingService : IService
    {
        private readonly UserService _userService;
        private readonly ThreadService _threadService;

        public bool enableDithering = true;
        public const float ditheringDistance = 20f;
        public const float lodMaxDistance = 40f + ditheringDistance;

        public List<ObjectModel> registeredProps = new();

        public ObjectStreamingService(UserService userService, ThreadService threadService)
        {
            _userService = userService;
            _threadService = threadService;

            _threadService.StartThread(DitheringUpdate);
        }

        [Thread]
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
                _threadService.StopThread(DitheringUpdate);
            }

            await BaseScript.Delay(100);
        }

        [Thread]
        private async Task Update()
        {
            for (int i = 0; i < registeredProps.Count; i++)
            {
                var prop = registeredProps[i];
                var pos = GetEntityCoords(PlayerPedId(), true, true);
                var distance = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, prop.Position.X, prop.Position.Y, prop.Position.Z, true);

                if (distance <= lodMaxDistance)
                {
                    // Créer
                    if (!ContainsRegisteredEntityByUniqueIndex(prop.UniqueIndex))
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
                    // Supprime
                    if (ContainsRegisteredEntityByUniqueIndex(prop.UniqueIndex))
                    {
                        var entity = prop.Handle;

                        DeleteEntity(ref entity);
                        DeleteObject(ref entity);

                        registeredProps[i].Handle = 0;
                        registeredProps[i].IsSpawned = false;
                    }
                }
            }

            await BaseScript.Delay(1000);
        }

        public ObjectModel GetRegisteredEntityByUniqueIndex(string uniqueIndex) => registeredProps.Find(x => x.UniqueIndex == uniqueIndex);
        public bool ContainsRegisteredEntityByUniqueIndex(string uniqueIndex) => registeredProps.Exists(x => x.UniqueIndex == uniqueIndex);

        public ObjectModel RegisteredEntityWithCustomUniqueIndex(string uniqueIndex, uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateEntity(model, position, rotation, placeOnGround);
            var obj = new ObjectModel(entity, model, position, rotation, placeOnGround)
            {
                UniqueIndex = uniqueIndex,
                IsSpawned = true
            };

            registeredProps.Add(obj);

            return obj;
        }

        public ObjectModel CreateRegisteredEntity(uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateEntity(model, position, rotation, placeOnGround);
            var obj = new ObjectModel(entity, model, position, rotation, placeOnGround)
            {
                UniqueIndex = RandomString(),
                IsSpawned = true
            };

            registeredProps.Add(obj);

            return obj;
        }

        private int CreateEntity(uint model, Vector3 position, Vector3 rotation, bool placeOnGround)
        {
            var entity = CreateObject(model, position.X, position.Y, position.Z, false, false, false, false, false);

            FreezeEntityPosition(entity, true);
            SetEntityRotation(entity, rotation.X, rotation.Y, rotation.Z, 1, true);

            if (enableDithering)
            {
                SetEntityAlpha(entity, 0, false);
            }

            if (placeOnGround)
            {
                PlaceObjectOnGroundProperly(entity, 1);
            }

            return entity;
        }

        public async Task DeleteRegisteredEntity(string uniqueIndex)
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

                    await BaseScript.Delay(0);
                }

                registeredProps.RemoveAt(index);
            }
        }
    }
}
