using Average.Client.Managers;
using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;

namespace Average.Client
{
    internal class Main : BaseScript
    {
        internal static CommandManager commandManager;
        internal static Framework framework;
        internal static ThreadManager threadManager;
        internal static EventManager eventManager;
        internal static ExportManager exportManager;
        internal static SyncManager syncManager;
        internal static MenuManager menu;
        internal static BlipManager blipManager;
        internal static NpcManager npcManager;
        internal static UserManager userManager;
        internal static PermissionManager permissionManager;
        internal static MapManager mapManager;
        internal static CharacterManager characterManager;
        internal static LanguageManager languageManager;
        internal static SaveManager saveManager;
        internal static ObjectManager objectManager;
        internal static NotificationManager notificationManager;
        internal static RpcRequest rpc;
        internal static CfxManager cfx;
        internal static EventHandlerDictionary eventHandlers;
        internal static PluginLoader loader;

        internal static Main instance;
        
        public Main()
        {
            instance = this;
            eventHandlers = EventHandlers;
            
            rpc = new RpcRequest(new RpcHandler(EventHandlers), new RpcTrigger(), new RpcSerializer());

            // Internal Script
            languageManager = new LanguageManager();
            threadManager = new ThreadManager(c => Tick += c, c => Tick -= c);
            eventManager = new EventManager();
            exportManager = new ExportManager();
            syncManager = new SyncManager();
            saveManager = new SaveManager();
            menu = new MenuManager();
            blipManager = new BlipManager();
            npcManager = new NpcManager();
            userManager = new UserManager();
            permissionManager = new PermissionManager();
            commandManager = new CommandManager();
            mapManager = new MapManager();
            notificationManager = new NotificationManager();
            characterManager = new CharacterManager();
            objectManager = new ObjectManager();
            cfx = new CfxManager();

            // Framework Script
            framework = new Framework(languageManager, menu, threadManager, eventManager, exportManager, syncManager, commandManager, blipManager, npcManager, userManager, permissionManager, mapManager, characterManager, saveManager, objectManager, notificationManager, rpc);

            // Plugin Loader
            loader = new PluginLoader();
            loader.Load();
        }
    }
}
