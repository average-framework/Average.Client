using Average.Client.Framework.IoC;
using Average.Client.Framework.Services;
using CitizenFX.Core;

namespace Average.Client
{
    internal class Bootstrapper
    {
        private readonly Main _main;
        private readonly Container _container;
        private readonly EventHandlerDictionary _eventHandlers;

        //internal static JObject BaseConfig = null;

        public Bootstrapper(Main main, Container container, EventHandlerDictionary eventHandlers)
        {
            _main = main;
            _container = container;
            _eventHandlers = eventHandlers;

            //BaseConfig = FileUtility.ReadFileFromRootDir("config.json").ToJObject();

            Register();
        }

        internal void Register()
        {
            // Others
            _container.RegisterInstance(_eventHandlers);
            _container.RegisterInstance(_main._attachCallback, serviceKey: "attachCallback");
            _container.RegisterInstance(_main._detachCallback, serviceKey: "detachCallback");

            // Managers

            // Repositories

            // Services
            _container.Register<TestService>();

            // Handlers

            // Reflections
        }
    }
}