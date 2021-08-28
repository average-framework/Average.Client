using System;

namespace Average.Client.Managers
{
    internal class CfxManager : InternalPlugin
    {
         public override void OnInitialized()
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
             Event.OnResourceStop(resource);
        }

        private void OnResourceStart(string resource)
        {
             Event.OnResourceStart(resource);
        }

        private void OnResourceStarting(string resource)
        {
             Event.OnResourceStarting(resource);
        }

        private void OnClientResourceStart(string resource)
        {
             Event.OnClientResourceStart(resource);
        }

        private void OnClientResourceStop(string resource)
        {
             Event.OnClientResourceStop(resource);
        }

        private void OnGameEventTriggered(string name, int[] data)
        {
             Event.OnGameEventTriggered(name, data);
        }

        private void OnClientMapStart(string resource)
        {
             Event.OnClientMapStart(resource);
        }

        private void OnClientMapStop(string resource)
        {
             Event.OnClientMapStop(resource);
        }

        private void OnClientGameTypeStart(string resource)
        {
             Event.OnClientGameTypeStart(resource);
        }

        private void OnClientGameTypeStop(string resource)
        {
             Event.OnClientGameTypeStop(resource);
        }

        private void OnPlayerActivated()
        {
             Event.OnPlayerActivated();
        }

        private void OnSessionInitialized()
        {
             Event.OnSessionInitialized();
        }

        #endregion
    }
}
