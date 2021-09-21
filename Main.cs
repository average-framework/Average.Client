using Average.Client.Framework.IoC;
using Average.Client.Framework.Rpc;
using Average.Shared.Rpc;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;

namespace Average.Client
{
    internal class Main : BaseScript
    {
        internal readonly Action<Func<Task>> _attachCallback;
        internal readonly Action<Func<Task>> _detachCallback;

        private readonly Container _container;
        private readonly Bootstrapper _boostrap;

        public Main()
        {
            _attachCallback = c => Tick += c;
            _detachCallback = c => Tick -= c;

            _container = new Container();
            _boostrap = new Bootstrapper(this, _container, EventHandlers);
        }
    }
}
