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
            _container.Register<ThreadManager>();

            // Services
            _container.Register<LanguageService>();
            _container.Register<UIService>();
            _container.Register<MenuService>();
            _container.Register<CharacterService>();
            _container.Register<CharacterCreatorService>();

            // Handlers
            _container.Register<UIHandler>();
            _container.Register<CommandHandler>();
            _container.Register<ClientHandler>();
            _container.Register<CharacterHandler>();
            _container.Register<CharacterCreatorHandler>();

            // Reflections
            _container.Resolve<EventManager>().Reflect();

            // Called on resource start for initialize the client on server side
            _container.Resolve<ClientHandler>().OnClientInitialized();
        }
    }
}