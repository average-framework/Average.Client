using Average.Plugins;
using Average.Threading;
using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Commands;
using SDK.Client.Diagnostics;
using SDK.Client.Events;
using SDK.Client.Exports;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;
using System;
using System.Threading.Tasks;

namespace Average
{
    internal class Main : BaseScript
    {
        static EventHandlerDictionary eventHandlers;

        internal static Logger logger;
        internal static CommandManager commandManager;
        internal static Framework framework;
        internal static ThreadManager threadManager;
        internal static EventManager eventManager;
        internal static ExportManager exportManager;
        internal static SDK.Client.SyncManager syncManager;

        internal SyncManager sync;

        PluginLoader plugin;

        public Main()
        {
            eventHandlers = EventHandlers;

            logger = new Logger();
            commandManager = new CommandManager(logger);
            threadManager = new ThreadManager(this);
            eventManager = new EventManager(EventHandlers, logger);
            exportManager = new ExportManager(logger);
            syncManager = new SDK.Client.SyncManager(EventHandlers, logger);
            framework = new Framework(threadManager, eventManager, exportManager, syncManager, logger, commandManager);
            plugin = new PluginLoader(commandManager);

            plugin.Load();

            sync = new SyncManager(syncManager);
            RegisterScript(sync);
        }

        internal static RpcRequest Event(string @event)
        {
            return new RpcRequest(@event, new RpcHandler(eventHandlers), new RpcTrigger(), new RpcSerializer());
        }

        /// <summary>
        /// Create new thread at runtime
        /// </summary>
        /// <param name="task"></param>
        internal void RegisterTick(Func<Task> func)
        {
            Tick += func;
        }

        /// <summary>
        /// Delete thread at runtime
        /// </summary>
        /// <param name="task"></param>
        internal void UnregisterTick(Func<Task> func)
        {
            Tick -= func;
        }
    }
}
