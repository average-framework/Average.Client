using Average.Commands;
using Average.Events;
using Average.Exports;
using Average.Plugins;
using Average.Threading;
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

        internal SyncManager sync;
        internal RpcRequest rpc;

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
            framework = new Framework(threadManager, eventManager, exportManager, syncManager, logger, commandManager, rpc);
            plugin = new PluginLoader(rpc, commandManager);

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
