// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Settings related to managing client resources.</summary>
    public partial class ClientResourceSettings
    {
        // public keys used to identify the dictionaries stored in the application context

        /// <summary>The host settings dictionary key.</summary>
        public static readonly string HostSettingsDictionaryKey = "HostSettingsDictionary";

        /// <summary>The portal settings dictionary key.</summary>
        public static readonly string PortalSettingsDictionaryKey = "PortalSettingsDictionary";

        // public keys used to identify the various host and portal level settings

        /// <summary>The composite files key.</summary>
        [Obsolete("Composite Files (bundling) have been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
        public static readonly string EnableCompositeFilesKey = "CrmEnableCompositeFiles";

        /// <summary>The minify CSS key.</summary>
        [Obsolete("Minification has been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
        public static readonly string MinifyCssKey = "CrmMinifyCss";

        /// <summary>The minify JS key.</summary>
        [Obsolete("Minification has been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
        public static readonly string MinifyJsKey = "CrmMinifyJs";

        /// <summary>The override default settings key.</summary>
        public static readonly string OverrideDefaultSettingsKey = "CrmOverrideDefaultSettings";

        /// <summary>The version key.</summary>
        public static readonly string VersionKey = "CrmVersion";

        private static readonly Type PortalControllerType;

        private bool statusChecked;
        private UpgradeStatus status;

        static ClientResourceSettings()
        {
            try
            {
                PortalControllerType = Type.GetType("DotNetNuke.Entities.Portals.PortalController, DotNetNuke");
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to get get types for reflection", exception);
            }
        }

        private UpgradeStatus Status
        {
            get
            {
                if (!this.statusChecked)
                {
                    this.status = GetStatusByReflection();
                    this.statusChecked = true;
                }

                return this.status;
            }
        }

        /// <inheritdoc cref="IsOverridingDefaultSettingsEnabled(int?)"/>
        [DnnDeprecated(9, 10, 3, "Use overload taking portalId")]
        public partial bool IsOverridingDefaultSettingsEnabled()
        {
            int? portalId = GetPortalIdThroughReflection();
            return this.IsOverridingDefaultSettingsEnabled(portalId);
        }

        /// <summary>Gets a value indicating whether overriding the default settings is enabled.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if it's enabled, otherwise <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool IsOverridingDefaultSettingsEnabled(int? portalId)
        {
            var portalVersion = GetIntegerSetting(portalId, PortalSettingsDictionaryKey, VersionKey);
            var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            return portalVersion.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value;
        }

        /// <inheritdoc cref="GetVersion(int?)"/>
        [DnnDeprecated(9, 10, 3, "Use overload taking portalId")]
        public partial int? GetVersion()
        {
            int? portalId = GetPortalIdThroughReflection();
            return this.GetVersion(portalId);
        }

        /// <summary>Gets the version.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The version or <see langword="null"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int? GetVersion(int? portalId)
        {
            var portalVersion = GetIntegerSetting(portalId, PortalSettingsDictionaryKey, VersionKey);
            var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            if (portalVersion.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value)
            {
                return portalVersion.Value;
            }

            // otherwise return the host setting
            var hostVersion = GetIntegerSetting(portalId, HostSettingsDictionaryKey, VersionKey);
            if (hostVersion.HasValue)
            {
                return hostVersion.Value;
            }

            // otherwise tell the calling method that nothing is set
            return null;
        }

        /// <summary>Gets a value indicating whether composite files are enabled.</summary>
        /// <returns>Whether it's enabled.</returns>
        [DnnDeprecated(10, 2, 0, "Composite files (bundling) have been deprecated.")]
        public partial bool? AreCompositeFilesEnabled()
        {
            int? portalId = GetPortalIdThroughReflection();
#pragma warning disable CS0618 // Type or member is obsolete
            return this.IsBooleanSettingEnabled(portalId, EnableCompositeFilesKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>Gets a value indicating whether CSS minification is enabled.</summary>
        /// <returns>Whether it's enabled.</returns>
        [DnnDeprecated(10, 2, 0, "Minification has been deprecated.")]
        public partial bool? EnableCssMinification()
        {
            int? portalId = GetPortalIdThroughReflection();
#pragma warning disable CS0618 // Type or member is obsolete
            return this.IsBooleanSettingEnabled(portalId, MinifyCssKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>Gets a value indicating whether JS minification is enabled.</summary>
        /// <returns>Whether it's enabled.</returns>
        [DnnDeprecated(10, 2, 0, "Minification has been deprecated.")]
        public partial bool? EnableJsMinification()
        {
            int? portalId = GetPortalIdThroughReflection();
#pragma warning disable CS0618 // Type or member is obsolete
            return this.IsBooleanSettingEnabled(portalId, MinifyJsKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool? GetBooleanSetting(int? portalId, string dictionaryKey, string settingKey)
        {
            var setting = GetSetting(portalId, dictionaryKey, settingKey);
            bool result;
            if (setting != null && bool.TryParse(setting, out result))
            {
                return result;
            }

            return null;
        }

        private static int? GetIntegerSetting(int? portalId, string dictionaryKey, string settingKey)
        {
            var setting = GetSetting(portalId, dictionaryKey, settingKey);
            int version;
            if (setting != null && int.TryParse(setting, out version))
            {
                if (version > -1)
                {
                    return version;
                }
            }

            return null;
        }

        private static string GetSetting(int? portalId, string dictionaryKey, string settingKey)
        {
            bool isHttpContext = HttpContext.Current != null && HttpContext.Current.Items.Contains(dictionaryKey);
            var settings = isHttpContext ? HttpContext.Current.Items[dictionaryKey] : null;
            if (settings == null)
            {
                if (dictionaryKey == HostSettingsDictionaryKey)
                {
                    return GetHostSettingThroughReflection(settingKey);
                }

                return GetPortalSettingThroughReflection(portalId, settingKey);
            }

            string value;
            var dictionary = (Dictionary<string, string>)settings;
            if (dictionary.TryGetValue(settingKey, out value))
            {
                return value;
            }

            // no valid setting was found
            return null;
        }

        private static string GetPortalSettingThroughReflection(int? portalId, string settingKey)
        {
            if (portalId is null)
            {
                return null;
            }

            try
            {
                using var scope = DependencyInjection.GetOrCreateServiceScope();
                var portalController = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, PortalControllerType);
                var method = PortalControllerType.GetMethod("GetPortalSettings", BindingFlags.Public | BindingFlags.Instance, null, [typeof(int),], null);
                var dictionary = (Dictionary<string, string>)method.Invoke(portalController, [portalId.Value,]);

                if (dictionary.TryGetValue(settingKey, out var value))
                {
                    return value;
                }
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Portal Setting Through Reflection", exception);
            }

            return null;
        }

        private static int? GetPortalIdThroughReflection()
        {
            try
            {
                if (HttpContext.Current == null)
                {
                    return null;
                }

                using var scope = DependencyInjection.GetOrCreateServiceScope();
                var portalAliasService = scope.ServiceProvider.GetRequiredService<IPortalAliasService>();
                var alias = portalAliasService.GetPortalAlias(HttpContext.Current.Request.Url.Host);

                return alias.PortalId;
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Portal ID Through Reflection", exception);
            }

            return null;
        }

        private static string GetHostSettingThroughReflection(string settingKey)
        {
            try
            {
                using var scope = DependencyInjection.GetOrCreateServiceScope();
                var hostSettingsService = scope.ServiceProvider.GetRequiredService<IHostSettingsService>();

                var dictionary = hostSettingsService.GetSettingsDictionary();
                if (dictionary.TryGetValue(settingKey, out var value))
                {
                    return value;
                }
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Host Setting Through Reflection", exception);
            }

            return null;
        }

        private static UpgradeStatus GetStatusByReflection()
        {
            try
            {
                using var scope = DependencyInjection.GetOrCreateServiceScope();
                var applicationStatusInfo = scope.ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
                return applicationStatusInfo.Status;
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Status By Reflection", exception);
                return UpgradeStatus.Unknown;
            }
        }

        private bool? IsBooleanSettingEnabled(int? portalId, string settingKey)
        {
            if (this.Status != UpgradeStatus.None)
            {
                return false;
            }

            var portalEnabled = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, settingKey);
            var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            if (portalEnabled.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value)
            {
                return portalEnabled.Value;
            }

            // otherwise return the host setting
            var hostEnabled = GetBooleanSetting(portalId, HostSettingsDictionaryKey, settingKey);
            if (hostEnabled.HasValue)
            {
                return hostEnabled.Value;
            }

            // otherwise tell the calling method that nothing is set
            return null;
        }
    }
}
