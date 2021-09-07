using System;
using System.Collections.Generic;
using Average.Client.Controllers;
using Average.Client.Managers;
using CitizenFX.Core;
using Newtonsoft.Json;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Rpc;
using SDK.Shared;
using static CitizenFX.Core.Native.API;

namespace Average.Client
{
    public abstract class InternalPlugin : IPlugin
    {
        public string Name { get; }
        
        public RpcRequest Rpc { get; set; }
        public ThreadManager Thread { get; set; }
        public CharacterManager Character { get; set; }
        public CommandManager Command { get; set; }
        public EventManager Event { get; set; }
        public ExportManager Export { get; set; }
        public PermissionManager Permission { get; set; }
        public SaveManager Save { get; set; }
        public SyncManager Sync { get; set; }
        public UserManager User { get; set; }
        public ObjectManager Streaming { get; set; }
        public NpcManager Npc { get; set; }
        public MenuManager Menu { get; set; }
        public NotificationManager Notification { get; set; }
        public LanguageManager Language { get; set; }
        public MapManager Map { get; set; }
        public BlipManager Blip { get; set; }
        public StorageManager Storage { get; set; }
        public CraftController Craft { get; set; }
        public DoorManager Door { get; set; }
        public PromptManager Prompt { get; set; }
        public RayMenuManager RayMenu { get; set; }
        public JobManager Job { get; set; }
        public EnterpriseManager Enterprise { get; set; }

        public InternalPlugin()
        {
            Name = GetType().Name;
        }
        
        public void SetDependencies(RpcRequest rpc, ThreadManager thread,
            CharacterManager character, CommandManager command, EventManager evnt, ExportManager export,
            PermissionManager permission, SaveManager save, SyncManager sync, UserManager user, ObjectManager streaming, NpcManager npc, MenuManager menu, NotificationManager notification, LanguageManager language, MapManager map, BlipManager blip, StorageManager storage, CraftController craft, DoorManager door, PromptManager prompt, RayMenuManager rayMenu, JobManager job, EnterpriseManager enterprise)
        {
            Rpc = rpc;
            Thread = thread;
            Character = character;
            Command = command;
            Event = evnt;
            Export = export;
            Permission = permission;
            Save = save;
            Sync = sync;
            User = user;
            Streaming = streaming;
            Npc = npc;
            Menu = menu;
            Notification = notification;
            Language = language;
            Map = map;
            Blip = blip;
            Storage = storage;
            Craft = craft;
            Door = door;
            Prompt = prompt;
            RayMenu = rayMenu;
            Job = job;
            Enterprise = enterprise;
        }
        
        public static async void SendNUI(object request)
        {
            await BaseScript.Delay(0);
            SendNuiMessage(JsonConvert.SerializeObject(request));
        }

        public void Focus(bool cursor = true)
        {
            SetNuiFocus(true, cursor);
        }

        public void Unfocus()
        {
            SetNuiFocus(false, false);
        }

        #region NUI

        [UICallback("ready")]
        private CallbackDelegate OnNuiReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (data["plugin"].ToString() == Name)
            {
                Log.Debug(Name + " nui is ready");
                OnNuiReady();
            }
            return result;
        }

        #endregion
        
        public virtual void OnInitialized()
        {
            
        }
        
        public virtual void OnNuiReady()
        {
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
