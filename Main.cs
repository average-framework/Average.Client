﻿using Average.Plugins;
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
    internal class Main : BaseScript
    {
        internal static EventHandlerDictionary Events { get; private set; }

        internal static Logger logger;
        internal static CommandManager commandManager;
        internal static Framework framework;
        internal static ThreadManager threadManager;

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

        internal static RpcRequest Event(string @event)
        {
            return new RpcRequest(@event, new RpcHandler(Events), new RpcTrigger(), new RpcSerializer());
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
