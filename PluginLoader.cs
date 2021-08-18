using Average.Client.Managers;
using CitizenFX.Core;
using Newtonsoft.Json;
using SDK.Client;
using SDK.Client.Plugins;
using SDK.Client.Rpc;
using SDK.Shared;
using SDK.Shared.Extensions;
using SDK.Shared.Plugins;
using SDK.Shared.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Average.Client
{
    internal class PluginLoader
    {
        RpcRequest rpc;
        CommandManager command;

        List<PluginInfo> pluginsInfo;

        bool isReady;

        public List<Plugin> Plugins { get; } = new List<Plugin>();

        public PluginLoader(RpcRequest rpc, CommandManager command)
        {
            this.rpc = rpc;
            this.command = command;
        }

        public async Task IsReady()
        {
            while (!isReady) await BaseScript.Delay(0);
        }

        async Task<List<PluginInfo>> GetPlugins()
        {
            rpc.Event("avg.internal.get_plugins").On(message =>
            {
                pluginsInfo = message.Args[0].Convert<List<PluginInfo>>();
            }).Emit();

            while (pluginsInfo == null) await BaseScript.Delay(250);
            return pluginsInfo;
        }

        public async void Load()
        {
            Main.logger.Debug("Getting plugins..");

            var pluginsInfo = await GetPlugins();

            Main.logger.Debug("Plugins getted.");

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = asm.GetTypes().Where(x => !x.IsAbstract && x.IsClass).ToList();
                    var pluginInfo = pluginsInfo.Find(x => x.Client == asm.GetName().Name + ".dll");

                    if (pluginInfo != null)
                    {
                        var mainScriptCount = 0;

                        foreach (var type in types)
                        {
                            var attr = type.GetCustomAttribute<MainScriptAttribute>();

                            if (attr != null)
                                mainScriptCount++;
                        }

                        if (mainScriptCount > 1)
                        {
                            Main.logger.Error("Unable to load multiples [MainScript] attribute in same plugin. Fix this error to continue.");
                            return;
                        }

                        if (mainScriptCount == 0)
                        {
                            Main.logger.Error($"Unable to load this plugin: {asm.FullName}, he does not contains [MainScript] attribute. Fix this error to continue.");
                            return;
                        }

                        foreach (var type in types)
                        {
                            Plugin script = null;

                            if (type.IsSubclassOf(typeof(Plugin)))
                            {
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
                            }

                            if (script == null)
                                continue;

                            RegisterThreads(type, script);
                            RegisterEvents(type, script);
                            RegisterExports(type, script);
                            RegisterSyncs(type, script);
                            RegisterGetSyncs(type, script);
                            RegisterNetworkGetSyncs(type, script);
                            RegisterNUICallbacks(type, script);
                            RegisterCommands(type, script);
                        }
                    }
                    else
                    {
                        //Main.logger.Error($"Unable to find plugin: {asm.GetName().Name}.dll");
                    }
                }

                isReady = true;
            }
            catch (Exception ex)
            {
                Main.logger.Error("Unknow StackTrace: " + JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        void RegisterCommands(Type type, object classObj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            // Registering commands (method need to be public to be detected)
            foreach (var method in type.GetMethods(flags))
            {
                var cmdAttr = method.GetCustomAttribute<ClientCommandAttribute>();
                var aliasAttr = method.GetCustomAttribute<ClientCommandAliasAttribute>();

                command.RegisterCommand(cmdAttr, aliasAttr, method, classObj);
            }
        }

        void RegisterThreads(Type type, object classObj)
        {
            // Registering threads
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
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
            // Registering events
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
            {
                var eventAttr = method.GetCustomAttribute<ClientEventAttribute>();

                if (eventAttr != null)
                {
                    Main.eventManager.RegisterEvent(method, eventAttr, classObj);
                }
            }
        }

        void RegisterExports(Type type, object classObj)
        {
            // Registering exports
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
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
            // Registering syncs
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            for (int i = 0; i < type.GetProperties(flags).Count(); i++)
            {
                var property = type.GetProperties(flags)[i];
                var syncAttr = property.GetCustomAttribute<SyncAttribute>();

                if (syncAttr != null)
                {
                    Main.syncManager.RegisterSync(ref property, syncAttr, classObj);
                }
            }

            for (int i = 0; i < type.GetFields(flags).Count(); i++)
            {
                var field = type.GetFields(flags)[i];
                var syncAttr = field.GetCustomAttribute<SyncAttribute>();

                if (syncAttr != null)
                {
                    Main.syncManager.RegisterSync(ref field, syncAttr, classObj);
                }
            }
        }

        void RegisterGetSyncs(Type type, object classObj)
        {
            // Registering getSyncs
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            for (int i = 0; i < type.GetProperties(flags).Count(); i++)
            {
                var property = type.GetProperties(flags)[i];
                var getSyncAttr = property.GetCustomAttribute<GetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterGetSync(ref property, getSyncAttr, classObj);
                }
            }

            for (int i = 0; i < type.GetFields(flags).Count(); i++)
            {
                var field = type.GetFields(flags)[i];
                var getSyncAttr = field.GetCustomAttribute<GetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterGetSync(ref field, getSyncAttr, classObj);
                }
            }
        }

        void RegisterNetworkGetSyncs(Type type, object classObj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            // Registering networkGetSyncs (property need to be public to be detected)
            for (int i = 0; i < type.GetProperties(flags).Count(); i++)
            {
                var property = type.GetProperties(flags)[i];
                var getSyncAttr = property.GetCustomAttribute<NetworkGetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterNetworkGetSync(ref property, getSyncAttr, classObj);
                }
            }

            // Registering networkGetSyncs (field need to be public to be detected)
            for (int i = 0; i < type.GetFields(flags).Count(); i++)
            {
                var field = type.GetFields(flags)[i];
                var getSyncAttr = field.GetCustomAttribute<NetworkGetSyncAttribute>();

                if (getSyncAttr != null)
                {
                    Main.syncManager.RegisterNetworkGetSync(ref field, getSyncAttr, classObj);
                }
            }
        }

        void RegisterNUICallbacks(Type type, object classObj)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attr = method.GetCustomAttribute<UICallbackAttribute>();
                var methodParams = method.GetParameters();

                if (attr != null)
                {
                    if (methodParams.Count() == 2)
                    {
                        if (methodParams[0].ParameterType == typeof(IDictionary<string, object>) && methodParams[1].ParameterType == typeof(CallbackDelegate))
                        {
                            try
                            {
                                var action = (Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate>)Action.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);
                                Main.eventManager.RegisterInternalNUICallbackEvent(attr.Name, action);
                                Main.logger.Debug($"Registering [UICallback] attribute: {attr.Name} on method: {method.Name}");
                            }
                            catch
                            {
                                Main.logger.Error($"Unable to cast [UICallback] attribute on method: {method.Name} to return type {method.ReturnType}. The return type need to be \"CallbackDelegate\".");
                            }
                        }
                        else
                        {
                            Main.logger.Error($"Unable to register [UICallback] attribute on method: {method.Name} because parameters type does not match with required parameters type. Method parameters need to be of type \"IDictionary<string, object>, CallbackDelegate\"");
                        }
                    }
                    else
                    {
                        Main.logger.Error($"Unable to register [UICallback] attribute on method: {method.Name} because this method does not contains required parameters count.");
                    }
                }
            }
        }

        void RegisterPlugin(Plugin script)
        {
            Plugins.Add(script);
        }

        void UnloadScript(Plugin script)
        {
            Plugins.Remove(script);
        }
    }
}
