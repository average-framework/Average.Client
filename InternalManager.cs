using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class InternalManager : IInternal
    {
        List<IPlugin> plugins = new List<IPlugin>();
        Logger logger;

        public InternalManager(Logger logger)
        {
            this.logger = logger;
        }

        public IEnumerable<string> GetRegisteredPluginsNamespace() => plugins.Select(x => x.GetType().FullName);

        public void SetPlugins(ref List<IPlugin> plugins) => this.plugins = plugins;

        public async Task<dynamic> GetPluginInstance(string pluginName)
        {
            await Main.loader.IsPluginsFullyLoaded();

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
