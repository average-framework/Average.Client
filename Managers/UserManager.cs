using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Interfaces;
using SDK.Shared.DataModels;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class UserManager : IUserManager
    {
        public UserData CurrentUser { get; private set; }

        public UserManager(Framework framework)
        {
            Task.Factory.StartNew(async () => 
            {
                await framework.IsReadyAsync();

                framework.Logger.Trace("Get user 0");
                framework.Rpc.Event("User.GetUser").On<UserData>((user) =>
                {
                    framework.Logger.Trace("Get user 1: " + user.Name);
                    CurrentUser = user;
                }).Emit();
            });
        }

        public async Task IsReady()
        {
            while (CurrentUser == null) await BaseScript.Delay(0);
        }
    }
}
