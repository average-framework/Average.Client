using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Shared;
using SDK.Shared.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Average.Client.Managers
{
    public class SyncManager : ISyncManager
    {
        private Dictionary<string, SyncPropertyState> _propertiesSyncs = new Dictionary<string, SyncPropertyState>();
        private Dictionary<string, SyncFieldState> _fieldsSyncs = new Dictionary<string, SyncFieldState>();

        private List<GetSyncPropertyState> _networkedPropertiesGetSyncs = new List<GetSyncPropertyState>();
        private List<GetSyncFieldState> _networkedFieldsGetSyncs = new List<GetSyncFieldState>();

        private List<GetSyncPropertyState> _propertiesGetSyncs = new List<GetSyncPropertyState>();
        private List<GetSyncFieldState> _fieldsGetSyncs = new List<GetSyncFieldState>();

        private const int SyncRate = 60;

        public SyncManager()
        {
            #region Event

            Main.eventHandlers["avg.internal.sync_property"] += new Action<string, object>(InternalNetworkSyncPropertyEvent);
            Main.eventHandlers["avg.internal.sync_field"] += new Action<string, object>(InternalNetworkSyncFieldEvent);

            #endregion

            #region Thread

            Main.threadManager.StartThread(Update);

            #endregion
        }

        private async Task Update()
        {
            await BaseScript.Delay(SyncRate);

            SyncProperties();
            SyncProperties();
        }

        private object GetPropertyValue(PropertyInfo property, object classObj)
        {
            if (property.GetIndexParameters().Length == 0)
                return property.GetValue(classObj, null);

            return null;
        }

        private object GetFieldValue(FieldInfo field, object classObj) => field.GetValue(classObj);

        public void SyncProperties()
        {
            for (var i = 0; i < _propertiesGetSyncs.Count; i++)
            {
                var getSync = _propertiesGetSyncs[i];

                if (_propertiesSyncs.ContainsKey(getSync.Attribute.Name))
                {
                    var sync = _propertiesSyncs[getSync.Attribute.Name];

                    if (sync.Property.PropertyType == getSync.Property.PropertyType)
                    {
                        var syncValue = GetPropertyValue(sync.Property, sync.ClassObj);

                        if (sync.LastValue != syncValue)
                        {
                            sync.LastValue = syncValue;
                            getSync.Property.SetValue(getSync.ClassObj, syncValue, null);
                        }
                    }
                    else
                    {
                        Log.Error($"Unable to sync properties from {sync.Attribute.Name}: {sync.Property.Name} with {getSync.Attribute.Name}: {getSync.Property.Name} because types is not the same [{string.Join(", ", sync.Property.PropertyType, getSync.Property.PropertyType)}]");
                    }
                }
            }
        }

        public void SyncFields()
        {
            for (int i = 0; i < _fieldsGetSyncs.Count; i++)
            {
                var getSync = _fieldsGetSyncs[i];

                if (_fieldsSyncs.ContainsKey(getSync.Attribute.Name))
                {
                    var sync = _fieldsSyncs[getSync.Attribute.Name];

                    if (sync.Field.FieldType == getSync.Field.FieldType)
                    {
                        var syncValue = GetFieldValue(sync.Field, sync.ClassObj);

                        if (sync.LastValue != syncValue)
                        {
                            sync.LastValue = syncValue;
                            getSync.Field.SetValue(getSync.ClassObj, syncValue);
                        }
                    }
                    else
                    {
                        Log.Error($"Unable to sync fields from {sync.Attribute.Name}: {sync.Field.Name} with {getSync.Attribute.Name}: {getSync.Field.Name} because types is not the same [{string.Join(", ", sync.Field.FieldType, getSync.Field.FieldType)}]");
                    }
                }
            }
        }

        public void RegisterSync(ref PropertyInfo property, SyncAttribute syncAttr, object classObj)
        {
            if (!_propertiesSyncs.ContainsKey(syncAttr.Name))
            {
                if (property.CanWrite && property.CanRead)
                {
                    _propertiesSyncs.Add(syncAttr.Name, new SyncPropertyState(syncAttr, property, classObj));
                    Log.Debug($"Registering [Sync] attribute: {syncAttr.Name} on property: {property.Name}");
                }
                else
                {
                    Log.Error($"Unable to register [Sync] attribute: {syncAttr.Name} on property: {property.Name}, [Sync] attribute can only be placed on getter & setter property.");
                }
            }
            else
            {
                Log.Error($"Unable to register [Sync] attribute: {syncAttr.Name} on property: {property.Name}, an [Sync] attribute have already been registered with this name.");
            }
        }

        public void RegisterSync(ref FieldInfo field, SyncAttribute syncAttr, object classObj)
        {
            if (!_fieldsSyncs.ContainsKey(syncAttr.Name))
            {
                _fieldsSyncs.Add(syncAttr.Name, new SyncFieldState(syncAttr, field, classObj));
                Log.Debug($"Registering [Sync] attribute: {syncAttr.Name} on field: {field.Name}");
            }
            else
            {
                Log.Error($"Unable to register [Sync] attribute: {syncAttr.Name} on field: {field.Name}, an [Sync] attribute have already been registered with this name.");
            }
        }

        public void RegisterGetSync(ref PropertyInfo property, GetSyncAttribute getSyncAttr, object classObj)
        {
            if (property.CanWrite && property.CanRead)
            {
                _propertiesGetSyncs.Add(new GetSyncPropertyState(getSyncAttr, property, classObj));
                Log.Debug($"Registering [GetSync] attribute: {getSyncAttr.Name} on property: {property.Name}.");
            }
            else
            {
                Log.Error($"Unable to register [GetSync] attribute: {getSyncAttr.Name} on property: {property.Name}, [GetSync] attribute can only be placed on getter & setter property.");
            }
        }

        public void RegisterGetSync(ref FieldInfo field, GetSyncAttribute getSyncAttr, object classObj)
        {
            _fieldsGetSyncs.Add(new GetSyncFieldState(getSyncAttr, field, classObj));
            Log.Debug($"Registering [GetSync] attribute: {getSyncAttr.Name} on field: {field.Name}.");
        }

        #region Network

        public void RegisterNetworkGetSync(ref PropertyInfo property, NetworkGetSyncAttribute getSyncAttr, object classObj)
        {
            if (property.CanWrite && property.CanRead)
            {
                _networkedPropertiesGetSyncs.Add(new GetSyncPropertyState(getSyncAttr, property, classObj));
                Log.Debug($"Registering [NetworkGetSync] attribute: {getSyncAttr.Name} on property: {property.Name}.");
            }
            else
            {
                Log.Error($"Unable to register [NetworkGetSync] attribute: {getSyncAttr.Name} on property: {property.Name}, [NetworkGetSync] attribute can only be placed on getter & setter property.");
            }
        }

        public void RegisterNetworkGetSync(ref FieldInfo field, NetworkGetSyncAttribute getSyncAttr, object classObj)
        {
            _networkedFieldsGetSyncs.Add(new GetSyncFieldState(getSyncAttr, field, classObj));
            Log.Debug($"Registering [NetworkGetSync] attribute: {getSyncAttr.Name} on field: {field.Name}.");
        }

        #endregion

        #region Internal Event

        private void InternalNetworkSyncPropertyEvent(string attrName, object value)
        {
            var getSyncs = _networkedPropertiesGetSyncs.Where(x => x.Attribute.Name == attrName).ToList();

            for (int i = 0; i < getSyncs.Count; i++)
            {
                var getSync = getSyncs[i];
                var getSyncValue = GetPropertyValue(getSync.Property, getSync.ClassObj);

                // Need to add this for few number type, int32 can be converted to int16 and result to an convertion error without this line
                var newValue = Convert.ChangeType(value, getSync.Property.PropertyType);

                if (newValue.GetType() == getSync.Property.PropertyType)
                {
                    getSync.Property.SetValue(getSync.ClassObj, newValue, null);
                }
                else
                {
                    Log.Error($"Unable to sync property from {attrName} with {getSync.Attribute.Name}: {getSync.Property.Name} because types is not the same [{string.Join(", ", newValue.GetType(), getSync.Property.PropertyType)}]");
                }
            }
        }

        private void InternalNetworkSyncFieldEvent(string attrName, object value)
        {
            var getSyncs = _networkedFieldsGetSyncs.Where(x => x.Attribute.Name == attrName).ToList();

            for (int i = 0; i < getSyncs.Count; i++)
            {
                var getSync = getSyncs[i];
                var getSyncValue = GetFieldValue(getSync.Field, getSync.ClassObj);

                // Need to add this for few number type, int32 can be converted to int16 and detecte an convertion error without this line
                var newValue = Convert.ChangeType(value, getSync.Field.FieldType);

                if (newValue.GetType() == getSync.Field.FieldType)
                {
                    getSync.Field.SetValue(getSync.ClassObj, newValue);
                }
                else
                {
                    Log.Error($"Unable to sync field from {attrName} with {getSync.Attribute.Name}: {getSync.Field.Name} because types is not the same [{string.Join(", ", newValue.GetType(), getSync.Field.FieldType)}]");
                }
            }
        }

        public IEnumerable<SyncPropertyState> GetAllSyncProperties() => _propertiesSyncs.Values.AsEnumerable();

        public IEnumerable<SyncFieldState> GetAllSyncFields() => _fieldsSyncs.Values.AsEnumerable();

        public IEnumerable<GetSyncPropertyState> GetAllNetworkedGetSyncProperties() => _networkedPropertiesGetSyncs.AsEnumerable();

        public IEnumerable<GetSyncFieldState> GetAllNetworkedGetSyncFields() => _networkedFieldsGetSyncs.AsEnumerable();

        public IEnumerable<GetSyncPropertyState> GetAllGetSyncProperties() => _propertiesGetSyncs.AsEnumerable();

        public IEnumerable<GetSyncFieldState> GetAllGetSyncFields() => _fieldsGetSyncs.AsEnumerable();

        #endregion
    }
}
