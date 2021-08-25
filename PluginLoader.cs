using CitizenFX.Core;
using Newtonsoft.Json;
using SDK.Client;
using SDK.Client.Plugins;
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
using SDK.Client.Diagnostics;

#pragma warning disable 8600

namespace Average.Client
{
    internal class PluginLoader
    {
        private List<PluginInfo> _pluginsInfo;

        bool isReady;

        private List<Plugin> _plugins;

        public async Task IsReady()
        {
            while (!isReady) await BaseScript.Delay(0);
        }

        async Task<List<PluginInfo>> GetPlugins()
        {
            Main.rpc.Event("avg.internal.get_plugins").On(message =>
            {
                _pluginsInfo = message.Args[0].Convert<List<PluginInfo>>();
            }).Emit();

            while (_pluginsInfo == null) await BaseScript.Delay(0);
            return _pluginsInfo;
        }

        public async void Load()
        {
            Log.Debug("Getting plugins..");

            _pluginsInfo = await GetPlugins();

            Log.Debug("Plugins getted.");

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = asm.GetTypes().Where(x => !x.IsAbstract && x.IsClass).ToList();
                    var pluginInfo = _pluginsInfo.Find(x => x.Client == asm.GetName().Name + ".dll");

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
                            Log.Error("Unable to load multiples [MainScript] attribute in same plugin. Fix this error to continue.");
                            return;
                        }

                        if (mainScriptCount == 0)
                        {
                            Log.Error($"Unable to load this plugin: {asm.FullName}, he does not contains [MainScript] attribute. Fix this error to continue.");
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

                                            Log.Info($"Plugin {asm.GetName().Name} -> script: {script.Name} successfully loaded.");
                                        }
                                    }
                                    catch (InvalidCastException ex)
                                    {
                                        Log.Error($"Unable to load {asm.GetName().Name}");
                                    }
                                }
                                else
                                {
                                    if (pluginInfo != null)
                                    {
                                        try
                                        {
                                            script = (Plugin)Activator.CreateInstance(type, Main.framework, pluginInfo);
                                            script.PluginInfo = pluginInfo;
                                            RegisterPlugin(script);

                                            Log.Info($"Plugin {asm.GetName().Name} -> script: {script.Name} successfully loaded.");
                                        }
                                        catch
                                        {
                                            Log.Error($"Unable to load script: {pluginInfo.Name}");
                                        }   
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
                        //Log.Error($"Unable to find plugin: {asm.GetName().Name}.dll");
                    }
                }

                isReady = true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknow StackTrace: " + JsonConvert.SerializeObject(ex, Formatting.Indented));
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

                Main.commandManager.RegisterCommand(cmdAttr, aliasAttr, method, classObj);
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
                                Log.Debug($"Registering [UICallback] attribute: {attr.Name} on method: {method.Name}");
                            }
                            catch
                            {
                                Log.Error($"Unable to cast [UICallback] attribute on method: {method.Name} to return type {method.ReturnType}. The return type need to be \"CallbackDelegate\".");
                            }
                        }
                        else
                        {
                            Log.Error($"Unable to register [UICallback] attribute on method: {method.Name} because parameters type does not match with required parameters type. Method parameters need to be of type \"IDictionary<string, object>, CallbackDelegate\"");
                        }
                    }
                    else
                    {
                        Log.Error($"Unable to register [UICallback] attribute on method: {method.Name} because this method does not contains required parameters count.");
                    }
                }
            }
        }

        void RegisterPlugin(Plugin script)
        {
            _plugins.Add(script);
        }

        void UnloadScript(Plugin script)
        {
            _plugins.Remove(script);
        }
    }
}
