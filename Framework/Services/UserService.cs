using Average.Client.Framework.Interfaces;
using Average.Shared.DataModels;

namespace Average.Client.Framework.Services
{
    internal class UserService : IService
    {
        public UserData User { get; set; }

        public UserService()
        {

        }
    }
}
