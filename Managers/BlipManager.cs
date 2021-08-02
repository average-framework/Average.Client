using CitizenFX.Core;
using SDK.Client.Interfaces;
using SDK.Shared;
using System;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Managers
{
    public class BlipManager : IBlipManager
    {
        public List<int> Blips { get; } = new List<int>();

        public BlipManager(EventHandlerDictionary eventHandlers)
        {
            eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        public int Create(int sprite, string text, float scale, Vector3 position)
        {
            var handle = CreateBlip(sprite, text, scale, position);

            Blips.Add(handle);
            return handle;
        }

        public void Delete(int handle)
        {
            if (Exist(handle))
            {
                if (DoesBlipExist(handle)) RemoveBlip(ref handle);
                if (Blips.Exists(x => x == handle)) Blips.Remove(handle);
            }
        }

        public int Get(int handle) => Exist(handle) ? Blips.Find(x => x == handle) : -1;

        public bool Exist(int handle) => Blips.Exists(x => x == handle);

        #region Events

        protected void OnResourceStop(string resource)
        {
            if (resource == Constant.RESOURCE_NAME)
            {
                for (int i = 0; i < Blips.Count; i++)
                {
                    var handle = Blips[i];
                    Delete(handle);
                }

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
