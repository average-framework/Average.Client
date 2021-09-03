using CitizenFX.Core;
using Newtonsoft.Json;
using SDK.Client;
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
using Average.Client.Managers;
using SDK.Client.Diagnostics;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;

namespace Average.Client
{
    internal class PluginLoader
    {
        private bool _isReady;

        private List<PluginInfo> _pluginsInfo;
        private List<InternalPlugin> _internalPlugins = new List<InternalPlugin>();
        private List<Plugin> _plugins = new List<Plugin>();

        public async Task IsReady()
        {
            while (!_isReady) await BaseScript.Delay(0);
        }

        private async Task<List<PluginInfo>> GetPlugins()
        {
            Main.rpc.Event("avg.internal.get_plugins").On(message =>
            {
                _pluginsInfo = message.Args[0].Convert<List<PluginInfo>>();
            }).Emit();

            while (_pluginsInfo == null) await BaseScript.Delay(0);
            return _pluginsInfo;
        }

        internal void LoadScript(Type type, PluginInfo pluginInfo)
        {
            try
            {
                var script = (Plugin) Activator.CreateInstance(type);
                
                script.SetDependencies(new RpcRequest(new RpcHandler(Main.eventHandlers), new RpcTrigger(), new RpcSerializer()), Main.thread, Main.character, Main.command, Main.evnt, Main.export, Main.permission, Main.save, Main.sync, Main.user, Main.streaming, Main.npc, Main.menu, Main.notification, Main.language, Main.map, Main.blip, Main.storage, Main.craft, Main.door, Main.prompt, pluginInfo);
                
                RegisterThreads(script.GetType(), script);
                RegisterEvents(script.GetType(), script);
                RegisterNuiCallbacks(script.GetType(), script);
                RegisterExports(script.GetType(), script);
                RegisterSyncs(script.GetType(), script);
                RegisterGetSyncs(script.GetType(), script);
                RegisterNetworkGetSyncs(script.GetType(), script);
                RegisterCommands(script.GetType(), script);
                RegisterPlugin(script);
                
                script.LoadConfiguration();
                script.OnInitialized();
             
                Log.Info($"Script: {script.Name} registered successfully.");
                // Log.Write("Script", $"% {script.Name} % registered successfully.", new Log.TextColor(ConsoleColor.Blue, ConsoleColor.White));
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to loading script: {type.Name}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
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
                    var types = asm.GetTypes().Where(x => !x.IsAbstract && x.IsClass && x.IsSubclassOf(typeof(Plugin))).ToList();
                    var pluginInfo = _pluginsInfo.Find(x => x.Client == asm.GetName().Name + ".dll");
            
                    if (pluginInfo is null) continue;

                    foreach (var type in types)
                    {
                        LoadScript(type, pluginInfo);
                    }
                }
            
                _isReady = true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknow StackTrace: " + JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        internal void RegisterInternalPlugin(InternalPlugin script)
        {
            _internalPlugins.Add(script);
        }

        internal void UnloadInternalScript(InternalPlugin script)
        {
            _internalPlugins.Remove(script);
        }
        
        internal T GetInternalInstance<T>()
        {
            var result = _internalPlugins.Find(x => x.GetType() == typeof(T));
            return (T) Convert.ChangeType(result, typeof(T));
        }
        
        internal void RegisterCommands(Type type, object classObj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            // Registering commands (method need to be public to be detected)
            foreach (var method in type.GetMethods(flags))
            {
                var cmdAttr = method.GetCustomAttribute<ClientCommandAttribute>();
                var aliasAttr = method.GetCustomAttribute<ClientCommandAliasAttribute>();

                if(cmdAttr != null)
                    CommandManager.RegisterInternalCommand(cmdAttr, aliasAttr, classObj, method);
            }
        }

        internal void RegisterThreads(Type type, object classObj)
        {
            // Registering threads
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
            {
                var threadAttr = method.GetCustomAttribute<ThreadAttribute>();

                if (threadAttr != null)
                    ThreadManager.RegisterInternalThread(method, threadAttr, classObj);
            }
        }

        internal void RegisterEvents(Type type, object classObj)
        {
            // Registering events
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
            {
                var eventAttr = method.GetCustomAttribute<ClientEventAttribute>();

                if (eventAttr != null)
                    EventManager.RegisterInternalEvent(method, eventAttr, classObj);
            }
        }
        
        internal void RegisterNuiCallbacks(Type type, object classObj)
        {
            // Registering nui callbacks
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
            {
                var eventAttr = method.GetCustomAttribute<UICallbackAttribute>();
                
                if (eventAttr != null)
                    EventManager.RegisterInternalNuiCallbackEvent(method, eventAttr, classObj);
            }
        }

        internal void RegisterExports(Type type, object classObj)
        {
            // Registering exports
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            foreach (var method in type.GetMethods(flags))
            {
                var exportAttr = method.GetCustomAttribute<ExportAttribute>();

                if (exportAttr != null)
                    ExportManager.RegisterInternalExport(method, exportAttr, classObj);
            }
        }

        internal void RegisterSyncs(Type type, object classObj)
        {
            // Registering syncs
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            for (int i = 0; i < type.GetProperties(flags).Count(); i++)
            {
                var property = type.GetProperties(flags)[i];
                var syncAttr = property.GetCustomAttribute<SyncAttribute>();

                if (syncAttr != null)
                    SyncManager.RegisterInternalSync(ref property, syncAttr, classObj);
            }

            for (int i = 0; i < type.GetFields(flags).Count(); i++)
            {
                var field = type.GetFields(flags)[i];
                var syncAttr = field.GetCustomAttribute<SyncAttribute>();

                if (syncAttr != null)
                    SyncManager.RegisterInternalSync(ref field, syncAttr, classObj);
            }
        }

        internal void RegisterGetSyncs(Type type, object classObj)
        {
            // Registering getSyncs
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            for (int i = 0; i < type.GetProperties(flags).Count(); i++)
            {
                var property = type.GetProperties(flags)[i];
                var getSyncAttr = property.GetCustomAttribute<GetSyncAttribute>();

                if (getSyncAttr != null)
                    SyncManager.RegisterInternalGetSync(ref property, getSyncAttr, classObj);
            }

            for (int i = 0; i < type.GetFields(flags).Count(); i++)
            {
                var field = type.GetFields(flags)[i];
                var getSyncAttr = field.GetCustomAttribute<GetSyncAttribute>();

                if (getSyncAttr != null)
                    SyncManager.RegisterInternalGetSync(ref field, getSyncAttr, classObj);
            }
        }

        internal void RegisterNetworkGetSyncs(Type type, object classObj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            // Registering networkGetSyncs (property need to be public to be detected)
            for (int i = 0; i < type.GetProperties(flags).Count(); i++)
            {
                var property = type.GetProperties(flags)[i];
                var getSyncAttr = property.GetCustomAttribute<NetworkGetSyncAttribute>();

                if (getSyncAttr != null)
                    SyncManager.RegisterInternalNetworkGetSync(ref property, getSyncAttr, classObj);
            }

            // Registering networkGetSyncs (field need to be public to be detected)
            for (int i = 0; i < type.GetFields(flags).Count(); i++)
            {
                var field = type.GetFields(flags)[i];
                var getSyncAttr = field.GetCustomAttribute<NetworkGetSyncAttribute>();

                if (getSyncAttr != null)
                    SyncManager.RegisterInternalNetworkGetSync(ref field, getSyncAttr, classObj);
            }
        }

        internal void RegisterNUICallbacks(Type type, object classObj)
        {
            foreach (var method in
                type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attr = method.GetCustomAttribute<UICallbackAttribute>();
                var methodParams = method.GetParameters();

                if (attr != null)
                {
                    if (methodParams.Count() == 2)
                    {
                        if (methodParams[0].ParameterType == typeof(IDictionary<string, object>) &&
                            methodParams[1].ParameterType == typeof(CallbackDelegate))
                        {
                            try
                            {
                                var action = (Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate>) Delegate.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] {method.ReturnType}).ToArray()), classObj, method);
                                EventManager.RegisterInternalNuiCallbackEvent(attr.Name, action);
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

        internal void RegisterPlugin(Plugin script)
        {
            _plugins.Add(script);
        }

        internal void UnloadScript(Plugin script)
        {
            _plugins.Remove(script);
        }
    }
}