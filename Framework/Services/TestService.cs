using Average.Client.Framework.Interfaces;
using CitizenFX.Core;

namespace Average.Client.Framework.Services
{
    internal class TestService : IService
    {
        public TestService(EventHandlerDictionary _events)
        {
            Debug.WriteLine($"^6TestService successfully initialized. [{(_events == null)}]");
        }
    }
}
