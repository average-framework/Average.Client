using CitizenFX.Core;
using System;

namespace Average.Managers
{
    internal class CfxManager
    {
        EventHandlerDictionary eventHandlers;
        EventManager eventManager;

        public CfxManager(EventHandlerDictionary eventHandlers, EventManager eventManager)
        {
            this.eventHandlers = eventHandlers;
            this.eventManager = eventManager;

            eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
            eventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            eventHandlers["onResourceStarting"] += new Action<string>(OnResourceStarting);
            eventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            eventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);
            eventHandlers["gameEventTriggered"] += new Action<string, int[]>(OnGameEventTriggered);
            eventHandlers["onClientMapStart"] += new Action<string>(OnClientMapStart);
            eventHandlers["onClientMapStop"] += new Action<string>(OnClientMapStop);
            eventHandlers["onClientGameTypeStart"] += new Action<string>(OnClientGameTypeStart);
            eventHandlers["onClientGameTypeStop"] += new Action<string>(OnClientGameTypeStop);
            eventHandlers["playerActivated"] += new Action(OnPlayerActivated);
            eventHandlers["sessionInitialized"] += new Action(OnSessionInitialized);
        }

        #region Events

        protected async void OnResourceStop(string resource)
        {
            eventManager.OnResourceStop(resource);
        }

        protected async void OnResourceStart(string resource)
        {
            eventManager.OnResourceStart(resource);
        }

        protected async void OnResourceStarting(string resource)
        {
            eventManager.OnResourceStarting(resource);
        }

        protected async void OnClientResourceStart(string resource)
        {
            eventManager.OnClientResourceStart(resource);
        }

        protected async void OnClientResourceStop(string resource)
        {
            eventManager.OnClientResourceStop(resource);
        }

        protected async void OnGameEventTriggered(string name, int[] data)
        {
            eventManager.OnGameEventTriggered(name, data);
        }

        protected async void OnClientMapStart(string resource)
        {
            eventManager.OnClientMapStart(resource);
        }

        protected async void OnClientMapStop(string resource)
        {
            eventManager.OnClientMapStop(resource);
        }

        protected async void OnClientGameTypeStart(string resource)
        {
            eventManager.OnClientGameTypeStart(resource);
        }

        protected async void OnClientGameTypeStop(string resource)
        {
            eventManager.OnClientGameTypeStop(resource);
        }

        protected async void OnPlayerActivated()
        {
            eventManager.OnPlayerActivated();
        }

        protected async void OnSessionInitialized()
        {
            eventManager.OnPlayerActivated();
        }

        #endregion
    }
}
