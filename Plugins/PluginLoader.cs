using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Commands;
using SDK.Client.Plugins;
using SDK.Shared;
using SDK.Shared.Extensions;
using SDK.Shared.Plugins;
using SDK.Shared.Threading;
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
        List<PluginInfo> pluginsInfo;

        public PluginLoader(CommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

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

                        if (pluginInfo != null)
                        {
                            foreach (var type in types)
                            {
                                Plugin script = null;

                                // Load script dynamically at runtime
                                if (type.GetCustomAttribute<MainScriptAttribute>() != null)
                                {
                                    try
                                    {
                                        if (pluginInfo != null)
                                        {
                                            script = (Plugin)Activator.CreateInstance(type, Main.framework, pluginInfo);
                                            script.PluginInfo = pluginInfo;
                                            RegisterPlugin(script);

                                            Main.logger.Info($"Plugin {asm.GetName().Name} -> script: {script.Name} successfully loaded.");
                                        }
                                    }
                                    catch (InvalidCastException ex)
                                    {
                                        Main.logger.Error($"Unable to load {asm.GetName().Name}");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        script = (Plugin)Activator.CreateInstance(type, Main.framework, pluginInfo);
                                        script.PluginInfo = pluginInfo;
                                        RegisterPlugin(script);

                                        Main.logger.Info($"Plugin {asm.GetName().Name} -> script: {script.Name} successfully loaded.");
                                    }
                                    catch
                                    {
                                        Main.logger.Error($"Unable to load script: {script.Name}");
                                    }
                                }

                                if (script == null)
                                {
                                    continue;
                                }

                                RegisterCommands(type, script);
                                RegisterThreads(type, script);
                                RegisterEvents(type, script);
                                RegisterExports(type, script);
                                RegisterSyncs(type, script);
                                RegisterGetSyncs(type, script);
                                RegisterNetworkGetSyncs(type, script);
                            }
                        }
                        else
                        {
                            Main.logger.Error($"Unable to find plugin: {asm.GetName().Name}.dll");
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
            // Registering commands (method need to be public to be detected)
            foreach (var method in type.GetMethods())
            {
                var cmdAttr = method.GetCustomAttribute<SDK.Client.CommandAttribute>();
                var aliasAttr = method.GetCustomAttribute<CommandAliasAttribute>();

                commandManager.RegisterCommand(cmdAttr, aliasAttr, method, classObj);
            }
        }

        void RegisterThreads(Type type, object classObj)
        {
            // Registering threads (method need to be public to be detected)
            foreach (var method in type.GetMethods())
            {
                var threadAttr = method.GetCustomAttribute<ThreadAttribute>();

                if (threadAttr != null)
                {
                    Main.threadManager.RegisterThread(method, threadAttr, classObj);
                }
            }
        }

        void RegisterEvents(Type type, object classObj)
        {
            // Registering events (method need to be public to be detected)
            foreach (var method in type.GetMethods())
            {
                var eventAttr = method.GetCustomAttribute<EventAttribute>();

                if (eventAttr != null)
                {
                    Main.eventManager.RegisterEvent(method, eventAttr, classObj);
                }
            }
        }

        void RegisterExports(Type type, object classObj)
        {
            // Registering exports (method need to be public to be detected)
            foreach (var method in type.GetMethods())
            {
                var exportAttr = method.GetCustomAttribute<ExportAttribute>();

                if (exportAttr != null)
                {
                    Main.exportManager.RegisterExport(method, exportAttr, classObj);
                }
            }
        }

        void RegisterSyncs(Type type, object classObj)
        {
            // Registering syncs (method need to be public to be detected)
            for (int i = 0; i < type.GetProperties().Count(); i++)
            {
                var property = type.GetProperties()[i];
                var syncAttr = property.GetCustomAttribute<SyncAttribute>();

                if (syncAttr != null)
                {
                    Main.syncManager.RegisterSync(ref property, syncAttr, classObj);
                }
            }

            for (int i = 0; i < type.GetFields().Count(); i++)
            {
                var field = type.GetFields()[i];
                var syncAttr = field.GetCustomAttribute<SyncAttribute>();

                if (syncAttr != null)
                {
                    Main.syncManager.RegisterSync(ref field, syncAttr, classObj);
                }
            }
        }

        void RegisterGetSyncs(Type type, object classObj)
        {
            // Registering getSyncs (method need to be public to be detected)
            for (int i = 0; i < type.GetProperties().Count(); i++)
            {
                var property = type.GetProperties()[i];
                var getSyncAttr = property.GetCustomAttribute<GetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterGetSync(ref property, getSyncAttr, classObj);
                }
            }

            for (int i = 0; i < type.GetFields().Count(); i++)
            {
                var field = type.GetFields()[i];
                var getSyncAttr = field.GetCustomAttribute<GetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterGetSync(ref field, getSyncAttr, classObj);
                }
            }
        }

        void RegisterNetworkGetSyncs(Type type, object classObj)
        {
            // Registering networkGetSyncs (property need to be public to be detected)
            for (int i = 0; i < type.GetProperties().Count(); i++)
            {
                var property = type.GetProperties()[i];
                var getSyncAttr = property.GetCustomAttribute<NetworkGetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterNetworkGetSync(ref property, getSyncAttr, classObj);
                }
            }

            // Registering networkGetSyncs (field need to be public to be detected)
            for (int i = 0; i < type.GetFields().Count(); i++)
            {
                var field = type.GetFields()[i];
                var getSyncAttr = field.GetCustomAttribute<NetworkGetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterNetworkGetSync(ref field, getSyncAttr, classObj);
                }
            }
        }

        void RegisterPlugin(IPlugin script)
        {
            //BaseScript.RegisterScript((Plugin)script);
            plugins.Add(script);
        }

        void UnloadScript(IPlugin script)
        {
            //BaseScript.UnregisterScript((Plugin)script);
            plugins.Remove(script);
        }
    }
}
