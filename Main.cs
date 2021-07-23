using Average.Client.Plugins;
using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Commands;
using SDK.Client.Diagnostics;
using SDK.Client.Rpc;
using SDK.Shared.Rpc;

namespace Average.Client
{
    public class Main : BaseScript
    {
        public static EventHandlerDictionary Events { get; private set; }

        public static Logger logger;
        public static CommandManager commandManager;
        public static Framework framework;

        PluginLoader plugin;

        public Main()
        {
            Events = EventHandlers;

            logger = new Logger();
            commandManager = new CommandManager(logger);
            framework = new Framework(EventHandlers, logger, commandManager);
            plugin = new PluginLoader(commandManager);

            plugin.Load();
        }

        public static RpcRequest Event(string @event)
        {
            return new RpcRequest(@event, new RpcHandler(Events), new RpcTrigger(), new RpcSerializer());
        }
    }
}
