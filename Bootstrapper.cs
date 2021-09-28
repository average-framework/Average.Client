﻿using Average.Client.Framework.Handlers;
using Average.Client.Framework.IoC;
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

            // Framework Services
            _container.Register<RpcService>(reuse: Reuse.Transient);
            _container.Register<EventService>();
            _container.Register<ThreadService>();
            _container.Register<CommandService>();
            _container.Register<LanguageService>();
            _container.Register<ReplicateStateService>();
            _container.Register<UIService>();
            _container.Register<MenuService>();
            _container.Register<RayService>();

            // Services
            _container.Register<CharacterService>();
            _container.Register<CharacterCreatorService>();

            // Handlers
            _container.Register<RpcHandler>();
            _container.Register<UIHandler>();
            _container.Register<CommandHandler>();
            _container.Register<ClientHandler>();
            _container.Register<CharacterHandler>();
            _container.Register<CharacterCreatorHandler>();
            _container.Register<ReplicateStateHandler>();

            // Reflections
            _container.Resolve<EventService>().Reflect();
            _container.Resolve<ThreadService>().Reflect();
            _container.Resolve<ReplicateStateService>().Reflect();

            // Called on resource start for initialize the client on server side
            _container.Resolve<ClientHandler>().OnClientInitialized();
        }
    }
}