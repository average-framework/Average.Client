using Average.Client.Commands;
using Average.Client.Framework.Handlers;
using Average.Client.Framework.IoC;
using Average.Client.Framework.Services;
using Average.Client.Scripts;
using Average.Client.Scripts.Commands;
using CitizenFX.Core;

namespace Average.Client
{
    internal class Bootstrapper
    {
        private readonly Container _container;
        private readonly EventHandlerDictionary _eventHandlers;

        public Bootstrapper(Container container, EventHandlerDictionary eventHandlers)
        {
            _container = container;
            _eventHandlers = eventHandlers;

            Register();
        }

        internal void Register()
        {
            // Others
            _container.RegisterInstance(_eventHandlers);

            // Framework Services
            //_container.Register<RpcService>(reuse: Reuse.Transient);
            _container.Register<EventService>();
            _container.Register<RpcService>();
            _container.Register<ThreadService>();
            _container.Register<UserService>();
            _container.Register<CommandService>();
            _container.Register<LanguageService>();
            _container.Register<UIService>();
            _container.Register<NotificationService>();
            _container.Register<MenuService>();
            _container.Register<ClientService>();
            _container.Register<InputService>();
            _container.Register<RayService>();
            _container.Register<WorldService>();
            _container.Register<DoorService>();
            _container.Register<MapService>();
            _container.Register<GameService>();
            _container.Register<InventoryService>();
            _container.Register<InventoryItemsService>();
            _container.Register<ObjectStreamingService>();

            // Services
            _container.Register<CharacterService>();
            _container.Register<CharacterCreatorService>();

            // Handlers
            _container.Register<RpcHandler>();
            _container.Register<UIHandler>();
            _container.Register<ClientHandler>();
            _container.Register<CharacterHandler>();
            _container.Register<CharacterCreatorHandler>();
            _container.Register<RayHandler>();
            _container.Register<WorldHandler>();
            _container.Register<DoorHandler>();
            _container.Register<InventoryHandler>();
            _container.Register<MenuHandler>();
            _container.Register<UserHandler>();
            _container.Register<ObjectStreamingHandler>();
            _container.Register<NotificationHandler>();

            // Scripts
            _container.Register<DebugScript>();

            // Commands
            _container.Register<WorldCommand>();
            _container.Register<InventoryCommand>();
            _container.Register<DebugCommand>();
            _container.Register<ObjectStreamingCommand>();
            _container.Register<MapCommand>();

            // Reflections
            _container.Resolve<EventService>().Reflect();
            _container.Resolve<UIService>().Reflect();
            _container.Resolve<ThreadService>().Reflect();
            _container.Resolve<CommandService>().Reflect();

            // Called on resource start for initialize the client on server side
            _container.Resolve<ClientHandler>().OnClientInitialized();
        }
    }
}