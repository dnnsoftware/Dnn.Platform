// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Web.Caching;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public abstract class SettingsRepository<T> : ISettingsRepository<T>
        where T : class, new()
    {
        private readonly IModuleController moduleController;
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="SettingsRepository{T}"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        protected SettingsRepository()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SettingsRepository{T}"/> class.</summary>
        /// <param name="moduleController">The module controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        protected SettingsRepository(IModuleController moduleController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController)
        {
            this.Mapping = this.LoadMapping();
            this.moduleController = moduleController ?? ModuleController.Instance;
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        }

        /// <summary>Gets cache key for this class. Used for parameter mapping storage as well as entire class persistence.</summary>
        protected virtual string MappingCacheKey => "SettingsRepository_" + typeof(T).FullName.Replace(".", "_");

        private static ISerializationManager SerializationManager => Globals.GetCurrentServiceProvider().GetRequiredService<ISerializationManager>();

        private IList<ParameterMapping> Mapping { get; }

        /// <inheritdoc />
        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(
                this.hostSettings,
                new CacheItemArgs(this.CacheKey(moduleContext.PortalID, moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext),
                this.Load,
                false);
        }

        /// <inheritdoc />
        public T GetSettings(int portalId)
        {
            return CBO.GetCachedObject<T>(
                this.hostSettings,
                new CacheItemArgs(this.CacheKey(portalId, -1), 20, CacheItemPriority.AboveNormal, null, portalId),
                this.Load,
                false);
        }

        /// <inheritdoc />
        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);
            this.SaveSettings(moduleContext.PortalID, moduleContext, settings);
        }

        /// <inheritdoc />
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
        /// <param name="attribute">The attribute.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="InvalidCastException">Thrown if string value cannot be deserialized to desired type.</exception>
        private static void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, string propertyValue)
        {
            SerializationManager.DeserializeProperty(settings, property, propertyValue, attribute.Serializer);
        }

        private static string GetAlgorithmNameSettingKey(ParameterMapping mapping)
        {
            var settingKey = mapping.FullParameterName;
            return CryptographyUtils.GetAlgorithmNameSettingKey(settingKey);
        }

        private void SaveSettings(int portalId, ModuleInfo moduleContext, T settings)
        {
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
                            var hashAlgorithmName = HashAlgorithmName.SHA512;
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(hashAlgorithmName, settingValueAsString, Config.GetDecryptionkey(), this.hostSettings.Guid);
                            this.moduleController.UpdateModuleSetting(moduleContext.ModuleID, GetAlgorithmNameSettingKey(mapping), hashAlgorithmName.Name);
                            moduleContext.ModuleSettings[GetAlgorithmNameSettingKey(mapping)] = hashAlgorithmName.Name;
                        }

                        this.moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute tmsa && moduleContext != null)
                    {
                        if (tmsa.IsSecure)
                        {
                            var hashAlgorithmName = HashAlgorithmName.SHA512;
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(hashAlgorithmName, settingValueAsString, Config.GetDecryptionkey(), this.hostSettings.Guid);
                            this.moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, GetAlgorithmNameSettingKey(mapping), hashAlgorithmName.Name);
                            moduleContext.TabModuleSettings[GetAlgorithmNameSettingKey(mapping)] = hashAlgorithmName.Name;
                        }

                        this.moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.TabModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is PortalSettingAttribute psa && portalId != -1)
                    {
                        this.portalController.UpdatePortalSetting(portalId, mapping.FullParameterName, settingValueAsString, true, Null.NullString, psa.IsSecure);
                    }
                    else if (attribute is HostSettingAttribute hsa)
                    {
                        if (hsa.IsSecure)
                        {
                            var hashAlgorithmName = HashAlgorithmName.SHA512;
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(hashAlgorithmName, settingValueAsString, Config.GetDecryptionkey(), this.hostSettings.Guid);
                            this.hostSettingsService.Update(GetAlgorithmNameSettingKey(mapping), hashAlgorithmName.Name);
                        }

                        this.hostSettingsService.Update(mapping.FullParameterName, settingValueAsString);
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
            var hostSettingsDictionary = this.hostSettingsService.GetSettings();
            var portalSettingsDictionary = this.portalController.GetPortalSettings(portalId);

            this.Mapping.ForEach(mapping =>
            {
                string settingValue = null;
                string algorithmName = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;

                // TODO: Make more extensible, enable other attributes to be defined
                if (attribute is HostSettingAttribute && hostSettingsDictionary.TryGetValue(mapping.FullParameterName, out var hostSetting))
                {
                    settingValue = hostSetting.Value;
                    if (attribute.IsSecure && hostSettingsDictionary.TryGetValue(GetAlgorithmNameSettingKey(mapping), out var algorithmNameSetting))
                    {
                        algorithmName = algorithmNameSetting.Value;
                    }
                }
                else if (attribute is PortalSettingAttribute && portalId != -1 && portalSettingsDictionary.TryGetValue(mapping.FullParameterName, out settingValue))
                {
                    if (attribute.IsSecure && !portalSettingsDictionary.TryGetValue(GetAlgorithmNameSettingKey(mapping), out algorithmName))
                    {
                        algorithmName = null;
                    }
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule?.TabModuleSettings.ContainsKey(mapping.FullParameterName) == true)
                {
                    settingValue = (string)ctlModule.TabModuleSettings[mapping.FullParameterName];
                    if (attribute.IsSecure)
                    {
                        algorithmName = ctlModule.TabModuleSettings[GetAlgorithmNameSettingKey(mapping)] as string;
                    }
                }
                else if (attribute is ModuleSettingAttribute && ctlModule?.ModuleSettings.ContainsKey(mapping.FullParameterName) == true)
                {
                    settingValue = (string)ctlModule.ModuleSettings[mapping.FullParameterName];
                    if (attribute.IsSecure)
                    {
                        algorithmName = ctlModule.ModuleSettings[GetAlgorithmNameSettingKey(mapping)] as string;
                    }
                }

                if (attribute.IsSecure)
                {
                    var algorithm = string.IsNullOrWhiteSpace(algorithmName) ? HashAlgorithmName.SHA1 : new HashAlgorithmName(algorithmName);
                    try
                    {
                        settingValue = Security.FIPSCompliant.DecryptAES(algorithm, settingValue, Config.GetDecryptionkey(), this.hostSettings.Guid);
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(new ModuleLoadException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("ErrorDecryptingSetting", Localization.SharedResourceFile), mapping.FullParameterName), ex, ctlModule));
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
