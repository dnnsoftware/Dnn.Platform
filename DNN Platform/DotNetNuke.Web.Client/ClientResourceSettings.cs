// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

using DotNetNuke.Internal.SourceGenerators;

// note: this class is duplicated in ClientDependency.Core.Config.DnnConfiguration, any updates need to be synced between the two.
public partial class ClientResourceSettings
{
    // public keys used to identify the dictionaries stored in the application context
    public static readonly string HostSettingsDictionaryKey = "HostSettingsDictionary";
    public static readonly string PortalSettingsDictionaryKey = "PortalSettingsDictionary";

    // public keys used to identify the various host and portal level settings
    public static readonly string EnableCompositeFilesKey = "CrmEnableCompositeFiles";
    public static readonly string MinifyCssKey = "CrmMinifyCss";
    public static readonly string MinifyJsKey = "CrmMinifyJs";
    public static readonly string OverrideDefaultSettingsKey = "CrmUseApplicationSettings";
    public static readonly string VersionKey = "CrmVersion";

    private static readonly Type PortalControllerType;
    private static readonly Type PortalAliasControllerType;
    private static readonly Type HostControllerType;
    private static readonly Type CommonGlobalsType;

    private bool statusChecked;
    private UpgradeStatus status;

    static ClientResourceSettings()
    {
        try
        {
            // all these types are part of the same library, so we don't need a separate catch for each one
            CommonGlobalsType = Type.GetType("DotNetNuke.Common.Globals, DotNetNuke");
            PortalControllerType = Type.GetType("DotNetNuke.Entities.Portals.PortalController, DotNetNuke");
            PortalAliasControllerType = Type.GetType("DotNetNuke.Entities.Portals.PortalAliasController, DotNetNuke");
            HostControllerType = Type.GetType("DotNetNuke.Entities.Controllers.HostController, DotNetNuke");
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private enum UpgradeStatus
    {
        /// <summary>The application need update to a higher version.</summary>
        Upgrade = 0,

        /// <summary>The application need to install itself.</summary>
        Install = 1,

        /// <summary>The application is normal running.</summary>
        None = 2,

        /// <summary>The application occur error when running.</summary>
        Error = 3,

        /// <summary>The application status is unknown.</summary>
        /// <remarks>This status should never be returned. its is only used as a flag that Status hasn't been determined.</remarks>
        Unknown = 4,
    }

    private UpgradeStatus Status
    {
        get
        {
            if (!this.statusChecked)
            {
                this.status = this.GetStatusByReflection();
                this.statusChecked = true;
            }

            return this.status;
        }
    }

    [DnnDeprecated(9, 10, 3, "Use overload taking portalId")]
    public partial bool IsOverridingDefaultSettingsEnabled()
    {
        int? portalId = GetPortalIdThroughReflection();
        return this.IsOverridingDefaultSettingsEnabled(portalId);
    }

    public bool IsOverridingDefaultSettingsEnabled(int? portalId)
    {
        var portalVersion = GetIntegerSetting(portalId, PortalSettingsDictionaryKey, VersionKey);
        var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

        // if portal version is set
        // and the portal "override default settings" flag is set and set to true
        return portalVersion.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value;
    }

    [DnnDeprecated(9, 10, 3, "Use overload taking portalId")]
    public partial int? GetVersion()
    {
        int? portalId = GetPortalIdThroughReflection();
        return this.GetVersion(portalId);
    }

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

    public bool? AreCompositeFilesEnabled()
    {
        int? portalId = GetPortalIdThroughReflection();
        return this.IsBooleanSettingEnabled(portalId, EnableCompositeFilesKey);
    }

    public bool? EnableCssMinification()
    {
        int? portalId = GetPortalIdThroughReflection();
        return this.IsBooleanSettingEnabled(portalId, MinifyCssKey);
    }

    public bool? EnableJsMinification()
    {
        int? portalId = GetPortalIdThroughReflection();
        return this.IsBooleanSettingEnabled(portalId, MinifyJsKey);
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
        try
        {
            if (portalId.HasValue)
            {
                var method = PortalControllerType.GetMethod("GetPortalSettingsDictionary");
                var dictionary = (Dictionary<string, string>)method.Invoke(null, new object[] { portalId.Value });
                string value;
                if (dictionary.TryGetValue(settingKey, out value))
                {
                    return value;
                }
            }
        }
        catch (Exception)
        {
            // ignore
        }

        return null;
    }

    private static int? GetPortalIdThroughReflection()
    {
        try
        {
            var method = PortalAliasControllerType.GetMethod("GetPortalAliasInfo");
            var portalAliasInfo = HttpContext.Current != null ? method.Invoke(null, new object[] { HttpContext.Current.Request.Url.Host }) : null;
            if (portalAliasInfo != null)
            {
                object portalId = portalAliasInfo.GetType().GetProperty("PortalID").GetValue(portalAliasInfo, new object[] { });
                return (int)portalId;
            }
        }
        catch (Exception)
        {
            // ignore
        }

        return null;
    }

    private static string GetHostSettingThroughReflection(string settingKey)
    {
        try
        {
            var method = HostControllerType.GetMethod("GetSettingsDictionary");
            var property = HostControllerType.BaseType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            var instance = property.GetValue(null, Type.EmptyTypes);
            var dictionary = (Dictionary<string, string>)method.Invoke(instance, Type.EmptyTypes);
            string value;
            if (dictionary.TryGetValue(settingKey, out value))
            {
                return value;
            }
        }
        catch (Exception)
        {
            // ignore
        }

        return null;
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

    private UpgradeStatus GetStatusByReflection()
    {
        try
        {
            var property = CommonGlobalsType.GetProperty("Status", BindingFlags.Static | BindingFlags.Public);
            var status = (UpgradeStatus)property.GetValue(null, null);
            return status;
        }
        catch (Exception)
        {
            return UpgradeStatus.Unknown;
        }
    }
}
