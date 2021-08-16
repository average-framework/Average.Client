using CitizenFX.Core;
using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Managers
{
    public class MapManager : IMapManager
    {
        Logger logger;
        PermissionManager permission;
        ThreadManager thread;

        float lodDistance = 600f;

        public List<ImapModel> ScannedImaps { get; } = new List<ImapModel>();
        public List<ImapModel> Imaps { get; private set; }
        public List<InteriorModel> Interiors { get; private set; }
        public List<CustomImapModel> CustomImaps { get; private set; }
        public List<CustomInteriorModel> CustomInteriors { get; private set; }
        public List<InteriorSetModel> InteriorsSet { get; private set; }

        public MapManager(Logger logger, PermissionManager permission, ThreadManager thread)
        {
            this.logger = logger;
            this.permission = permission;
            this.thread = thread;

            Imaps = Configuration.Parse<List<ImapModel>>("utils/imaps.json");
            Interiors = Configuration.Parse<List<InteriorModel>>("utils/interiors.json");
            CustomImaps = Configuration.Parse<List<CustomImapModel>>("configs/custom_imaps.json");
            CustomInteriors = Configuration.Parse<List<CustomInteriorModel>>("configs/custom_interiors.json");
            InteriorsSet = Configuration.Parse<List<InteriorSetModel>>("utils/interiors_set.json");
        }

        protected async Task LowSpecModeUpdate()
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

        #region Commands

        [Command("map.lowspecmode_disable")]
        public async void LowSpecModeDisableCommand()
        {
            if (await permission.HasPermission("player"))
            {
                thread.StopThread(LowSpecModeUpdate);

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
                    var hash = (uint)long.Parse(imap.Hash);

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
        public async void LowSpecModeDistanceCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("player"))
            {
                if (args.Count == 1)
                {
                    lodDistance = float.Parse(args[0].ToString());
                }
            }
        }

        [Command("map.lowspecmode")]
        public async void LowSpecModeCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("player"))
            {
                // Décharge tout les imaps par défaut
                for (int i = 0; i < Imaps.Count; i++)
                {
                    var imap = Imaps[i];
                    var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                    if (IsImapActive(hash))
                    {
                        RemoveImap(hash);
                        logger.Debug("[Map] Unloading imap: " + imap.Hash);
                    }
                }

                logger.Debug("[Map] All imaps was removed.");
                logger.Debug("[Map] Starting deferring imap loading.");

                thread.StartThread(LowSpecModeUpdate);
            }
        }

        [Command("map.reload_custom_imaps")]
        public async void ReloadCustomImapsCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("owner"))
            {
                UnloadCustomImaps();
                await BaseScript.Delay(0);
                LoadCustomImaps();
            }
        }

        [Command("map.load_imaps")]
        public async void LoadImapsCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("owner"))
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
        public async void LoadCustomImapsCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("owner"))
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
        public async void RemoveImapsCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("owner"))
                UnloadImaps();
        }

        [ClientCommand("map.remove_custom_imaps")]
        public async void RemoveCustomImapsCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("owner"))
                UnloadCustomImaps();
        }

        [ClientCommand("map.scan_proximity")]
        public async void ScanProximityCommand(int source, List<object> args, string raw)
        {
            if (await permission.HasPermission("owner"))
            {
                var distance = float.Parse(args[0].ToString());
                var pos = GetEntityCoords(PlayerPedId(), true, true);

                ScannedImaps.Clear();

                foreach (var imap in Imaps)
                {
                    if (GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, imap.X, imap.Y, imap.Z, true) <= distance)
                    {
                        logger.Debug("[Map] Scanning: " + imap.Hash);
                        ScannedImaps.Add(imap);
                    }
                }

                logger.Debug("[Map] Scanned imap count: " + ScannedImaps.Count);
            }
        }

        #endregion

        public void LoadCustomImaps()
        {
            foreach (var imap in CustomImaps)
            {
                var hash = (uint)long.Parse(imap.Hash);

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
                var hash = (uint)long.Parse(imap.Hash);

                if (IsImapActive(hash))
                    RemoveImap(hash);
            }
        }

        public void UnloadCustomImaps()
        {
            foreach (var imap in CustomImaps)
            {
                var hash = (uint)long.Parse(imap.Hash);

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
                Function.Call((Hash)0x174D0AAB11CED739, interior, entitySetName);
        }

        public void UnloadInterior(int interior, string entitySetName)
        {
            if (IsInteriorEntitySetActive(interior, entitySetName))
                Function.Call((Hash)0x33B81A2C07A51FFF, interior, entitySetName, true);
        }
    }
}
