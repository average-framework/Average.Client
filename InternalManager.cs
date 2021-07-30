using SDK.Client.Interfaces;
using SDK.Shared.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Average.Managers
{
    public class InternalManager : IInternal
    {
        List<IPlugin> plugins;

        public InternalManager()
        {
        
        }

        public IEnumerable<string> GetRegisteredPluginsNamespace()
        {
            return plugins.Select(x => x.GetType().FullName);
        }

        public void SetPluginList(ref List<IPlugin> plugins)
        {
            this.plugins = plugins;
        }

        public T GetPluginInstance<T>(string pluginName)
        {
            try
            {
                var instance = plugins.Find(x => x.GetType().FullName == pluginName);

                if (instance != null)
                {
                    return (T)instance;
                }
            }
            catch
            {

            }

            return default(T);
        }

        public dynamic GetPluginInstance(string pluginName)
        {
            var instance = plugins.Find(x => x.GetType().FullName == pluginName);

            if (instance != null)
            {
                return (dynamic)instance;
            }

            return null;
        }
    }
}
