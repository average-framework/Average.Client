using Average.Managers;
using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;
using System;
using System.Threading.Tasks;

namespace Average
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
        internal static InternalManager internalManager;

        CfxManager cfx;
        RpcRequest rpc;
        PluginLoader plugin;

        public Main()
        {
            rpc = new RpcRequest(new RpcHandler(EventHandlers), new RpcTrigger(), new RpcSerializer());
            logger = new Logger();

            commandManager = new CommandManager(logger);
            threadManager = new ThreadManager(this);
            eventManager = new EventManager(EventHandlers, logger);
            exportManager = new ExportManager(logger);
            syncManager = new SyncManager(EventHandlers, logger);
            internalManager = new InternalManager();
            framework = new Framework(threadManager, eventManager, exportManager, syncManager, logger, commandManager, internalManager, rpc);
            cfx = new CfxManager(EventHandlers, eventManager);
            plugin = new PluginLoader(rpc, commandManager);
            internalManager.SetPluginList(ref plugin.plugins);

            plugin.Load();
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
