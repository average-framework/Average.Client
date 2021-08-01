using CitizenFX.Core;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Average.Managers
{
    public class InternalManager : IInternal
    {
        List<IPlugin> plugins;
        Logger logger;

        public bool IsWorking { get; set; }

        public InternalManager(Logger logger)
        {
            this.logger = logger;
        }

        public IEnumerable<string> GetRegisteredPluginsNamespace() => plugins.Select(x => x.GetType().FullName);

        public void SetPluginList(ref List<IPlugin> plugins) => this.plugins = plugins;

        public async Task<T> GetPluginInstance<T>(string pluginName)
        {
            try
            {
                while (IsWorking == true) await BaseScript.Delay(0);

                var instance = plugins.Find(x => x.GetType().FullName == pluginName);

                if (instance == null)
                {
                    logger.Error($"This class namespace / class does not exists. This namespace can be changed by another one, please contact owner of this script for more informations. [{pluginName}]");
                    return default(T);
                }

                return (T)instance;
            }
            catch
            {
                logger.Error($"This class namespace / class does not exists. This namespace can be changed by another one, please contact owner of this script for more informations. [{pluginName}]");
            }

            return default(T);
        }

        public dynamic GetPluginInstance(string pluginName)
        {
            var instance = plugins.Find(x => x.GetType().FullName == pluginName);

            if (instance == null)
            {
                logger.Error($"This class namespace / class does not exists. This namespace can be changed by another one, please contact owner of this script for more informations. [{pluginName}]");
                return null;
            }

            return (dynamic)instance;
        }
    }
}
