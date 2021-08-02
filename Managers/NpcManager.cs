﻿using CitizenFX.Core;
using SDK.Client.Interfaces;
using SDK.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Managers
{
    public class NpcManager : INpcManager
    {
        public List<int> Npcs { get; } = new List<int>();

        public NpcManager(EventHandlerDictionary eventHandlers)
        {
            eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        public async Task<int> Create(uint model, int variation, Vector3 position, float heading, bool isNetwork = false, bool netMissionEntity = false)
        {
            if (!HasModelLoaded(model)) await LoadModel(model);
            var handle = CreatePed(model, position.X, position.Y, position.Z, heading, isNetwork, netMissionEntity, false, false);

            SetEntityVisible(handle, true);
            SetPedOutfitPreset(handle, variation);

            Npcs.Add(handle);
            return handle;
        }

        public int Get(int handle) => Npcs.Find(x => x == handle);

        public bool Exist(int handle) => Npcs.Exists(x => x == handle);

        public void Delete(int handle)
        {
            if (Exist(handle))
            {
                if (DoesEntityExist(handle)) DeleteEntity(ref handle);
                Npcs.Remove(Get(handle));
            }
        }

        #region Events

        protected void OnResourceStop(string resource)
        {
            Debug.WriteLine("Test stop resource");

            if (resource == Constant.RESOURCE_NAME)
            {
                for (int i = 0; i < Npcs.Count; i++)
                {
                    Delete(i);
                }
            }
        }

        #endregion
    }
}
