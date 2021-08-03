using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Interfaces;
using SDK.Shared.DataModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class PermissionManager : IPermissionManager
    {
        Framework framework;

        public List<PermissionData> Permissions { get; private set; }

        public PermissionManager(Framework framework)
        {
            this.framework = framework;

            Task.Factory.StartNew(async () =>
            {
                await framework.IsReadyAsync();
                framework.Rpc.Event("Permission.GetAll").On<List<PermissionData>>(permissions => Permissions = permissions).Emit();
            });
        }

        public async Task IsReady()
        {
            while (Permissions == null) await BaseScript.Delay(250);
        }

        public bool Exist(string name) => Permissions.Exists(x => x.Name == name);

        public bool Exist(int level) => Permissions.Exists(x => x.Level == level);

        public async Task<bool> HasPermission(string name)
        {
            await framework.User.IsReady();

            if (Exist(name))
            {
                var permissionLevel = Permissions.Find(x => x.Name == framework.User.CurrentUser.Permission.Name).Level;
                var needLevel = Permissions.Find(x => x.Name == name).Level;
                return permissionLevel >= needLevel;
            }

            return false;
        }

        public async Task<bool> HasPermission(string name, int level)
        {
            await framework.User.IsReady();
            return Exist(name) && framework.User.CurrentUser.Permission.Level >= level;
        }
    }
}
