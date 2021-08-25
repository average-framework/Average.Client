using CitizenFX.Core;
using System;

namespace Average.Client.Managers
{
    internal class CfxManager
    {
        public CfxManager()
        {
            Main.eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
            Main.eventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            Main.eventHandlers["onResourceStarting"] += new Action<string>(OnResourceStarting);
            Main.eventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            Main.eventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);
            Main.eventHandlers["gameEventTriggered"] += new Action<string, int[]>(OnGameEventTriggered);
            Main.eventHandlers["onClientMapStart"] += new Action<string>(OnClientMapStart);
            Main.eventHandlers["onClientMapStop"] += new Action<string>(OnClientMapStop);
            Main.eventHandlers["onClientGameTypeStart"] += new Action<string>(OnClientGameTypeStart);
            Main.eventHandlers["onClientGameTypeStop"] += new Action<string>(OnClientGameTypeStop);
            Main.eventHandlers["playerActivated"] += new Action(OnPlayerActivated);
            Main.eventHandlers["sessionInitialized"] += new Action(OnSessionInitialized);
        }

        #region Events

        protected void OnResourceStop(string resource)
        {
             Main.eventManager.OnResourceStop(resource);
        }

        protected void OnResourceStart(string resource)
        {
             Main.eventManager.OnResourceStart(resource);
        }

        protected void OnResourceStarting(string resource)
        {
             Main.eventManager.OnResourceStarting(resource);
        }

        protected void OnClientResourceStart(string resource)
        {
             Main.eventManager.OnClientResourceStart(resource);
        }

        protected void OnClientResourceStop(string resource)
        {
             Main.eventManager.OnClientResourceStop(resource);
        }

        protected void OnGameEventTriggered(string name, int[] data)
        {
             Main.eventManager.OnGameEventTriggered(name, data);
        }

        protected void OnClientMapStart(string resource)
        {
             Main.eventManager.OnClientMapStart(resource);
        }

        protected void OnClientMapStop(string resource)
        {
             Main.eventManager.OnClientMapStop(resource);
        }

        protected void OnClientGameTypeStart(string resource)
        {
             Main.eventManager.OnClientGameTypeStart(resource);
        }

        protected void OnClientGameTypeStop(string resource)
        {
             Main.eventManager.OnClientGameTypeStop(resource);
        }

        protected void OnPlayerActivated()
        {
             Main.eventManager.OnPlayerActivated();
        }

        protected void OnSessionInitialized()
        {
             Main.eventManager.OnPlayerActivated();
        }

        #endregion
    }
}
