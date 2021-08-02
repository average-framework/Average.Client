using CitizenFX.Core;
using System;

namespace Average.Managers
{
    internal class CfxManager
    {
        EventManager eventManager;

        public CfxManager(EventHandlerDictionary eventHandlers, EventManager eventManager)
        {
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

        protected void OnResourceStop(string resource)
        {
            eventManager.OnResourceStop(resource);
        }

        protected void OnResourceStart(string resource)
        {
            eventManager.OnResourceStart(resource);
        }

        protected void OnResourceStarting(string resource)
        {
            eventManager.OnResourceStarting(resource);
        }

        protected void OnClientResourceStart(string resource)
        {
            eventManager.OnClientResourceStart(resource);
        }

        protected void OnClientResourceStop(string resource)
        {
            eventManager.OnClientResourceStop(resource);
        }

        protected void OnGameEventTriggered(string name, int[] data)
        {
            eventManager.OnGameEventTriggered(name, data);
        }

        protected void OnClientMapStart(string resource)
        {
            eventManager.OnClientMapStart(resource);
        }

        protected void OnClientMapStop(string resource)
        {
            eventManager.OnClientMapStop(resource);
        }

        protected void OnClientGameTypeStart(string resource)
        {
            eventManager.OnClientGameTypeStart(resource);
        }

        protected void OnClientGameTypeStop(string resource)
        {
            eventManager.OnClientGameTypeStop(resource);
        }

        protected void OnPlayerActivated()
        {
            eventManager.OnPlayerActivated();
        }

        protected void OnSessionInitialized()
        {
            eventManager.OnPlayerActivated();
        }

        #endregion
    }
}
