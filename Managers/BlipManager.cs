using CitizenFX.Core;
using SDK.Client.Interfaces;
using SDK.Shared;
using System;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class BlipManager : IBlipManager
    {
        private List<int> _blips = new List<int>();

        public BlipManager()
        {
            #region Event

            Main.eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            #endregion
        }

        public int Create(int sprite, string text, float scale, Vector3 position)
        {
            var handle = CreateBlip(sprite, text, scale, position);
            _blips.Add(handle);
            return handle;
        }

        public void Delete(int handle)
        {
            if (!Exist(handle)) return;
            
            if (DoesBlipExist(handle))
                RemoveBlip(ref handle);

            if (_blips.Exists(x => x == handle))
                _blips.Remove(handle);
        }

        public int Get(int handle) => Exist(handle) ? _blips.Find(x => x == handle) : -1;

        public bool Exist(int handle) => _blips.Exists(x => x == handle);

        #region Event

        private void OnResourceStop(string resource)
        {
            if (resource == Constant.RESOURCE_NAME)
            {
                for (int i = 0; i < _blips.Count; i++)
                    Delete(_blips[i]);

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
