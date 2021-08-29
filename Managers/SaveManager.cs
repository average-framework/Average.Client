using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDK.Client;

namespace Average.Client.Managers
{
    public class SaveManager : InternalPlugin, ISaveManager
    {
        private readonly List<ISaveable> _tasks = new List<ISaveable>();

        public async Task SaveAll()
        {
            for (int i = 0; i < _tasks.Count; i++)
                await _tasks[i].SaveData();

            Event.EmitServer("Save.All");
            Log.Debug("[Save] Player data sended to server.");
        }

        public void AddInQueue(ISaveable saveable) => _tasks.Add(saveable);

        public void DeleteFromQueue(ISaveable saveable) => _tasks.Remove(saveable);

        #region Event

        [ClientEvent("Save.All")]
        private async void SaveAllEvent() => await SaveAll();

        #endregion
    }
}
