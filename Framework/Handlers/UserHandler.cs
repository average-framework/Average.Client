using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.DataModels;

namespace Average.Client.Framework.Handlers
{
    internal class UserHandler : IHandler
    {
        private readonly UserService _userService;

        public UserHandler(UserService userService)
        {
            _userService = userService;
        }

        [ClientEvent("User:Initialize")]
        private void OnClientInitialized(string userJson)
        {
            _userService.User = userJson.Deserialize<UserData>();
        }
    }
}
