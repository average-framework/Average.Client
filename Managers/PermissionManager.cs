using CitizenFX.Core;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDK.Client;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Managers
{
    public class PermissionManager : InternalPlugin, IPermissionManager
    {
        private List<PermissionData> _permissions;

        public override void OnInitialized()
        {
            #region Rpc

            Log.Debug("Getting permissions..");
            Main.rpc.Event("Permission.GetAll").On<List<PermissionData>>(permissions =>
            {
                _permissions = permissions;
                
                Log.Debug("Getted permissions");
            }).Emit();

            #endregion
        }

        public async Task IsReady()
        {
            while (_permissions == null)await BaseScript.Delay(0);
        }

        public bool Exist(string name) => _permissions.Exists(x => x.Name == name);

        public bool Exist(int level) => _permissions.Exists(x => x.Level == level);

        public async Task<bool> HasPermission(string name)
        {
            await User.IsReady();

            if (Exist(name))
            {
                var permissionLevel = _permissions.Find(x => x.Name == User.CurrentUser.Permission.Name).Level;
                var needLevel = _permissions.Find(x => x.Name == name).Level;
                return permissionLevel >= needLevel;
            }

            return false;
        }

        public async Task<bool> HasPermission(string name, int level)
        {
            await User.IsReady();

            if (Exist(name))
            {
                var permissionLevel = _permissions.Find(x => x.Name == User.CurrentUser.Permission.Name).Level;
                var needLevel = _permissions.Find(x => x.Name == name).Level;
                return permissionLevel >= needLevel && User.CurrentUser.Permission.Level >= level;
            }

            return false;
        }

        #region Command

        [ClientCommand("permission.set", "owner", 4)]
        private void SetPermissionCommand(int targetId, string permName, int permLevel)
        {
            BaseScript.TriggerServerEvent("Permission.Set", GetPlayerServerId(targetId), permName, permLevel);
        }

        #endregion

        #region Event

        [ClientEvent("Permission.Set")]
        private void SetPermissionEvent(string permissionName, int permissionLevel)
        {
            User.CurrentUser.Permission.Name = permissionName;
            User.CurrentUser.Permission.Level = permissionLevel;
            
            Log.Info($"New permission: [{permissionName}, {permissionLevel}]");
        }

        #endregion
    }
}
