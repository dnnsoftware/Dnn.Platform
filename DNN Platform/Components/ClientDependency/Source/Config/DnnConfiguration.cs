//*** DNN related change *** the whole file
namespace ClientDependency.Core.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;

    // note: this class is duplicated in DotNetNuke.Web.Client.ClientResourceSettings, any updates need to be synced between the two.
    // currently the only item utilized is the version node to override the version of files per-portal.
    public class DnnConfiguration
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
        public static readonly string DebugModeKey = "DebugMode";

        private static readonly Type _portalControllerType;
        private static readonly Type _portalAliasControllerType;
        private static readonly Type _hostControllerType;
        private static readonly Type _commonGlobalsType;

        static DnnConfiguration()
        {
            try
            {
                // all these types are part of the same library, so we don't need a separate catch for each one
                _commonGlobalsType = Type.GetType("DotNetNuke.Common.Globals, DotNetNuke");
                _portalControllerType = Type.GetType("DotNetNuke.Entities.Portals.PortalController, DotNetNuke");
                _portalAliasControllerType = Type.GetType("DotNetNuke.Entities.Portals.PortalAliasController, DotNetNuke");
                _hostControllerType = Type.GetType("DotNetNuke.Entities.Controllers.HostController, DotNetNuke");
            }
            catch (Exception)
            {
            }
        }

        public int? GetVersion()
        {
            var portalVersion = GetIntegerSetting(PortalSettingsDictionaryKey, VersionKey);
            var overrideDefaultSettings = GetBooleanSetting(PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            if (portalVersion.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value)
                return portalVersion.Value;

            // otherwise return the host setting
            var hostVersion = GetIntegerSetting(HostSettingsDictionaryKey, VersionKey);
            if (hostVersion.HasValue)
                return hostVersion.Value;

            // otherwise tell the calling method that nothing is set
            return null;
        }

        public bool? AreCompositeFilesEnabled()
        {
            return IsBooleanSettingEnabled(EnableCompositeFilesKey);
        }

        public bool? EnableCssMinification()
        {
            return IsBooleanSettingEnabled(MinifyCssKey);
        }

        public bool? EnableJsMinification()
        {
            return IsBooleanSettingEnabled(MinifyJsKey);
        }

        public bool IsDebugMode()
        {
            if (Status != UpgradeStatus.None)
            {
                return true;
            }

            var hostEnabled = GetBooleanSetting(HostSettingsDictionaryKey, DebugModeKey);
            if (hostEnabled.HasValue)
                return hostEnabled.Value;

            return false;
        }

        private bool? IsBooleanSettingEnabled(string settingKey)
        {
            if(Status != UpgradeStatus.None)
            {
                return false;
            }
            
            var portalEnabled = GetBooleanSetting(PortalSettingsDictionaryKey, settingKey);
            var overrideDefaultSettings = GetBooleanSetting(PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            if (portalEnabled.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value)
                return portalEnabled.Value;

            // otherwise return the host setting
            var hostEnabled = GetBooleanSetting(HostSettingsDictionaryKey, settingKey);
            if (hostEnabled.HasValue)
                return hostEnabled.Value;

            // otherwise tell the calling method that nothing is set
            return null;
        }

        private static bool? GetBooleanSetting(string dictionaryKey, string settingKey)
        {
            var setting = GetSetting(dictionaryKey, settingKey);
            bool result;
            if (setting != null && bool.TryParse(setting, out result))
            {
                return result;
            }
            return null;
        }

        private static int? GetIntegerSetting(string dictionaryKey, string settingKey)
        {
            var setting = GetSetting(dictionaryKey, settingKey);
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

        private static string GetSetting(string dictionaryKey, string settingKey)
        {
            var settings = HttpContext.Current.Items[dictionaryKey];
            if (settings == null)
            {
                if (dictionaryKey == HostSettingsDictionaryKey)
                    return GetHostSettingThroughReflection(settingKey);

                return GetPortalSettingThroughReflection(settingKey);
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

        private static string GetPortalSettingThroughReflection(string settingKey)
        {
            try
            {
                int? portalId = GetPortalIdThroughReflection();
                if (portalId.HasValue)
                {
                    var method = _portalControllerType.GetMethod("GetPortalSettingsDictionary");
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
            }
            return null;
        }

        private static int? GetPortalIdThroughReflection()
        {
            try
            {
                var method = _portalAliasControllerType.GetMethod("GetPortalAliasInfo");
                var requestedDomainNameMethod = _commonGlobalsType.GetMethod("GetDomainName", new[] { typeof(HttpRequest) });
                if (requestedDomainNameMethod != null)
                {
                    var requestedDomainName = requestedDomainNameMethod.Invoke(null, new object[] { HttpContext.Current.Request });

                    var portalAliasInfo = method.Invoke(null, new object[] { requestedDomainName });
                    if (portalAliasInfo != null)
                    {
                        object portalId = portalAliasInfo.GetType().GetProperty("PortalID").GetValue(portalAliasInfo, new object[] { });
                        return (int)portalId;
                    }
                }
                
            }
            catch (Exception)
            {
            }
            return null;
        }

        private static string GetHostSettingThroughReflection(string settingKey)
        {
            try
            {
                var method = _hostControllerType.GetMethod("GetSettingsDictionary");
                var property = _hostControllerType.BaseType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
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
            }
            return null;
        }

        private bool _statusChecked;
        private UpgradeStatus _status;

        private UpgradeStatus Status
        {
            get
            {
                if (!_statusChecked)
                {
                    _status = GetStatusByReflection();
                    _statusChecked = true;
                }

                return _status;
            }
        }

        private UpgradeStatus GetStatusByReflection()
        {
            try
            {
                var property = _commonGlobalsType.GetProperty("Status", BindingFlags.Static | BindingFlags.Public);
                var status = (UpgradeStatus)property.GetValue(null, null);
                return status;
            }
            catch (Exception)
            {
                return UpgradeStatus.Unknown;
            }
        }

        private enum UpgradeStatus
        {
            /// <summary>
            /// The application need update to a higher version.
            /// </summary>
            Upgrade,
            /// <summary>
            /// The application need to install itself.
            /// </summary>
            Install,
            /// <summary>
            /// The application is normal running.
            /// </summary>
            None,
            /// <summary>
            /// The application occur error when running.
            /// </summary>
            Error,
            /// <summary>
            /// The application status is unknown,
            /// </summary>
            /// <remarks>This status should never be returned. its is only used as a flag that Status hasn't been determined.</remarks>
            Unknown
        }
    }
}
