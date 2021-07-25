using CitizenFX.Core;
using System.Threading.Tasks;

namespace Average
{
    public class SyncManager : BaseScript
    {
        SDK.Client.SyncManager syncManager;
        int syncRate;

        public SyncManager(SDK.Client.SyncManager syncManager, int syncRate = 60)
        {
            this.syncManager = syncManager;
            this.syncRate = syncRate;

            Tick += SyncUpdate;
        }

        protected async Task SyncUpdate()
        {
            await Delay(syncRate);

            syncManager.SyncProperties();
            syncManager.SyncFields();
        }
    }
}
