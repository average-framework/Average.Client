using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Services
{
    internal class GameService : IService
    {
        public GameService()
        {
            Logger.Debug("GameService Initialized successfully");
        }

        internal void Init()
        {
            Call(0x4B8F743A4A6D2FF8, true);
        }
    }
}
