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
        internal static InternalManager internalManager;

        CfxManager cfx;
        RpcRequest rpc;
        internal static PluginLoader loader;

        public Main()
        {
            Task.Factory.StartNew(async () =>
            {
                await Delay(0);

                rpc = new RpcRequest(new RpcHandler(EventHandlers), new RpcTrigger(), new RpcSerializer());
                logger = new Logger();

                Configuration.Init(logger);

                framework = new Framework();
                commandManager = new CommandManager(logger);
                threadManager = new ThreadManager(c => Tick += c, c => Tick -= c);
                eventManager = new EventManager(EventHandlers, logger);
                exportManager = new ExportManager(logger);
                syncManager = new SyncManager(EventHandlers, logger, framework);
                menu = new MenuManager(eventManager);
                blipManager = new BlipManager(EventHandlers);
                npcManager = new NpcManager(EventHandlers);
                userManager = new UserManager(framework);
                permissionManager = new PermissionManager(framework);
                mapManager = new MapManager(framework);
                characterManager = new CharacterManager(framework);
                cfx = new CfxManager(EventHandlers, eventManager);
                internalManager = new InternalManager(logger);

                framework.SetDependencies(menu, threadManager, eventManager, exportManager, syncManager, logger, commandManager, internalManager, blipManager, npcManager, userManager, permissionManager, mapManager, characterManager, rpc);

                loader = new PluginLoader(framework);
                await loader.Load();
                await loader.IsPluginsFullyLoaded();

                var plugins = loader.Plugins;
                internalManager.SetPlugins(ref plugins);

                //var plugins = loader.Plugins;
                //internalManager.SetPluginList(ref plugins);

                framework.IsReadyToWork = true;
            });
        }

        internal void RegisterTick(Func<Task> func)
        {
            Tick += func;
        }

        internal void UnregisterTick(Func<Task> func)
        {
            Tick -= func;
        }
    }
}
