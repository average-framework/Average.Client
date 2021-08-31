using System;
using System.Threading.Tasks;
using Average.Client.Controllers;
using Average.Client.Managers;
using CitizenFX.Core;
using SDK.Client.Diagnostics;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;

namespace Average.Client
{
    internal class Main : BaseScript
    {
        internal static PluginLoader loader;

        internal static Action<Func<Task>> attachCallback;
        internal static Action<Func<Task>> detachCallback;
        
        internal static EventHandlerDictionary eventHandlers;
        internal static RpcRequest rpc;

        #region Internal Manager

        internal static readonly CharacterManager character = new CharacterManager();
        internal static readonly CommandManager command = new CommandManager();
        internal static readonly EventManager evnt = new EventManager();
        internal static readonly ExportManager export = new ExportManager();
        internal static readonly PermissionManager permission = new PermissionManager();
        internal static readonly SaveManager save = new SaveManager();
        internal static readonly SyncManager sync = new SyncManager();
        internal static readonly ThreadManager thread = new ThreadManager();
        internal static readonly UserManager user = new UserManager();
        internal static readonly BlipManager blip = new BlipManager();
        internal static readonly NpcManager npc = new NpcManager();
        internal static readonly LanguageManager language = new LanguageManager();
        internal static readonly MapManager map = new MapManager();
        internal static readonly ObjectManager streaming = new ObjectManager();
        internal static readonly NotificationManager notification = new NotificationManager();
        internal static readonly MenuManager menu = new MenuManager();
        internal static readonly CfxManager cfx = new CfxManager();
        internal static readonly StorageManager storage = new StorageManager();

        #endregion;

        #region Internal Controller

        internal static readonly CraftController craft = new CraftController();

        #endregion
        
        public Main()
        {
            eventHandlers = EventHandlers;
            detachCallback = c => Tick -= c;
            attachCallback = c => Tick += c;
            
            rpc = new RpcRequest(new RpcHandler(EventHandlers), new RpcTrigger(), new RpcSerializer());

            // Plugin Loader
            loader = new PluginLoader();

            LoadInternalScript(language);
            LoadInternalScript(evnt);
            LoadInternalScript(export);
            LoadInternalScript(thread);
            LoadInternalScript(sync);
            LoadInternalScript(permission);
            LoadInternalScript(command);
            LoadInternalScript(save);
            LoadInternalScript(notification);
            LoadInternalScript(menu);
            LoadInternalScript(user);
            LoadInternalScript(blip);
            LoadInternalScript(npc);
            LoadInternalScript(character);
            LoadInternalScript(storage);
            LoadInternalScript(map);
            LoadInternalScript(streaming);
            LoadInternalScript(craft);
            LoadInternalScript(cfx);
            
            loader.Load();
        }
        
        internal void LoadInternalScript(InternalPlugin script)
        {
            try
            {
                script.SetDependencies(new RpcRequest(new RpcHandler(eventHandlers), new RpcTrigger(), new RpcSerializer()), thread, character, command, evnt, export, permission, save, sync, user, streaming, npc, menu, notification, language, map, blip, storage, craft);
                
                loader.RegisterThreads(script.GetType(), script);
                loader.RegisterEvents(script.GetType(), script);
                loader.RegisterNuiCallbacks(script.GetType(), script);
                loader.RegisterExports(script.GetType(), script);
                loader.RegisterSyncs(script.GetType(), script);
                loader.RegisterGetSyncs(script.GetType(), script);
                loader.RegisterNetworkGetSyncs(script.GetType(), script);
                loader.RegisterCommands(script.GetType(), script);
                loader.RegisterInternalPlugin(script);
                
                script.OnInitialized();
             
                Log.Info($"Script: {script.Name} registered successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to loading script: {script.Name}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
        }
    }
}
