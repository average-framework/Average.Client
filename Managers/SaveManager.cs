using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class SaveManager : InternalPlugin, ISaveManager
    {
        private List<ISaveable> _tasks = new List<ISaveable>();

        public override void OnInitialized()
        {
            #region Event

            Main.eventHandlers["Save.All"] += new Action(SaveAllEvent);

            #endregion
        }

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

        protected async void SaveAllEvent() => await SaveAll();

        #endregion
    }
}
