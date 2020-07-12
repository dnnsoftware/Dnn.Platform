// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Caching;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    public abstract class SettingsRepository<T> : ISettingsRepository<T> where T : class, new()
    {
        public const string CachePrefix = "ModuleSettingsPersister_";

        private readonly IModuleController _moduleController;

        protected SettingsRepository()
        {
            this.Mapping = this.LoadMapping();
            this._moduleController = ModuleController.Instance;
        }
        protected virtual string MappingCacheKey
        {
            get
            {
                var type = typeof(T);
                return CachePrefix + type.FullName.Replace(".", "_");
            }
        }

        private IList<ParameterMapping> Mapping { get; }

        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext), this.Load, false);
        }

        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);

            this.Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;

                if (property.CanRead) // Should be, because we asked for properties with a Get accessor.
                {
                    var settingValueAsString = SerializationController.SerializeProperty(settings, property, attribute.Serializer);

                    if (attribute is ModuleSettingAttribute)
                    {
                        this._moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute)
                    {
                        this._moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.TabModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is PortalSettingAttribute)
                    {
                        PortalController.UpdatePortalSetting(moduleContext.PortalID, mapping.FullParameterName, settingValueAsString);
                    }
                }
            });
            DataCache.SetCache(this.CacheKey(moduleContext.TabModuleID), settings);
        }

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

        private T Load(CacheItemArgs args)
        {
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var settings = new T();

            this.Mapping.ForEach(mapping =>
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
                    this.DeserializeProperty(settings, property, attribute, settingValue);
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
    }
}
