using System;

namespace Average.Client.Managers
{
    internal class CfxManager
    {
        public CfxManager()
        {
             #region Event

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

             #endregion
        }

        #region Event

        private void OnResourceStop(string resource)
        {
             Main.eventManager.OnResourceStop(resource);
        }

        private void OnResourceStart(string resource)
        {
             Main.eventManager.OnResourceStart(resource);
        }

        private void OnResourceStarting(string resource)
        {
             Main.eventManager.OnResourceStarting(resource);
        }

        private void OnClientResourceStart(string resource)
        {
             Main.eventManager.OnClientResourceStart(resource);
        }

        private void OnClientResourceStop(string resource)
        {
             Main.eventManager.OnClientResourceStop(resource);
        }

        private void OnGameEventTriggered(string name, int[] data)
        {
             Main.eventManager.OnGameEventTriggered(name, data);
        }

        private void OnClientMapStart(string resource)
        {
             Main.eventManager.OnClientMapStart(resource);
        }

        private void OnClientMapStop(string resource)
        {
             Main.eventManager.OnClientMapStop(resource);
        }

        private void OnClientGameTypeStart(string resource)
        {
             Main.eventManager.OnClientGameTypeStart(resource);
        }

        private void OnClientGameTypeStop(string resource)
        {
             Main.eventManager.OnClientGameTypeStop(resource);
        }

        private void OnPlayerActivated()
        {
             Main.eventManager.OnPlayerActivated();
        }

        private void OnSessionInitialized()
        {
             Main.eventManager.OnSessionInitialized();
        }

        #endregion
    }
}
