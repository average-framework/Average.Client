using CitizenFX.Core;
using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SDK.Client.Models;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Managers
{
    public class MapManager : InternalPlugin, IMapManager
    {
        private float lodDistance = 600f;

        public List<ImapModel> ScannedImaps { get; } = new List<ImapModel>();
        public List<ImapModel> Imaps { get; private set; }
        public List<InteriorModel> Interiors { get; private set; }
        public List<CustomImapModel> CustomImaps { get; private set; }
        public List<CustomInteriorModel> CustomInteriors { get; private set; }
        public List<InteriorSetModel> InteriorsSet { get; private set; }

        public override void OnInitialized()
        {
            Imaps = Configuration.Parse<List<ImapModel>>("utils/imaps.json");
            Interiors = Configuration.Parse<List<InteriorModel>>("utils/interiors.json");
            CustomImaps = Configuration.Parse<List<CustomImapModel>>("configs/custom_imaps.json");
            CustomInteriors = Configuration.Parse<List<CustomInteriorModel>>("configs/custom_interiors.json");
            InteriorsSet = Configuration.Parse<List<InteriorSetModel>>("utils/interiors_set.json");
        }

        #region Thread

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
                        RequestImap(hash);
                }
                else
                {
                    if (IsImapActive(hash))
                        RemoveImap(hash);
                }
            }

            await BaseScript.Delay(1000);
        }

        #endregion

        #region Command

        [ClientCommand("map.lowspecmode_disable", "player", 0)]
        private async void LowSpecModeDisableCommand()
        {
            Thread.StopThread(LowSpecModeUpdate);

            await BaseScript.Delay(1000);

            for (int i = 0; i < Imaps.Count; i++)
            {
                var imap = Imaps[i];
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (!IsImapActive(hash))
                    RequestImap(hash);
            }

            for (int i = 0; i < CustomImaps.Count; i++)
            {
                var imap = CustomImaps[i];
                var hash = (uint) long.Parse(imap.Hash);

                if (imap.Enabled)
                {
                    if (!IsImapActive(hash))
                        RequestImap(hash);
                }
                else
                {
                    if (IsImapActive(hash))
                        RemoveImap(hash);
                }
            }
        }

        [ClientCommand("map.lowspecmode_dist", "player", 0)]
        private void LowSpecModeDistanceCommand(float range)
        {
            lodDistance = range;
        }

        [ClientCommand("map.lowspecmode", "player", 0)]
        private async void LowSpecModeCommand()
        {
            // Décharge tout les imaps par défaut
            for (int i = 0; i < Imaps.Count; i++)
            {
                var imap = Imaps[i];
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (IsImapActive(hash))
                {
                    RemoveImap(hash);
                    Log.Debug("[Map] Unloading imap: " + imap.Hash);
                }
            }

            Log.Debug("[Map] All imaps was removed.");
            Log.Debug("[Map] Starting deferring imap loading.");

            Thread.StartThread(LowSpecModeUpdate);
        }

        [ClientCommand("map.reload_custom_imaps", "owner", 4)]
        private async void ReloadCustomImapsCommand()
        {
            UnloadCustomImaps();
            await BaseScript.Delay(0);
            LoadCustomImaps();
        }

        [ClientCommand("map.load_imaps", "owner", 4)]
        private void LoadImapsCommand()
        {
            foreach (var imap in Imaps)
            {
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (!IsImapActive(hash))
                    RequestImap(hash);
            }
        }

        [ClientCommand("map.load_custom_imaps", "owner", 4)]
        private async void LoadCustomImapsCommand()
        {
            foreach (var imap in Imaps)
            {
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (!IsImapActive(hash))
                    RequestImap(hash);
            }
        }

        [ClientCommand("map.remove_imaps", "owner", 4)]
        private void RemoveImapsCommand()
        {
            UnloadImaps();
        }

        [ClientCommand("map.remove_custom_imaps", "owner", 4)]
        private async void RemoveCustomImapsCommand()
        {
            UnloadCustomImaps();
        }

        [ClientCommand("map.scan_proximity", "owner", 4)]
        private void ScanProximityCommand(float range)
        {
            var pos = GetEntityCoords(PlayerPedId(), true, true);

            ScannedImaps.Clear();

            foreach (var imap in Imaps)
            {
                if (GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, imap.X, imap.Y, imap.Z, true) <= range)
                {
                    Log.Debug("[Map] Scanning: " + imap.Hash);
                    ScannedImaps.Add(imap);
                }
            }

            Log.Debug("[Map] Scanned imap count: " + ScannedImaps.Count);
        }

        #endregion

        public void LoadCustomImaps()
        {
            foreach (var imap in CustomImaps)
            {
                var hash = (uint) long.Parse(imap.Hash);

                if (imap.Enabled)
                {
                    if (!IsImapActive(hash))
                        RequestImap(hash);
                }
                else
                {
                    if (IsImapActive(hash))
                        RemoveImap(hash);
                }
            }
        }

        public void LoadCustomInteriors()
        {
            foreach (var interior in CustomInteriors)
            {
                if (interior.Enable)
                    LoadInterior(interior.Id, interior.Set);
                else
                    UnloadInterior(interior.Id, interior.Set);
            }
        }

        public void UnloadImaps()
        {
            foreach (var imap in Imaps)
            {
                var hash = (uint) long.Parse(imap.Hash);

                if (IsImapActive(hash))
                    RemoveImap(hash);
            }
        }

        public void UnloadCustomImaps()
        {
            foreach (var imap in CustomImaps)
            {
                var hash = (uint) long.Parse(imap.Hash);

                if (IsImapActive(hash))
                    RemoveImap(hash);
            }
        }

        public void UnloadCustomInteriors()
        {
            foreach (var interior in CustomInteriors)
            {
                if (interior.Enable)
                    UnloadInterior(interior.Id, interior.Set);
            }
        }

        public void UnloadInteriors()
        {
            foreach (var interior in Interiors)
            {
                var set = InteriorsSet.Find(x => x.Id == interior.Id);
                if (set == null) continue;
                UnloadInterior(interior.Id, set.Set);
            }
        }

        public void LoadInterior(int interior, string entitySetName)
        {
            if (!IsInteriorEntitySetActive(interior, entitySetName))
                Function.Call((Hash) 0x174D0AAB11CED739, interior, entitySetName);
        }

        public void UnloadInterior(int interior, string entitySetName)
        {
            if (IsInteriorEntitySetActive(interior, entitySetName))
                Function.Call((Hash) 0x33B81A2C07A51FFF, interior, entitySetName, true);
        }
    }
}