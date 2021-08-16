using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Rpc;
using SDK.Shared.DataModels;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class UserManager : IUserManager
    {
        public UserData CurrentUser { get; private set; }

        public UserManager(Logger logger, RpcRequest rpc)
        {
            logger.Warn("Getting user..");

            rpc.Event("User.GetUser").On<UserData>((user) =>
            {
                logger.Warn("Getted user: " + user.Name);
                CurrentUser = user;
            }).Emit();
        }

        public async Task IsReady()
        {
            while (CurrentUser == null) await BaseScript.Delay(0);
        }
    }
}
