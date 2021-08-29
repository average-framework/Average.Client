using CitizenFX.Core;
using SDK.Client.Interfaces;
using SDK.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDK.Client;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class NpcManager : InternalPlugin, INpcManager
    {
        private readonly List<int> _npcs = new List<int>();

        public async Task<int> Create(uint model, int variation, Vector3 position, float heading, bool isNetwork = false, bool netMissionEntity = false)
        {
            if (!HasModelLoaded(model))
                await LoadModel(model);

            var handle = CreatePed(model, position.X, position.Y, position.Z, heading, isNetwork, netMissionEntity, false, false);

            SetEntityVisible(handle, true);
            SetPedOutfitPreset(handle, variation);

            _npcs.Add(handle);
            return handle;
        }

        public int Get(int handle) => Exist(handle) ? _npcs.Find(x => x == handle) : -1;

        public bool Exist(int handle) => _npcs.Exists(x => x == handle);

        public void Delete(int handle)
        {
            if (Exist(handle))
            {
                if (DoesEntityExist(handle))
                    DeleteEntity(ref handle);

                if (_npcs.Exists(x => x == handle))
                    _npcs.Remove(Get(handle));
            }
        }

        #region Event

        [ClientEvent("ResourceStop")]
        private void OnResourceStop(string resource)
        {
            if (resource == Constant.RESOURCE_NAME)
            {
                for (int i = 0; i < _npcs.Count; i++)
                    Delete(_npcs[i]);

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
