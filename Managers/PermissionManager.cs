using CitizenFX.Core;
using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Rpc;
using SDK.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Managers
{
    public class PermissionManager : IPermissionManager
    {
        Logger logger;
        UserManager user;

        public List<PermissionData> Permissions { get; private set; }

        public PermissionManager(Logger logger, RpcRequest rpc, UserManager user, EventHandlerDictionary eventHandler)
        {
            this.logger = logger;
            this.user = user;

            #region Event

            eventHandler["Permission.Set"] += new Action<string, int>(SetPermissionEvent);
            
            logger.Debug("Getting permissions..");

            rpc.Event("Permission.GetAll").On<List<PermissionData>>(permissions =>
            {
                logger.Debug("Getted permissions");
                Permissions = permissions;
            }).Emit();

            #endregion

            #region Command

            RegisterCommand("permission.set", new Action<int, List<object>, string>(SetPermissionCommand), false);

            #endregion
        }

        public async Task IsReady()
        {
            while (Permissions == null)await BaseScript.Delay(0);
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

        #region Command

        private async void SetPermissionCommand(int source, List<object> args, string raw)
        {
            if (await HasPermission("owner"))
            {
                var targetId = int.Parse(args[0].ToString());
                var permName = args[1].ToString();
                var permLevel = int.Parse(args[2].ToString());
                
                BaseScript.TriggerServerEvent("Permission.Set", GetPlayerServerId(targetId), permName, permLevel);
            }
        }

        #endregion

        #region Event

        private void SetPermissionEvent(string permissionName, int permissionLevel)
        {
            user.CurrentUser.Permission.Name = permissionName;
            user.CurrentUser.Permission.Level = permissionLevel;
            logger.Info($"New permission: [{permissionName}, {permissionLevel}]");
        }

        #endregion
    }
}
