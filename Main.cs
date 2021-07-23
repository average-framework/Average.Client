using Average.Plugins;
using Average.Threading;
using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Commands;
using SDK.Client.Diagnostics;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;
using System;
using System.Threading.Tasks;

namespace Average
{
    public class Main : BaseScript
    {
        public static EventHandlerDictionary Events { get; private set; }

        public static Logger logger;
        public static CommandManager commandManager;
        public static Framework framework;
        public static ThreadManager threadManager;

        PluginLoader plugin;

        public Main()
        {
            Events = EventHandlers;

            logger = new Logger();
            commandManager = new CommandManager(logger);
            threadManager = new ThreadManager(this);
            framework = new Framework(EventHandlers, threadManager, logger, commandManager);
            plugin = new PluginLoader(commandManager);

            plugin.Load();
        }

        public static RpcRequest Event(string @event)
        {
            return new RpcRequest(@event, new RpcHandler(Events), new RpcTrigger(), new RpcSerializer());
        }

        /// <summary>
        /// Create new thread at runtime
        /// </summary>
        /// <param name="task"></param>
        public void RegisterTick(Func<Task> func)
        {
            Tick += func;
        }

        /// <summary>
        /// Delete thread at runtime
        /// </summary>
        /// <param name="task"></param>
        public void UnregisterTick(Func<Task> func)
        {
            Tick -= func;
        }
    }
}
