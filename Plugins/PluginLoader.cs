using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Commands;
using SDK.Client.Extensions;
using SDK.Client.Plugins;
using SDK.Shared;
using SDK.Shared.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Average.Plugins
{
    internal class PluginLoader
    {
        CommandManager commandManager;

        List<IPlugin> plugins = new List<IPlugin>();

        public PluginLoader(CommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

        List<PluginInfo> pluginsInfo;

        async Task<List<PluginInfo>> GetPlugins()
        {
            Main.Event("avg.internal.get_plugins").On(message =>
            {
                pluginsInfo = message.Args[0].Convert<List<PluginInfo>>();
            }).Emit();

            while (pluginsInfo == null)
            {
                await BaseScript.Delay(250);
            }

            return pluginsInfo;
        }

        public async Task Load()
        {
            Main.logger.Debug("Getting plugins..");

            var pluginsInfo = await GetPlugins();

            Main.logger.Debug("Plugins getted.");

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var sdk = asm.GetCustomAttribute<ClientPluginAttribute>();

                    if (sdk != null)
                    {
                        var types = asm.GetTypes().Where(x => !x.IsAbstract && x.IsClass && x.IsSubclassOf(typeof(Plugin))).ToList();
                        var mainScriptCount = 0;

                        foreach (var type in types)
                        {
                            var attr = type.GetCustomAttribute<MainScriptAttribute>();

                            if (attr != null)
                            {
                                mainScriptCount++;
                            }
                        }

                        if (mainScriptCount > 1)
                        {
                            Main.logger.Error("Unable to load multiples [MainScript] attribute in same plugin. Fix this error to continue.");
                            return;
                        }

                        if (mainScriptCount == 0)
                        {
                            Main.logger.Error("Unable to load this plugin, he does not contains [MainScript] attribute. Fix this error to continue.");
                            return;
                        }

                        var pluginInfo = pluginsInfo.Find(x => x.Client == asm.GetName().Name + ".dll");

                        foreach (var type in types)
                        {
                            object classObj = null;

                            // Load script dynamically at runtime
                            if (type.GetCustomAttribute<MainScriptAttribute>() != null)
                            {
                                try
                                {
                                    if (pluginInfo != null)
                                    {
                                        classObj = Activator.CreateInstance(type, Main.framework, pluginInfo);
                                        var plugin = classObj as Plugin;
                                        plugin.PluginInfo = pluginInfo;
                                        RegisterPlugin(plugin);

                                        Main.logger.Info($"{asm.GetName().Name} Successfully loaded.");
                                    }
                                }
                                catch (InvalidCastException ex)
                                {
                                    Main.logger.Error($"Unable to load {asm.GetName().Name}");
                                }
                            }

                            if (classObj == null)
                            {
                                continue;
                            }

                            RegisterCommands(type, classObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Main.logger.Error(ex.StackTrace);
            }
        }

        void RegisterCommands(Type type, object classObj)
        {
            // Load registered commands (method need to be public to be detected)
            foreach (var method in type.GetMethods())
            {
                var cmdAttr = method.GetCustomAttribute<SDK.Client.CommandAttribute>();
                var aliasAttr = method.GetCustomAttribute<CommandAliasAttribute>();

                commandManager.RegisterCommand(cmdAttr, aliasAttr, method, classObj);
            }
        }

        void RegisterPlugin(IPlugin script)
        {
            BaseScript.RegisterScript((Plugin)script);
            plugins.Add(script);
        }

        void UnloadScript(IPlugin script)
        {
            BaseScript.UnregisterScript((Plugin)script);
            plugins.Remove(script);
        }
    }
}
