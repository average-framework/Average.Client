using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Shared;
using SDK.Shared.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Average
{
    public class SyncManager : ISyncManager
    {
        Dictionary<string, SyncPropertyState> propertiesSyncs;
        Dictionary<string, SyncFieldState> fieldsSyncs;

        List<GetSyncPropertyState> networkedPropertiesGetSyncs;
        List<GetSyncFieldState> networkedFieldsGetSyncs;

        List<GetSyncPropertyState> propertiesGetSyncs;
        List<GetSyncFieldState> fieldsGetSyncs;

        EventHandlerDictionary eventHandlers;
        Logger logger;

        public SyncManager(EventHandlerDictionary eventHandlers, Logger logger)
        {
            this.eventHandlers = eventHandlers;
            this.logger = logger;

            propertiesSyncs = new Dictionary<string, SyncPropertyState>();
            fieldsSyncs = new Dictionary<string, SyncFieldState>();

            networkedPropertiesGetSyncs = new List<GetSyncPropertyState>();
            networkedFieldsGetSyncs = new List<GetSyncFieldState>();

            propertiesGetSyncs = new List<GetSyncPropertyState>();
            fieldsGetSyncs = new List<GetSyncFieldState>();

            eventHandlers["avg.internal.sync_property"] += new Action<string, object>(InternalNetworkSyncPropertyEvent);
            eventHandlers["avg.internal.sync_field"] += new Action<string, object>(InternalNetworkSyncFieldEvent);
        }

        internal object GetPropertyValue(PropertyInfo property, object classObj)
        {
            if (property.GetIndexParameters().Length == 0)
            {
                // Can get value
                return property.GetValue(classObj, null);
            }
            else
            {
                // Cannot get value
            }

            return null;
        }

        internal object GetFieldValue(FieldInfo field, object classObj)
        {
            return field.GetValue(classObj);
        }

        public void SyncProperties()
        {
            for (int i = 0; i < propertiesGetSyncs.Count; i++)
            {
                var getSync = propertiesGetSyncs[i];

                if (propertiesSyncs.ContainsKey(getSync.Attribute.Name))
                {
                    var sync = propertiesSyncs[getSync.Attribute.Name];

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
                        logger.Error($"Unable to sync properties from {sync.Attribute.Name}: {sync.Property.Name} with {getSync.Attribute.Name}: {getSync.Property.Name} because types is not the same [{string.Join(", ", sync.Property.PropertyType, getSync.Property.PropertyType)}]");
                    }
                }
            }
        }

        public void SyncFields()
        {
            for (int i = 0; i < fieldsGetSyncs.Count; i++)
            {
                var getSync = fieldsGetSyncs[i];

                if (fieldsSyncs.ContainsKey(getSync.Attribute.Name))
                {
                    var sync = fieldsSyncs[getSync.Attribute.Name];

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
                        logger.Error($"Unable to sync fields from {sync.Attribute.Name}: {sync.Field.Name} with {getSync.Attribute.Name}: {getSync.Field.Name} because types is not the same [{string.Join(", ", sync.Field.FieldType, getSync.Field.FieldType)}]");
                    }
                }
            }
        }

        public void RegisterSync(ref PropertyInfo property, SyncAttribute syncAttr, object classObj)
        {
            if (!propertiesSyncs.ContainsKey(syncAttr.Name))
            {
                if (property.CanWrite && property.CanRead)
                {
                    propertiesSyncs.Add(syncAttr.Name, new SyncPropertyState(syncAttr, property, classObj));
                    logger.Debug($"Registering [Sync] attribute: {syncAttr.Name} on property: {property.Name}");
                }
                else
                {
                    logger.Error($"Unable to register [Sync] attribute: {syncAttr.Name} on property: {property.Name}, [Sync] attribute can only be placed on getter & setter property.");
                }
            }
            else
            {
                logger.Error($"Unable to register [Sync] attribute: {syncAttr.Name} on property: {property.Name}, an [Sync] attribute have already been registered with this name.");
            }
        }

        public void RegisterSync(ref FieldInfo field, SyncAttribute syncAttr, object classObj)
        {
            if (!fieldsSyncs.ContainsKey(syncAttr.Name))
            {
                fieldsSyncs.Add(syncAttr.Name, new SyncFieldState(syncAttr, field, classObj));
                logger.Debug($"Registering [Sync] attribute: {syncAttr.Name} on field: {field.Name}");
            }
            else
            {
                logger.Error($"Unable to register [Sync] attribute: {syncAttr.Name} on field: {field.Name}, an [Sync] attribute have already been registered with this name.");
            }
        }

        public void RegisterGetSync(ref PropertyInfo property, GetSyncAttribute getSyncAttr, object classObj)
        {
            if (property.CanWrite && property.CanRead)
            {
                propertiesGetSyncs.Add(new GetSyncPropertyState(getSyncAttr, property, classObj));
                logger.Debug($"Registering [GetSync] attribute: {getSyncAttr.Name} on property: {property.Name}.");
            }
            else
            {
                logger.Error($"Unable to register [GetSync] attribute: {getSyncAttr.Name} on property: {property.Name}, [GetSync] attribute can only be placed on getter & setter property.");
            }
        }

        public void RegisterGetSync(ref FieldInfo field, GetSyncAttribute getSyncAttr, object classObj)
        {
            fieldsGetSyncs.Add(new GetSyncFieldState(getSyncAttr, field, classObj));
            logger.Debug($"Registering [GetSync]: {getSyncAttr.Name} on field: {field.Name}.");
        }

        #region Network

        public void RegisterNetworkGetSync(ref PropertyInfo property, NetworkGetSyncAttribute getSyncAttr, object classObj)
        {
            if (property.CanWrite && property.CanRead)
            {
                networkedPropertiesGetSyncs.Add(new GetSyncPropertyState(getSyncAttr, property, classObj));
                logger.Debug($"Registering [NetworkGetSync]: {getSyncAttr.Name} on property: {property.Name}.");
            }
            else
            {
                logger.Error($"Unable to register [NetworkGetSync]: {getSyncAttr.Name} on property: {property.Name}, [NetworkGetSync] attribute can only be placed on getter & setter property.");
            }
        }

        public void RegisterNetworkGetSync(ref FieldInfo field, NetworkGetSyncAttribute getSyncAttr, object classObj)
        {
            networkedFieldsGetSyncs.Add(new GetSyncFieldState(getSyncAttr, field, classObj));
            logger.Debug($"Registering [NetworkGetSync]: {getSyncAttr.Name} on field: {field.Name}.");
        }

        #endregion

        #region Internal Event

        void InternalNetworkSyncPropertyEvent(string attrName, object value)
        {
            var getSyncs = networkedPropertiesGetSyncs.Where(x => x.Attribute.Name == attrName).ToList();

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
                    logger.Error($"Unable to sync property from {attrName} with {getSync.Attribute.Name}: {getSync.Property.Name} because types is not the same [{string.Join(", ", newValue.GetType(), getSync.Property.PropertyType)}]");
                }
            }
        }

        void InternalNetworkSyncFieldEvent(string attrName, object value)
        {
            var getSyncs = networkedFieldsGetSyncs.Where(x => x.Attribute.Name == attrName).ToList();

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
                    logger.Error($"Unable to sync field from {attrName} with {getSync.Attribute.Name}: {getSync.Field.Name} because types is not the same [{string.Join(", ", newValue.GetType(), getSync.Field.FieldType)}]");
                }
            }
        }

        public IEnumerable<SyncPropertyState> GetAllSyncProperties()
        {
            return propertiesSyncs.Values.AsEnumerable();
        }

        public IEnumerable<SyncFieldState> GetAllSyncFields()
        {
            return fieldsSyncs.Values.AsEnumerable();
        }

        public IEnumerable<GetSyncPropertyState> GetAllNetworkedGetSyncProperties()
        {
            return networkedPropertiesGetSyncs.AsEnumerable();
        }

        public IEnumerable<GetSyncFieldState> GetAllNetworkedGetSyncFields()
        {
            return networkedFieldsGetSyncs.AsEnumerable();
        }

        public IEnumerable<GetSyncPropertyState> GetAllGetSyncProperties()
        {
            return propertiesGetSyncs.AsEnumerable();
        }

        public IEnumerable<GetSyncFieldState> GetAllGetSyncFields()
        {
            return fieldsGetSyncs.AsEnumerable();
        }

        #endregion
    }
}
