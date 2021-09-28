using Average.Client.Framework.IoC;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;

namespace Average.Client
{
    internal class Main : BaseScript
    {
        private readonly Action<Func<Task>> _addTick;
        private readonly Action<Func<Task>> _removeTick;

        private static Main _instance;

        public Main()
        {
            _instance = this;

            _addTick = task => Tick += task;
            _removeTick = task => Tick -= task;

            var container = new Container();
            var boostrap = new Bootstrapper(container, EventHandlers);
        }

        internal static void AddTick(Func<Task> func) => _instance._addTick(func);
        internal static void RemoveTick(Func<Task> func) => _instance._removeTick(func);
    }
}
