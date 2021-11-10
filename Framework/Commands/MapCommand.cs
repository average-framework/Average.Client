using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Services;
using CitizenFX.Core;
using System.Globalization;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Commands
{
    internal class MapCommand
    {
        private readonly MapService _mapService;

        public MapCommand(MapService mapService)
        {
            _mapService = mapService;
        }

        [ClientCommand("debug:disable_lowspecmode")]
        private async void LowSpecModeDisableCommand()
        {
            _mapService.StopLowSpecMode();

            await BaseScript.Delay(1000);

            for (int i = 0; i < _mapService.Imaps.Count; i++)
            {
                var imap = _mapService.Imaps[i];
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (!IsImapActive(hash))
                {
                    RequestImap(hash);
                }
            }

            for (int i = 0; i < _mapService.MyImaps.Count; i++)
            {
                var imap = _mapService.MyImaps[i];
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
        }

        [ClientCommand("debug:dist_lowspecmode")]
        private void LowSpecModeDistanceCommand(float lodDistance)
        {
            _mapService.lodDistance = lodDistance;
        }

        [ClientCommand("debug:lowspecmode")]
        private async void LowSpecModeCommand()
        {
            // Décharge tout les imaps par défaut
            for (int i = 0; i < _mapService.Imaps.Count; i++)
            {
                var imap = _mapService.Imaps[i];
                var hash = uint.Parse(imap.Hash, NumberStyles.AllowHexSpecifier);

                if (IsImapActive(hash))
                {
                    RemoveImap(hash);
                    Logger.Warn("Unloading imap: " + imap.Hash);
                }
            }

            Logger.Debug("All imaps suppressed");

            await BaseScript.Delay(1000);

            Logger.Debug("Starting déferring imap loading");

            _mapService.StartLowSpecMode();
        }
    }
}
