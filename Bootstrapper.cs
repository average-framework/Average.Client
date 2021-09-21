using Average.Client.Framework.Handlers;
using Average.Client.Framework.IoC;
using Average.Client.Framework.Managers;
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

            // Managers
            _container.Register<EventManager>();
            _container.Register<CommandManager>();

            // Repositories

            // Services

            // Handlers
            _container.Register<CommandHandler>();
            _container.Register<ClientHandler>();

            // Reflections
            _container.Resolve<EventManager>().Reflect();
        }
    }
}