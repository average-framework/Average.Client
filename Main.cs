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

        internal static readonly CharacterManager character = new();
        internal static readonly CommandManager command = new();
        internal static readonly EventManager evnt = new();
        internal static readonly ExportManager export = new();
        internal static readonly PermissionManager permission = new();
        internal static readonly SaveManager save = new();
        internal static readonly SyncManager sync = new();
        internal static readonly ThreadManager thread = new();
        internal static readonly UserManager user = new();
        internal static readonly BlipManager blip = new();
        internal static readonly NpcManager npc = new();
        internal static readonly LanguageManager language = new();
        internal static readonly MapManager map = new();
        internal static readonly ObjectManager streaming = new();
        internal static readonly NotificationManager notification = new();
        internal static readonly MenuManager menu = new();
        internal static readonly RayMenuManager rayMenu = new();
        internal static readonly CfxManager cfx = new();
        internal static readonly StorageManager storage = new();
        internal static readonly DoorManager door = new();
        internal static readonly PromptManager prompt = new();
        internal static readonly JobManager job = new();
        internal static readonly EnterpriseManager enterprise = new();

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
            LoadInternalScript(rayMenu);
            LoadInternalScript(permission);
            LoadInternalScript(command);
            LoadInternalScript(save);
            LoadInternalScript(notification);
            LoadInternalScript(menu);
            LoadInternalScript(user);
            LoadInternalScript(blip);
            LoadInternalScript(npc);
            LoadInternalScript(prompt);
            LoadInternalScript(character);
            LoadInternalScript(job);
            LoadInternalScript(enterprise);
            LoadInternalScript(storage);
            LoadInternalScript(map);
            LoadInternalScript(streaming);
            LoadInternalScript(craft);
            LoadInternalScript(door);
            LoadInternalScript(cfx);

            loader.Load();
        }
        
        internal void LoadInternalScript(InternalPlugin script)
        {
            try
            {
                script.SetDependencies(new RpcRequest(new RpcHandler(eventHandlers), new RpcTrigger(), new RpcSerializer()), thread, character, command, evnt, export, permission, save, sync, user, streaming, npc, menu, notification, language, map, blip, storage, craft, door, prompt, rayMenu, job, enterprise);
                
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
