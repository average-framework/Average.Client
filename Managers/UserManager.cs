using CitizenFX.Core;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared.DataModels;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class UserManager : IUserManager
    {
        public UserData CurrentUser { get; private set; }

        public UserManager()
        {
            Log.Warn("Getting user..");

            Main.rpc.Event("User.GetUser").On<UserData>((user) =>
            {
                Log.Warn("Getted user");
                CurrentUser = user;
            }).Emit();
        }

        public async Task IsReady()
        {
            while (CurrentUser == null) await BaseScript.Delay(0);
        }
    }
}
