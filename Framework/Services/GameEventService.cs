using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Services
{
    internal class GameEventService : IService
    {
        private readonly ExportDictionary _exportDictionary;

        public GameEventService(ExportDictionary exportDictionary)
        {
            _exportDictionary = exportDictionary;
        }

        public class EventPedCreatedEventArgs : EventArgs
        {
            public int Handle { get; set; }

            public EventPedCreatedEventArgs(int handle)
            {
                Handle = handle;
            }
        }

        public class EventPedDestroyedEventArgs : EventArgs
        {
            public int Handle { get; set; }

            public EventPedDestroyedEventArgs(int handle)
            {
                Handle = handle;
            }
        }

        public class EventVehicleCreatedEventArgs : EventArgs
        {
            public int Handle { get; set; }

            public EventVehicleCreatedEventArgs(int handle)
            {
                Handle = handle;
            }
        }

        public class EventVehicleDestroyedEventArgs : EventArgs
        {
            public int Handle { get; set; }

            public EventVehicleDestroyedEventArgs(int handle)
            {
                Handle = handle;
            }
        }

        public class EventEntityDamagedEventArgs : EventArgs
        {
            public int DamagedEntityId { get; set; }
            public int EntityIdOwnerOfDamage { get; set; }
            public uint WeaponHash { get; set; }
            public uint AmmoHash { get; set; }
            public float DamageAmount { get; set; }
            public int Unk1 { get; set; }
            public Vector3 EntityCoords { get; set; }

            public EventEntityDamagedEventArgs(int damagedEntityId, int entityIdOwnerOfDamage, uint weaponHash, uint ammoHash, float damageAmout, int unk1, Vector3 entityCoords)
            {
                DamagedEntityId = damagedEntityId;
                EntityIdOwnerOfDamage = entityIdOwnerOfDamage;
                WeaponHash = weaponHash;
                AmmoHash = ammoHash;
                DamageAmount = damageAmout;
                Unk1 = unk1;
                EntityCoords = entityCoords;
            }
        }

        public event EventHandler<EventPedCreatedEventArgs> PedCreated;
        public event EventHandler<EventPedDestroyedEventArgs> PedDestroyed;
        public event EventHandler<EventVehicleCreatedEventArgs> VehicleCreated;
        public event EventHandler<EventVehicleDestroyedEventArgs> VehicleDestroyed;
        public event EventHandler<EventEntityDamagedEventArgs> EntityDamaged;

        [StructLayout(LayoutKind.Explicit, Size = 8 * 1)]
        [SecurityCritical]
        internal struct EventPedCreated
        {
            [FieldOffset(0)] internal int pedId;

            internal EventPedCreated(int pedId)
            {
                this.pedId = pedId;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 8 * 1)]
        [SecurityCritical]
        internal struct EventPedDestroyed
        {
            [FieldOffset(0)] internal int pedId;

            internal EventPedDestroyed(int pedId)
            {
                this.pedId = pedId;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 8 * 1)]
        [SecurityCritical]
        internal struct EventVehicleCreated
        {
            [FieldOffset(0)] internal int vehicleId;

            internal EventVehicleCreated(int vehicleId)
            {
                this.vehicleId = vehicleId;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 8 * 1)]
        [SecurityCritical]
        internal struct EventVehicleDestroyed
        {
            [FieldOffset(0)] internal int vehicleId;

            internal EventVehicleDestroyed(int vehicleId)
            {
                this.vehicleId = vehicleId;
            }
        }

        [Thread]
        private async Task Update()
        {
            var size = GetNumberOfEvents(0);

            if (size > 0)
            {
                for (int i = 0; i < size; i++)
                {
                    var aiEventAtIndex = GetEventAtIndex(0, i);
                    var netEventAtIndex = GetEventAtIndex(1, i);

                    unsafe
                    {
                        if (aiEventAtIndex == GetHashKey("EVENT_PED_CREATED"))
                        {
                            //Logger.Debug("Event: EVENT_PED_CREATED");

                            var eventDataSize = 1;
                            var str = new EventPedCreated();
                            int result = ((IntPtr)(&str)).ToInt32();

                            GetEventData(0, i, ref result, eventDataSize);
                            PedCreated?.Invoke(this, new EventPedCreatedEventArgs(result));
                        }
                        else if (aiEventAtIndex == GetHashKey("EVENT_PED_DESTROYED"))
                        {
                            //Logger.Debug("Event: EVENT_PED_DESTROYED");

                            var eventDataSize = 1;
                            var str = new EventPedDestroyed();
                            int result = ((IntPtr)(&str)).ToInt32();

                            GetEventData(0, i, ref result, eventDataSize);
                            PedDestroyed?.Invoke(this, new EventPedDestroyedEventArgs(result));
                        }
                        else if (aiEventAtIndex == GetHashKey("EVENT_VEHICLE_CREATED"))
                        {
                            //Logger.Debug("Event: EVENT_VEHICLE_CREATED");

                            var eventDataSize = 1;
                            var str = new EventVehicleCreated();
                            int result = ((IntPtr)(&str)).ToInt32();
                            
                            GetEventData(0, i, ref result, eventDataSize);
                            VehicleCreated?.Invoke(this, new EventVehicleCreatedEventArgs(result));
                        }
                        else if (aiEventAtIndex == GetHashKey("EVENT_VEHICLE_DESTROYED"))
                        {
                            //Logger.Debug("Event: EVENT_VEHICLE_DESTROYED");

                            var eventDataSize = 1;
                            var str = new EventVehicleDestroyed();
                            int result = ((IntPtr)(&str)).ToInt32();

                            GetEventData(0, i, ref result, eventDataSize);
                            VehicleDestroyed?.Invoke(this, new EventVehicleDestroyedEventArgs(result));
                        }
                        else if (aiEventAtIndex == GetHashKey("EVENT_ENTITY_DAMAGED"))
                        {
                            //Logger.Debug("Event: EVENT_ENTITY_DAMAGED");

                            var eventDataSize = 9;

                            IDictionary<string, object> dict = (ExpandoObject)_exportDictionary[GetCurrentResourceName()].GetEventEntityDamaged(0, i, eventDataSize);

                            var isValidDamagedEntityId = int.TryParse(dict["damagedEntity"].ToString(), out int damagedEntityId);
                            var isValidEntityIdOwnerOfDamage = int.TryParse(dict["entityIdOwnerOfDamage"].ToString(), out int entityIdOwnerOfDamage);
                            var isValidWeaponHash = int.TryParse(dict["weaponHash"].ToString(), out int weaponHash);
                            var isValidAmmoHash = int.TryParse(dict["ammoHash"].ToString(), out int ammoHash);
                            var isValidDamageAmount = float.TryParse(dict["damageAmount"].ToString(), out float damageAmount);
                            var isValidUnk1 = int.TryParse(dict["unk1"].ToString(), out int unk1);
                            var isValidCoordX = float.TryParse(dict["coordX"].ToString(), out float coordX);
                            var isValidCoordY = float.TryParse(dict["coordY"].ToString(), out float coordY);
                            var isValidCoordZ = float.TryParse(dict["coordZ"].ToString(), out float coordZ);

                            EntityDamaged?.Invoke(this, new EventEntityDamagedEventArgs(damagedEntityId, entityIdOwnerOfDamage, (uint)weaponHash, (uint)ammoHash, damageAmount, unk1, new Vector3(coordX, coordY, coordZ)));
                        }

                        if (netEventAtIndex == GetHashKey("EVENT_NETWORK_DAMAGE_ENTITY"))
                        {

                        }
                    }
                }
            }
        }
    }
}
