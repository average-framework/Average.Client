using System;
using System.Threading.Tasks;
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

        #region Internal Scripts

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

        #endregion;
        
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
            LoadInternalScript(map);
            LoadInternalScript(streaming);
            LoadInternalScript(cfx);
            
            loader.Load();
        }
        
        internal void LoadInternalScript(InternalPlugin script)
        {
            try
            {
                script.SetDependencies(rpc, thread, character, command, evnt, export, permission, save, sync, user, streaming, npc, menu, notification, language, map, blip);
                
                loader.RegisterThreads(script.GetType(), script);
                loader.RegisterEvents(script.GetType(), script);
                loader.RegisterExports(script.GetType(), script);
                loader.RegisterSyncs(script.GetType(), script);
                loader.RegisterGetSyncs(script.GetType(), script);
                loader.RegisterNetworkGetSyncs(script.GetType(), script);
                loader.RegisterCommands(script.GetType(), script);
                loader.RegisterInternalPlugin(script);
                
                script.OnInitialized();
             
                Log.Info($"Script: {script.Name} registered successfully.");
                // Log.Write("Script", $"% {script.Name} % registered successfully.", new Log.TextColor(ConsoleColor.Blue, ConsoleColor.White));
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to loading script: {script.Name}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
        }
    }
}
