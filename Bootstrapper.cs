using Average.Client.Framework.Handlers;
using Average.Client.Framework.IoC;
using Average.Client.Framework.Managers;
using Average.Client.Framework.Rpc;
using Average.Client.Framework.Services;
using Average.Client.Handlers;
using CitizenFX.Core;

namespace Average.Client
{
    internal class Bootstrapper
    {
        private readonly Main _main;
        private readonly Container _container;
        private readonly EventHandlerDictionary _eventHandlers;

        public Bootstrapper(Main main, Container container, EventHandlerDictionary eventHandlers)
        {
            _main = main;
            _container = container;
            _eventHandlers = eventHandlers;

            Register();
        }

        internal void Register()
        {
            // Others
            _container.RegisterInstance(_eventHandlers);
            _container.RegisterInstance(_main._attachCallback, serviceKey: "attachCallback");
            _container.RegisterInstance(_main._detachCallback, serviceKey: "detachCallback");

            _container.Register<RpcRequest>(reuse: Reuse.Transient);

            // Managers
            _container.Register<EventManager>();
            _container.Register<CommandManager>();

            // Services
            _container.Register<CharacterService>();

            // Handlers
            _container.Register<CommandHandler>();
            _container.Register<ClientHandler>();
            _container.Register<CharacterService>();

            // Reflections
            _container.Resolve<EventManager>().Reflect();

            // Called on resource start for initialize the client on server side
            _container.Resolve<ClientHandler>().OnClientInitialized();
        }
    }
}