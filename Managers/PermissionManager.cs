using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Rpc;
using SDK.Shared.DataModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class PermissionManager : IPermissionManager
    {
        UserManager user;

        public List<PermissionData> Permissions { get; private set; }

        public PermissionManager(Logger logger, RpcRequest rpc, UserManager user)
        {
            this.user = user;

            logger.Debug("Getting permissions..");

            rpc.Event("Permission.GetAll").On<List<PermissionData>>(permissions =>
            {
                logger.Debug("Getted permissions");
                Permissions = permissions;
            }).Emit();
        }

        public async Task IsReady()
        {
            while (Permissions == null)await BaseScript.Delay(250);
        }

        public bool Exist(string name) => Permissions.Exists(x => x.Name == name);

        public bool Exist(int level) => Permissions.Exists(x => x.Level == level);

        public async Task<bool> HasPermission(string name)
        {
            await user.IsReady();

            if (Exist(name))
            {
                var permissionLevel = Permissions.Find(x => x.Name == user.CurrentUser.Permission.Name).Level;
                var needLevel = Permissions.Find(x => x.Name == name).Level;
                return permissionLevel >= needLevel;
            }

            return false;
        }

        public async Task<bool> HasPermission(string name, int level)
        {
            await user.IsReady();
            return Exist(name) && user.CurrentUser.Permission.Level >= level;
        }
    }
}
