using CitizenFX.Core;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class SaveManager : ISaveManager
    {
        Logger logger;
        EventManager eventManager;

        List<ISaveable> tasks = new List<ISaveable>();

        public SaveManager(Logger logger, EventManager eventManager, EventHandlerDictionary eventHandlers)
        {
            this.logger = logger;
            this.eventManager = eventManager;

            #region Event

            eventHandlers["Save.All"] += new Action(SaveAllEvent);

            #endregion
        }

        public async Task SaveAll()
        {
            for (int i = 0; i < tasks.Count; i++)
                await tasks[i].Save();

            eventManager.EmitServer("Save.All");
            logger.Debug("[Save] Player data sended to server.");
        }

        public void AddInQueue(ISaveable saveable) => tasks.Add(saveable);

        public void DeleteFromQueue(ISaveable saveable) => tasks.Remove(saveable);

        #region Event

        protected async void SaveAllEvent() => await SaveAll();

        #endregion
    }
}
