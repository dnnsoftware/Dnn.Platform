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
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc/>
    public abstract class SettingsRepository<T> : ISettingsRepository<T>
        where T : class, new()
    {
        private readonly IModuleController moduleController;

        /// <summary>Initializes a new instance of the <see cref="SettingsRepository{T}"/> class.</summary>
        protected SettingsRepository()
        {
            this.Mapping = this.LoadMapping();
            this.moduleController = ModuleController.Instance;
        }

        /// <summary>Gets cache key for this class. Used for parameter mapping storage as well as entire class persistence.</summary>
        protected virtual string MappingCacheKey => "SettingsRepository_" + typeof(T).FullName.Replace(".", "_");

        private static ISerializationManager SerializationManager => Globals.GetCurrentServiceProvider().GetRequiredService<ISerializationManager>();

        private IList<ParameterMapping> Mapping { get; }

        /// <inheritdoc/>
        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(moduleContext.PortalID, moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext), this.Load, false);
        }

        /// <inheritdoc/>
        public T GetSettings(int portalId)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(portalId, -1), 20, CacheItemPriority.AboveNormal, null, portalId), this.Load, false);
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

        /// <summary>Retrieves the parameter mapping from cache if still there, otherwise recreates it.</summary>
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

        /// <summary>Rebuilds parameter mapping of the class.</summary>
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

        /// <summary>Deserializes the property.</summary>
        /// <param name="settings">The settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="InvalidCastException">Thrown if string value cannot be deserialized to desired type.</exception>
        private static void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, string propertyValue)
        {
            SerializationManager.DeserializeProperty(settings, property, propertyValue, attribute.Serializer);
        }

        private void SaveSettings(int portalId, ModuleInfo moduleContext, T settings)
        {
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<Abstractions.Application.IHostSettingsService>();

            this.Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;

                // Should be, because we asked for properties with a Get accessor.
                if (property.CanRead)
                {
                    var settingValueAsString = SerializationManager.SerializeProperty(settings, property, attribute.Serializer);

                    if (attribute is ModuleSettingAttribute msa && moduleContext != null)
                    {
                        if (msa.IsSecure)
                        {
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(settingValueAsString, Config.GetDecryptionkey(), Host.Host.GUID);
                        }

                        this.moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute tmsa && moduleContext != null)
                    {
                        if (tmsa.IsSecure)
                        {
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(settingValueAsString, Config.GetDecryptionkey(), Host.Host.GUID);
                        }

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
                            isSecure: psa.IsSecure);
                    }
                    else if (attribute is HostSettingAttribute hsa)
                    {
                        if (hsa.IsSecure)
                        {
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(settingValueAsString, Config.GetDecryptionkey(), Host.Host.GUID);
                        }

                        hostSettingsService.Update(mapping.FullParameterName, settingValueAsString);
                    }
                }
            });

            DataCache.ClearCache(this.CacheKeyPortalPrefix(portalId));
            DataCache.SetCache(this.CacheKey(portalId, moduleContext?.TabModuleID ?? -1), settings);
        }

        private T Load(CacheItemArgs args)
        {
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var portalId = ctlModule?.PortalID ?? (int)args.ParamList[1];
            var settings = new T();
            var hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<Abstractions.Application.IHostSettingsService>().GetSettings();

            this.Mapping.ForEach(mapping =>
            {
                string settingValue = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;

                // TODO: Make more extensible, enable other attributes to be defined
                if (attribute is HostSettingAttribute hsa && hostSettings.TryGetValue(mapping.FullParameterName, out var hostSetting))
                {
                    settingValue = hostSetting.Value;
                }
                else if (attribute is PortalSettingAttribute && portalId != -1 && PortalController.Instance.GetPortalSettings(portalId).ContainsKey(mapping.FullParameterName))
                {
                    settingValue = PortalController.Instance.GetPortalSettings(portalId)[mapping.FullParameterName];
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule != null && ctlModule.TabModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.TabModuleSettings[mapping.FullParameterName];
                }
                else if (attribute is ModuleSettingAttribute && ctlModule != null && ctlModule.ModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.ModuleSettings[mapping.FullParameterName];
                }

                if (attribute.IsSecure)
                {
                    try
                    {
                        settingValue = Security.FIPSCompliant.DecryptAES(settingValue, Config.GetDecryptionkey(), Host.Host.GUID);
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(new ModuleLoadException(string.Format(Localization.GetString("ErrorDecryptingSetting", Localization.SharedResourceFile), mapping.FullParameterName), ex, ctlModule));
                    }
                }

                if (settingValue != null && property.CanWrite)
                {
                    DeserializeProperty(settings, property, attribute, settingValue);
                }
            });

            return settings;
        }

        /// <summary>Gets the cache key for the given portal and tab module.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabModuleId">The tab module ID.</param>
        /// <remarks>When <paramref name="tabModuleId"/> is -1, the cache key is for portal settings instead.</remarks>
        /// <returns>The cache key.</returns>
        private string CacheKey(int portalId, int tabModuleId) => $"{this.CacheKeyPortalPrefix(portalId)}{tabModuleId}";

        /// <summary>Gets the prefix of the cache key for the given portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The cache key prefix.</returns>
        private string CacheKeyPortalPrefix(int portalId) => $"Settings{this.MappingCacheKey}_{portalId}_";
    }
}
