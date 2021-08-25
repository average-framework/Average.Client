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
    public class MapManager : IMapManager
    {
        private float lodDistance = 600f;

        public List<ImapModel> ScannedImaps { get; } = new List<ImapModel>();
        public List<ImapModel> Imaps { get; }
        public List<InteriorModel> Interiors { get; }
        public List<CustomImapModel> CustomImaps { get; }
        public List<CustomInteriorModel> CustomInteriors { get; }
        public List<InteriorSetModel> InteriorsSet { get; }

        public MapManager()
        {
            Imaps = Configuration.Parse<List<ImapModel>>("utils/imaps.json");
            Interiors = Configuration.Parse<List<InteriorModel>>("utils/interiors.json");
            CustomImaps = Configuration.Parse<List<CustomImapModel>>("configs/custom_imaps.json");
            CustomInteriors = Configuration.Parse<List<CustomInteriorModel>>("configs/custom_interiors.json");
            InteriorsSet = Configuration.Parse<List<InteriorSetModel>>("utils/interiors_set.json");
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

        #region Command

        [Command("map.lowspecmode_disable")]
        private async void LowSpecModeDisableCommand()
        {
            if (await Main.permissionManager.HasPermission("player"))
            {
                Main.threadManager.StopThread(LowSpecModeUpdate);

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
        }

        [Command("map.lowspecmode_dist")]
        private async void LowSpecModeDistanceCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("player"))
            {
                if (args.Count == 1)
                {
                    lodDistance = float.Parse(args[0].ToString());
                }
            }
        }

        [Command("map.lowspecmode")]
        private async void LowSpecModeCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("player"))
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

                Main.threadManager.StartThread(LowSpecModeUpdate);
            }
        }

        [Command("map.reload_custom_imaps")]
        private async void ReloadCustomImapsCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                UnloadCustomImaps();
                await BaseScript.Delay(0);
                LoadCustomImaps();
            }
        }

        [Command("map.load_imaps")]
        private async void LoadImapsCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                foreach (var imap in Imaps)
                {
                    var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                    if (!IsImapActive(hash))
                        RequestImap(hash);
                }
            }
        }

        [ClientCommand("map.load_custom_imaps")]
        private async void LoadCustomImapsCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                foreach (var imap in Imaps)
                {
                    var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                    if (!IsImapActive(hash))
                        RequestImap(hash);
                }
            }
        }

        [ClientCommand("map.remove_imaps")]
        private async void RemoveImapsCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
                UnloadImaps();
        }

        [ClientCommand("map.remove_custom_imaps")]
        private async void RemoveCustomImapsCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
                UnloadCustomImaps();
        }

        [ClientCommand("map.scan_proximity")]
        private async void ScanProximityCommand(int source, List<object> args, string raw)
        {
            if (await Main.permissionManager.HasPermission("owner"))
            {
                var distance = float.Parse(args[0].ToString());
                var pos = GetEntityCoords(PlayerPedId(), true, true);

                ScannedImaps.Clear();

                foreach (var imap in Imaps)
                {
                    if (GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, imap.X, imap.Y, imap.Z, true) <= distance)
                    {
                        Log.Debug("[Map] Scanning: " + imap.Hash);
                        ScannedImaps.Add(imap);
                    }
                }

                Log.Debug("[Map] Scanned imap count: " + ScannedImaps.Count);
            }
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