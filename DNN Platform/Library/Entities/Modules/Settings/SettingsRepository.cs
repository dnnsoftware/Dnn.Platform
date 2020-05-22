// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Caching;

namespace DotNetNuke.Entities.Modules.Settings
{
    public abstract class SettingsRepository<T> : ISettingsRepository<T> where T : class, new()
    {
        #region Properties

        private IList<ParameterMapping> Mapping { get; }

        private readonly IModuleController _moduleController;

        #endregion

        protected SettingsRepository()
        {
            Mapping = LoadMapping();
            _moduleController = ModuleController.Instance;
        }

        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(CacheKey(moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext), Load, false);
        }

        #region Serialization
        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);

            Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;

                if (property.CanRead) // Should be, because we asked for properties with a Get accessor.
                {
                    var settingValueAsString = SerializationController.SerializeProperty(settings, property, attribute.Serializer);

                    if (attribute is ModuleSettingAttribute)
                    {
                        _moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute)
                    {
                        _moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.TabModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is PortalSettingAttribute)
                    {
                        PortalController.UpdatePortalSetting(moduleContext.PortalID, mapping.FullParameterName, settingValueAsString);
                    }
                }
            });
            DataCache.SetCache(CacheKey(moduleContext.TabModuleID), settings);
        }
        #endregion

        #region Mappings
        protected IList<ParameterMapping> LoadMapping()
        {
            var cacheKey = this.MappingCacheKey;
            var mapping = CachingProvider.Instance().GetItem(cacheKey) as IList<ParameterMapping>;
            if (mapping == null)
            {
                mapping = this.CreateMapping();
                // HARDCODED: 2 hour expiration. 
                // Note that "caching" can also be accomplished with a static dictionary since the Attribute/Property mapping does not change unless the module is updated.
                CachingProvider.Instance().Insert(cacheKey, mapping, (DNNCacheDependency)null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
            }

            return mapping;
        }

        public const string CachePrefix = "ModuleSettingsPersister_";
        protected virtual string MappingCacheKey
        {
            get
            {
                var type = typeof(T);
                return CachePrefix + type.FullName.Replace(".", "_");
            }
        }

        protected virtual IList<ParameterMapping> CreateMapping()
        {
            var mapping = new List<ParameterMapping>();
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

            properties.ForEach(property =>
            {
                var attributes = property.GetCustomAttributes<ParameterAttributeBase>(true);
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping(attribute, property)));
            });

            return mapping;
        }
        #endregion

        #region Deserialization
        private T Load(CacheItemArgs args)
        {
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var settings = new T();

            Mapping.ForEach(mapping =>
            {
                string settingValue = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;

                // TODO: Make more extensible, enable other attributes to be defined
                if (attribute is PortalSettingAttribute && PortalController.Instance.GetPortalSettings(ctlModule.PortalID).ContainsKey(mapping.FullParameterName))
                {
                    settingValue = PortalController.Instance.GetPortalSettings(ctlModule.PortalID)[mapping.FullParameterName];
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule.TabModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.TabModuleSettings[mapping.FullParameterName];
                }
                else if (attribute is ModuleSettingAttribute && ctlModule.ModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.ModuleSettings[mapping.FullParameterName];
                }

                if (settingValue != null && property.CanWrite)
                {
                    DeserializeProperty(settings, property, attribute, settingValue);
                }
            });

            return settings;
        }

        private string CacheKey(int moduleId)
        {
            return string.Format("SettingsModule{0}", moduleId);
        }

        /// <summary>
        /// Deserializes the property.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="System.InvalidCastException"></exception>
        private void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, string propertyValue)
        {
            SerializationController.DeserializeProperty(settings, property, propertyValue, attribute.Serializer);
        }
        #endregion

    }
}
