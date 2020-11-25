// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Caching;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc/>
    public abstract class SettingsRepository<T> : ISettingsRepository<T> where T : class, new()
    {
        private readonly IModuleController moduleController;

        private static ISerializationManager SerializationManager =>
            Globals.DependencyProvider.GetRequiredService<ISerializationManager>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsRepository{T}"/> class.
        /// </summary>
        protected SettingsRepository()
        {
            this.Mapping = this.LoadMapping();
            this.moduleController = ModuleController.Instance;
        }

        /// <summary>
        /// Gets cache key for this class. Used for parameter mapping storage as well as entire class persistence.
        /// </summary>
        protected virtual string MappingCacheKey
        {
            get
            {
                var type = typeof(T);
                return "SettingsRepository_" + type.FullName.Replace(".", "_");
            }
        }

        private IList<ParameterMapping> Mapping { get; }

        /// <inheritdoc/>
        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext), this.Load, false);
        }

        /// <inheritdoc/>
        public T GetSettings(int portalId)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(portalId), 20, CacheItemPriority.AboveNormal, null, portalId), this.Load, false);
        }

        /// <inheritdoc/>
        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);
            this.SaveSettings(moduleContext.PortalID, moduleContext, settings);
        }

        /// <inheritdoc/>
        public void SaveSettings(int portalId, T settings)
        {
            Requires.NotNull("settings", settings);
            this.SaveSettings(portalId, null, settings);
        }

        /// <summary>
        /// Retrieves the parameter mapping from cache if still there, otherwise recreates it.
        /// </summary>
        /// <returns>List of parameters.</returns>
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

        /// <summary>
        /// Rebuilds parameter mapping of the class.
        /// </summary>
        /// <returns>List of parameters.</returns>
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

        private void SaveSettings(int portalId, ModuleInfo moduleContext, T settings)
        {
            this.Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;

                if (property.CanRead) // Should be, because we asked for properties with a Get accessor.
                {
                    var settingValueAsString = SerializationController.SerializeProperty(settings, property, attribute.Serializer);

                    if (attribute is ModuleSettingAttribute && moduleContext != null)
                    {
                        this.moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute && moduleContext != null)
                    {
                        this.moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.TabModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is PortalSettingAttribute psa && portalId != -1)
                    {
                        PortalController.UpdatePortalSetting(
                            portalId,
                            mapping.FullParameterName,
                            settingValueAsString,
                            clearCache: true,
                            cultureCode: Null.NullString,
                            psa.IsSecure);
                    }
                    else if (attribute is HostSettingAttribute)
                    {
                        HostController.Instance.Update(mapping.FullParameterName, settingValueAsString);
                    }
                }
            });
            DataCache.SetCache(this.CacheKey(moduleContext == null ? portalId : moduleContext.TabModuleID), settings);
        }

        private T Load(CacheItemArgs args)
        {
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var portalId = ctlModule == null ? (int)args.ParamList[1] : ctlModule.PortalID;
            var settings = new T();

            this.Mapping.ForEach(mapping =>
            {
                string settingValue = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;

                // TODO: Make more extensible, enable other attributes to be defined
                if (attribute is HostSettingAttribute && HostController.Instance.GetSettings().ContainsKey(mapping.FullParameterName))
                {
                    settingValue = HostController.Instance.GetSettings()[mapping.FullParameterName].Value;
                }
                else if (attribute is PortalSettingAttribute && portalId != -1 && PortalController.Instance.GetPortalSettings(portalId).ContainsKey(mapping.FullParameterName))
                {
                    var psa = (PortalSettingAttribute)attribute;
                    settingValue = PortalController.Instance.GetPortalSettings(portalId, string.Empty)[mapping.FullParameterName];
                    if (psa.IsSecure)
                    {
                        settingValue = Security.FIPSCompliant.DecryptAES(settingValue, Config.GetDecryptionkey(), Host.Host.GUID);
                    }
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule != null && ctlModule.TabModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.TabModuleSettings[mapping.FullParameterName];
                }
                else if (attribute is ModuleSettingAttribute && ctlModule != null && ctlModule.ModuleSettings.ContainsKey(mapping.FullParameterName))
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

        private string CacheKey(int id) => $"Settings{this.MappingCacheKey}_{id}";

        /// <summary>
        /// Deserializes the property.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="InvalidCastException">Thrown if string value cannot be deserialized to desired type.</exception>
        private void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, string propertyValue)
        {
            SerializationManager.DeserializeProperty(settings, property, propertyValue, attribute.Serializer);
        }
    }
}
