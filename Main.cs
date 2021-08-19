using Average.Client.Managers;
using Average.Client.Menu;
using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;
using System;
using System.Threading.Tasks;

namespace Average.Client
{
    internal class Main : BaseScript
    {
        internal static Logger logger;
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

        CfxManager cfx;
        RpcRequest rpc;

        internal static EventHandlerDictionary eventHandlers;

        internal static PluginLoader loader;

        public Main()
        {
            eventHandlers = EventHandlers;

            logger = new Logger();
            rpc = new RpcRequest(new RpcHandler(EventHandlers), new RpcTrigger(), new RpcSerializer());

            Configuration.Init(logger);

            // Internal Script
            languageManager = new LanguageManager();
            threadManager = new ThreadManager(c => Tick += c, c => Tick -= c);
            eventManager = new EventManager(EventHandlers, logger);
            exportManager = new ExportManager(logger);
            syncManager = new SyncManager(logger, EventHandlers, threadManager);
            saveManager = new SaveManager(logger, eventManager, EventHandlers);
            menu = new MenuManager(eventManager);
            blipManager = new BlipManager(EventHandlers);
            npcManager = new NpcManager(EventHandlers);
            userManager = new UserManager(logger, rpc);
            permissionManager = new PermissionManager(logger, rpc, userManager, EventHandlers);
            commandManager = new CommandManager(logger, permissionManager);
            mapManager = new MapManager(logger, permissionManager, threadManager);
            characterManager = new CharacterManager(logger, threadManager, eventManager, rpc, saveManager);
            objectManager = new ObjectManager();
            cfx = new CfxManager(EventHandlers, eventManager);

            // Framework Script
            framework = new Framework(languageManager, menu, threadManager, eventManager, exportManager, syncManager, logger, commandManager, blipManager, npcManager, userManager, permissionManager, mapManager, characterManager, saveManager, objectManager, rpc);

            // Plugin Loader
            loader = new PluginLoader(rpc, commandManager);
            loader.Load();
        }
    }
}
