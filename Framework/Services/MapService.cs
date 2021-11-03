using Average.Client.Models;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class MapService
    {
        private readonly ThreadService _threadService;

        public float lodDistance = 600f;

        public List<Imap> Imaps { get; }
        public List<MyImap> MyImaps { get; }
        public List<Interior> Interiors { get; }
        public List<InteriorSet> InteriorsSet { get; }
        public List<MyInterior> MyInteriors { get; }

        public MapService(ThreadService threadService)
        {
            _threadService = threadService;

            MyImaps = Configuration.Parse<List<MyImap>>("configs/my_imaps");
            MyInteriors = Configuration.Parse<List<MyInterior>>("configs/my_interiors");
            Imaps = Configuration.Parse<List<Imap>>("utilities/imaps_infos");
            Interiors = Configuration.Parse<List<Interior>>("utilities/interiors_infos");
            InteriorsSet = Configuration.Parse<List<InteriorSet>>("utilities/interiors_set_infos");
        }

        public void StartLowSpecMode()
        {
            _threadService.StartThread(LowSpecModeUpdate);
        }

        public void StopLowSpecMode()
        {
            _threadService.StopThread(LowSpecModeUpdate);
        }

        private async Task LowSpecModeUpdate()
        {
            var ped = PlayerPedId();
            var pos = GetEntityCoords(ped, true, true);

            for (int i = 0; i < Imaps.Count; i++)
            {
                var imap = Imaps[i];
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, imap.X, imap.Y, imap.Z, false) <= lodDistance)
                {
                    if (!IsImapActive(hash))
                    {
                        RequestImap(hash);
                    }
                }
                else
                {
                    if (IsImapActive(hash))
                    {
                        RemoveImap(hash);
                    }
                }
            }

            await BaseScript.Delay(1000);
        }

        internal void Load()
        {
            foreach (var imap in MyImaps)
            {
                var hash = (uint)long.Parse(imap.Hash);

                if (imap.Enabled)
                {
                    if (!IsImapActive(hash))
                    {
                        RequestImap(hash);
                    }
                }
                else
                {
                    if (IsImapActive(hash))
                    {
                        RemoveImap(hash);
                    }
                }
            }

            foreach (var interior in MyInteriors)
            {
                if (interior.Enable)
                {
                    Load(interior.Id, interior.Set);
                }
                else
                {
                    Unload(interior.Id, interior.Set);
                }
            }
        }

        internal void UnloadAll()
        {
            foreach (var imap in MyImaps)
            {
                var hash = (uint)long.Parse(imap.Hash);
                if (IsImapActive(hash))
                {
                    RemoveImap(hash);
                }
            }
        }

        internal void Load(int interior, string entitySetName)
        {
            if (!IsInteriorEntitySetActive(interior, entitySetName))
            {
                Call(0x174D0AAB11CED739, interior, entitySetName);
            }
        }

        internal void Unload(int interior, string entitySetName)
        {
            if (IsInteriorEntitySetActive(interior, entitySetName))
            {
                Call(0x33B81A2C07A51FFF, interior, entitySetName, true);
            }
        }
    }
}
