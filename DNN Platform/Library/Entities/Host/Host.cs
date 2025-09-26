// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client;

    using SchedulerMode = DotNetNuke.Services.Scheduling.SchedulerMode;

    /// <summary>Contains most of the host settings.</summary>
    [Serializable]
    [DnnDeprecated(10, 0, 2, "Use DotNetNuke.Abstractions.Application.IHostSettings or DotNetNuke.Abstractions.Application.IMailSettings via dependency injection")]
    public partial class Host : BaseEntityInfo
    {
        private static Globals.PerformanceSettings? performanceSetting;

        /// <summary>Gets the duration before an account is automatically unlocked.</summary>
        public static int AutoAccountUnlockDuration => HostController.Instance.GetInteger("AutoAccountUnlockDuration", 10);

        /// <summary>Gets the <see cref="HttpCacheability"/> value for authenticated requests.</summary>
        /// <remarks>The following mapping is used:
        /// <list type="bullet">
        /// <item><c>"0"</c> - <see cref="HttpCacheability.NoCache"/></item>
        /// <item><c>"1"</c> - <see cref="HttpCacheability.Private"/></item>
        /// <item><c>"2"</c> - <see cref="HttpCacheability.Public"/></item>
        /// <item><c>"3"</c> - <see cref="HttpCacheability.Server"/></item>
        /// <item><c>"4"</c> - <see cref="HttpCacheability.ServerAndNoCache"/></item>
        /// <item><c>"5"</c> - <see cref="HttpCacheability.ServerAndPrivate"/></item>
        /// </list>
        /// </remarks>
        public static string AuthenticatedCacheability => HostController.Instance.GetString("AuthenticatedCacheability", "ServerAndNoCache");

        /// <summary>Gets the <see cref="HttpCacheability"/> value for unauthenticated requests.</summary>
        /// <remarks>The following mapping is used:
        /// <list type="bullet">
        /// <item><c>"0"</c> - <see cref="HttpCacheability.NoCache"/></item>
        /// <item><c>"1"</c> - <see cref="HttpCacheability.Private"/></item>
        /// <item><c>"2"</c> - <see cref="HttpCacheability.Public"/></item>
        /// <item><c>"3"</c> - <see cref="HttpCacheability.Server"/></item>
        /// <item><c>"4"</c> - <see cref="HttpCacheability.ServerAndNoCache"/></item>
        /// <item><c>"5"</c> - <see cref="HttpCacheability.ServerAndPrivate"/></item>
        /// </list>
        /// </remarks>
        public static string UnauthenticatedCacheability => HostController.Instance.GetString("UnauthenticatedCacheability", "ServerAndNoCache");

        /// <summary>Gets a value indicating whether CDN has been enabled for all registered javascript libraries.</summary>
        public static bool CdnEnabled => HostController.Instance.GetBoolean("CDNEnabled", false);

        /// <summary>Gets a value indicating whether the Upgrade Indicator is enabled.</summary>
        public static bool CheckUpgrade => HostController.Instance.GetBoolean("CheckUpgrade", true);

        /// <summary>Gets the Control Panel.</summary>
        public static string ControlPanel => HostController.Instance.GetString("ControlPanel", Globals.glbDefaultControlPanel);

        /// <summary>Gets a value indicating whether the default Edit Bar is disabled.</summary>
        public static bool DisableEditBar => HostController.Instance.GetBoolean("DisableEditBar", false);

        /// <summary>
        /// Gets a value indicating whether setting to control where the control panel is loaded by the core and allowed to control its own visibility.
        /// this is useful when the control panel needs to be visible for all users regardless of edit page/module permissions.
        /// it's also for backwards compatibility, prior to 7.2 the control panel was always loaded.
        /// </summary>
        public static bool AllowControlPanelToDetermineVisibility => HostController.Instance.GetBoolean("AllowControlPanelToDetermineVisibility", Globals.glbAllowControlPanelToDetermineVisibility);

        /// <summary>Gets a value indicating whether Composite Files are enabled at the host level.</summary>
        public static bool CrmEnableCompositeFiles => HostController.Instance.GetBoolean(ClientResourceSettings.EnableCompositeFilesKey, false);

        /// <summary>Gets a value indicating whether CSS Minification is enabled at the host level.</summary>
        public static bool CrmMinifyCss => HostController.Instance.GetBoolean(ClientResourceSettings.MinifyCssKey);

        /// <summary>Gets a value indicating whether JS Minification is enabled at the host level.</summary>
        public static bool CrmMinifyJs => HostController.Instance.GetBoolean(ClientResourceSettings.MinifyJsKey);

        /// <summary>Gets the Client Resource Management version number.</summary>
        public static int CrmVersion => HostController.Instance.GetInteger(ClientResourceSettings.VersionKey, 1);

        /// <summary>Gets the Default Admin Container.</summary>
        public static string DefaultAdminContainer
        {
            get
            {
                string setting = HostController.Instance.GetString("DefaultAdminContainer");
                if (string.IsNullOrEmpty(setting))
                {
                    setting = SkinController.GetDefaultAdminContainer();
                }

                return setting;
            }
        }

        /// <summary>Gets the Default Admin Skin.</summary>
        public static string DefaultAdminSkin
        {
            get
            {
                string setting = HostController.Instance.GetString("DefaultAdminSkin");
                if (string.IsNullOrEmpty(setting))
                {
                    setting = SkinController.GetDefaultAdminSkin();
                }

                return setting;
            }
        }

        /// <summary>Gets the Default Doc Type.</summary>
        public static string DefaultDocType
        {
            get
            {
                string doctype = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
                string setting = HostController.Instance.GetString("DefaultDocType");
                if (!string.IsNullOrEmpty(setting))
                {
                    switch (setting)
                    {
                        case "0":
                            doctype = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
                            break;
                        case "1":
                            doctype = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
                            break;
                        case "2":
                            doctype = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";
                            break;
                        case "3":
                            doctype = "<!DOCTYPE html>";
                            break;
                    }
                }

                return doctype;
            }
        }

        /// <summary>Gets the Default Portal Container.</summary>
        public static string DefaultPortalContainer
        {
            get
            {
                string setting = HostController.Instance.GetString("DefaultPortalContainer");
                if (string.IsNullOrEmpty(setting))
                {
                    setting = SkinController.GetDefaultPortalContainer();
                }

                return setting;
            }
        }

        /// <summary>Gets the Default Portal Skin.</summary>
        public static string DefaultPortalSkin
        {
            get
            {
                string setting = HostController.Instance.GetString("DefaultPortalSkin");
                if (string.IsNullOrEmpty(setting))
                {
                    setting = SkinController.GetDefaultPortalSkin();
                }

                return setting;
            }
        }

        /// <summary>Gets the Demo Period for new portals.</summary>
        public static int DemoPeriod => HostController.Instance.GetInteger("DemoPeriod");

        /// <summary>Gets a value indicating whether demo signups are enabled.</summary>
        public static bool DemoSignup => HostController.Instance.GetBoolean("DemoSignup", false);

        /// <summary>Gets a value indicating whether to display the beta notice.</summary>
        public static bool DisplayBetaNotice => HostController.Instance.GetBoolean("DisplayBetaNotice", true);

        /// <summary>Gets a value indicating whether to display the copyright.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.9.2. No replacement. Scheduled removal in v11.0.0.")]
        public static bool DisplayCopyright => HostController.Instance.GetBoolean("Copyright", true);

        /// <summary>Gets a value indicating whether enable checking for banned words when setting password during registration.</summary>
        public static bool EnableBannedList => HostController.Instance.GetBoolean("EnableBannedList", true);

        /// <summary>Gets a value indicating whether Browser Language Detection is Enabled.</summary>
        public static bool EnableBrowserLanguage => HostController.Instance.GetBoolean("EnableBrowserLanguage", true);

        public static bool EnableContentLocalization => HostController.Instance.GetBoolean("EnableContentLocalization", false);

        /// <summary>
        ///   Gets a value indicating whether the installation runs in debug mode. This property can be used
        ///   by the framework and extensions alike to write more verbose logs/onscreen
        ///   information, etc. It is set in the host settings page.
        /// </summary>
        public static bool DebugMode => HostController.Instance.GetBoolean("DebugMode", false);

        /// <summary>Gets a value indicating whether a CSS class based on the Module Name is automatically rendered.</summary>
        public static bool EnableCustomModuleCssClass => HostController.Instance.GetBoolean("EnableCustomModuleCssClass", true);

        /// <summary>Gets a value indicating whether force upgrade wizard open in ssl channel.</summary>
        public static bool UpgradeForceSsl => HostController.Instance.GetBoolean("UpgradeForceSSL", false);

        /// <summary>Gets the domain used when upgrade wizard forced to shown in ssl channel.</summary>
        public static string SslDomain => HostController.Instance.GetString("SSLDomain");

        /// <summary>Gets a value indicating whether File AutoSync is Enabled.</summary>
        public static bool EnableFileAutoSync => HostController.Instance.GetBoolean("EnableFileAutoSync", false);

        /// <summary>Gets a value indicating whether the IP address of the user is checked against a list during login.</summary>
        public static bool EnableIPChecking => HostController.Instance.GetBoolean("EnableIPChecking", false);

        /// <summary>Gets a value indicating whether Module Online Help is Enabled.</summary>
        public static bool EnableModuleOnLineHelp => HostController.Instance.GetBoolean("EnableModuleOnLineHelp", false);

        /// <summary>Gets a value indicating whether the Request Filters are Enabled.</summary>
        public static bool EnableRequestFilters => HostController.Instance.GetBoolean("EnableRequestFilters", false);

        /// <summary>Gets a value indicating whether a client-side password strength meter is shown on registration screen.</summary>
        public static bool EnableStrengthMeter => HostController.Instance.GetBoolean("EnableStrengthMeter", false);

        /// <summary>Gets a value indicating whether a previous passwords are stored to check if user is reusing them.</summary>
        public static bool EnablePasswordHistory => HostController.Instance.GetBoolean("EnablePasswordHistory", true);

        /// <summary>Gets a value indicating whether to use the Language in the Url.</summary>
        public static bool EnableUrlLanguage => HostController.Instance.GetBoolean("EnableUrlLanguage", true);

        /// <summary>Gets a value indicating whether Users Online are Enabled.</summary>
        [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled removal in v11.0.0.")]
        public static bool EnableUsersOnline => !HostController.Instance.GetBoolean("DisableUsersOnline", true);

        /// <summary>Gets a value indicating whether SSL is Enabled for SMTP.</summary>
        public static bool EnableSMTPSSL
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetPortalSettingAsBoolean("SMTPEnableSSL", PortalSettings.Current.PortalId, false);
                }

                return HostController.Instance.GetBoolean("SMTPEnableSSL", false);
            }
        }

        /// <summary>Gets the currently configured SMTP OAuth provider if existing, for the current portal if portal SMTP enabled, otherwise for the installation.</summary>
        public static string SMTPAuthProvider
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetPortalSetting("SMTPAuthProvider", PortalSettings.Current.PortalId, string.Empty);
                }

                return HostController.Instance.GetString("SMTPAuthProvider", string.Empty);
            }
        }

        /// <summary>Gets a value indicating whether the Event Log Buffer is Enabled.</summary>
        public static bool EventLogBuffer => HostController.Instance.GetBoolean("EventLogBuffer", false);

        /// <summary>Gets the allowed file extensions.</summary>
        public static FileExtensionWhitelist AllowedExtensionWhitelist => new(HostController.Instance.GetString("FileExtensions"));

        /// <summary>Gets default list of extensions an end user is allowed to upload.</summary>
        public static FileExtensionWhitelist DefaultEndUserExtensionWhitelist => new(HostController.Instance.GetString("DefaultEndUserExtensionWhitelist"));

        /// <summary>Gets the GUID.</summary>
        public static string GUID => HostController.Instance.GetString("GUID");

        /// <summary>Gets the Help URL.</summary>
        public static string HelpURL => HostController.Instance.GetString("HelpURL");

        /// <summary>Gets the Host Currency.</summary>
        public static string HostCurrency
        {
            get
            {
                string setting = HostController.Instance.GetString("HostCurrency");
                if (string.IsNullOrEmpty(setting))
                {
                    setting = "USD";
                }

                return setting;
            }
        }

        /// <summary>Gets the Host Email.</summary>
        public static string HostEmail => HostController.Instance.GetString("HostEmail");

        /// <summary>Gets the Host Fee.</summary>
        public static double HostFee => HostController.Instance.GetDouble("HostFee", 0);

        /// <summary>Gets the Host Portal's PortalId.</summary>
        public static int HostPortalID => HostController.Instance.GetInteger("HostPortalId");

        /// <summary>Gets the Host Space.</summary>
        public static double HostSpace => HostController.Instance.GetDouble("HostSpace", 0);

        /// <summary>Gets the Host Title.</summary>
        public static string HostTitle => HostController.Instance.GetString("HostTitle");

        /// <summary>Gets the Host URL.</summary>
        public static string HostURL => HostController.Instance.GetString("HostURL");

        /// <summary>Gets the HttpCompression Algorithm.</summary>
        public static int HttpCompressionAlgorithm => HostController.Instance.GetInteger("HttpCompression");

        /// <summary>Gets size of the batch used to determine how many emails are sent per CoreMessaging Scheduler run.</summary>
        public static int MessageSchedulerBatchSize => HostController.Instance.GetInteger("MessageSchedulerBatchSize", 50);

        /// <summary>Gets set length of time (in minutes) that reset links are valid for - default is 60.</summary>
        public static int MembershipResetLinkValidity => HostController.Instance.GetInteger("MembershipResetLinkValidity", 60);

        /// <summary>Gets set length of time (in minutes) that reset links are valid for - default is 24 hours (1440 min).</summary>
        public static int AdminMembershipResetLinkValidity => HostController.Instance.GetInteger("AdminMembershipResetLinkValidity", 1440);

        /// <summary>Gets set number of passwords stored for password change comparison operations - default is 5.</summary>
        public static int MembershipNumberPasswords => HostController.Instance.GetInteger("MembershipNumberPasswords", 5);

        /// <summary>Gets the number of days that must pass before a password can be reused - default is 0 (i.e. password reuse is only governed by <see cref="EnablePasswordHistory"/> and <see cref="MembershipNumberPasswords"/>).</summary>
        public static int MembershipDaysBeforePasswordReuse => HostController.Instance.GetInteger("MembershipDaysBeforePasswordReuse", 0);

        /// <summary>
        /// Gets the HTTP Status code returned if IP address filtering is enabled on login
        /// and the users IP does not meet criteria -default is 403.
        /// </summary>
        public static string MembershipFailedIPException => HostController.Instance.GetString("MembershipFailedIPException", "403");

        /// <summary>Gets the Module Caching method.</summary>
        public static string ModuleCachingMethod => HostController.Instance.GetString("ModuleCaching");

        /// <summary>Gets the Page Caching method.</summary>
        public static string PageCachingMethod => HostController.Instance.GetString("PageCaching");

        /// <summary>Gets the Page Quota.</summary>
        public static int PageQuota => HostController.Instance.GetInteger("PageQuota", 0);

        /// <summary>Gets the PageState Persister.</summary>
        public static string PageStatePersister
        {
            get
            {
                string setting = HostController.Instance.GetString("PageStatePersister");
                if (string.IsNullOrEmpty(setting))
                {
                    setting = "P";
                }

                return setting;
            }
        }

        /// <summary>Gets the Password Expiry.</summary>
        public static int PasswordExpiry => HostController.Instance.GetInteger("PasswordExpiry", 0);

        /// <summary>Gets the Password Expiry Reminder window.</summary>
        public static int PasswordExpiryReminder => HostController.Instance.GetInteger("PasswordExpiryReminder", 7);

        /// <summary>Gets the Payment Processor.</summary>
        public static string PaymentProcessor => HostController.Instance.GetString("PaymentProcessor");

        /// <summary>Gets the Payment Processor Password.</summary>
        public static string ProcessorPassword => HostController.Instance.GetEncryptedString("ProcessorPassword", Config.GetDecryptionkey());

        /// <summary>Gets the Payment Processor User Id.</summary>
        public static string ProcessorUserId => HostController.Instance.GetString("ProcessorUserId");

        /// <summary>Gets the Proxy Server Password.</summary>
        public static string ProxyPassword => HostController.Instance.GetString("ProxyPassword");

        /// <summary>Gets the Proxy Server Port.</summary>
        public static int ProxyPort => HostController.Instance.GetInteger("ProxyPort");

        /// <summary>Gets the Proxy Server.</summary>
        public static string ProxyServer => HostController.Instance.GetString("ProxyServer");

        /// <summary>Gets the Proxy Server UserName.</summary>
        public static string ProxyUsername => HostController.Instance.GetString("ProxyUsername");

        /// <summary>Gets a value indicating whether to use the remember me checkbox.</summary>
        public static bool RememberCheckbox => HostController.Instance.GetBoolean("RememberCheckbox", true);

        /// <summary>Gets the Scheduler Mode.</summary>
        public static SchedulerMode SchedulerMode
        {
            get
            {
                SchedulerMode setting = SchedulerMode.TIMER_METHOD;
                string s = HostController.Instance.GetString("SchedulerMode");
                if (!string.IsNullOrEmpty(s))
                {
                    setting = (SchedulerMode)Enum.Parse(typeof(SchedulerMode), s);
                }

                return setting;
            }
        }

        /// <summary>Gets the delayAtAppStart value.</summary>
        /// <remarks>Defaults is 1 min(60 sec).</remarks>
        public static int SchedulerdelayAtAppStart => HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1);

        /// <summary>Gets a value indicating whether to include Common Words in the Search Index.</summary>
        public static bool SearchIncludeCommon => HostController.Instance.GetBoolean("SearchIncludeCommon", false);

        /// <summary>Gets a value indicating whether to include Numbers in the Search Index.</summary>
        public static bool SearchIncludeNumeric => HostController.Instance.GetBoolean("SearchIncludeNumeric", true);

        /// <summary>Gets the maximum Search Word length to index.</summary>
        public static int SearchMaxWordlLength => HostController.Instance.GetInteger("MaxSearchWordLength", 50);

        /// <summary>Gets the maximum Search Word length to index.</summary>
        public static int SearchMinWordlLength => HostController.Instance.GetInteger("MinSearchWordLength", 4);

        /// <summary>Gets the filter used for inclusion of tag info.</summary>
        public static string SearchIncludedTagInfoFilter => HostController.Instance.GetString("SearchIncludedTagInfoFilter", string.Empty);

        /// <summary>Gets a value indicating whether display the text of errors injected via the error querystring parameter.</summary>
        public static bool ShowCriticalErrors => HostController.Instance.GetBoolean("ShowCriticalErrors", true);

        /// <summary>Gets the Site Log Buffer size.</summary>
        [Obsolete("Deprecated in DotNetNuke 8.0.0. No replacement. Scheduled removal in v11.0.0.")]
        public static int SiteLogBuffer => 1;

        /// <summary>Gets the Site Log History.</summary>
        [Obsolete("Deprecated in DotNetNuke 8.0.0. No replacement. Scheduled removal in v11.0.0.")]
        public static int SiteLogHistory => 0;

        /// <summary>Gets the Site Log Storage location.</summary>
        [Obsolete("Deprecated in DotNetNuke 8.0.0. No replacement. Scheduled removal in v11.0.0.")]
        public static string SiteLogStorage => "D";

        /// <summary>Gets the SMTP Authentication.</summary>
        public static string SMTPAuthentication => GetSmtpSetting("SMTPAuthentication");

        /// <summary>Gets the SMTP Password.</summary>
        public static string SMTPPassword
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetEncryptedString("SMTPPassword", PortalController.Instance.GetCurrentPortalSettings().PortalId, Config.GetDecryptionkey());
                }
                else
                {
                    string decryptedText;
                    try
                    {
                        decryptedText = HostController.Instance.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
                    }
                    catch (Exception)
                    {
                        // fixes case where smtppassword failed to encrypt due to failing upgrade
                        var current = HostController.Instance.GetString("SMTPPassword");
                        if (!string.IsNullOrEmpty(current))
                        {
                            HostController.Instance.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                            decryptedText = current;
                        }
                        else
                        {
                            decryptedText = string.Empty;
                        }
                    }

                    return decryptedText;
                }
            }
        }

        /// <summary>Gets the SMTP Server.</summary>
        public static string SMTPServer => GetSmtpSetting("SMTPServer");

        /// <summary>Gets the SMTP Username.</summary>
        public static string SMTPUsername => GetSmtpSetting("SMTPUsername");

        /// <summary>Gets the SMTP Connection Limit.</summary>
        public static int SMTPConnectionLimit
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", PortalSettings.Current.PortalId, 2);
                }

                return HostController.Instance.GetInteger("SMTPConnectionLimit", 2);
            }
        }

        /// <summary>Gets the SMTP MaxIdleTime.</summary>
        public static int SMTPMaxIdleTime
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", PortalSettings.Current.PortalId, 100000);
                }

                return HostController.Instance.GetInteger("SMTPMaxIdleTime", 100000);
            }
        }

        /// <summary>Gets a value indicating whether Exceptions are rethrown.</summary>
        public static bool ThrowCBOExceptions => HostController.Instance.GetBoolean("ThrowCBOExceptions", false);

        /// <summary>Gets a value indicating whether Friendly Urls is Enabled.</summary>
        public static bool UseFriendlyUrls => HostController.Instance.GetBoolean("UseFriendlyUrls", false);

        /// <summary>Gets a value indicating whether Custom Error Messages is Enabled.</summary>
        public static bool UseCustomErrorMessages => HostController.Instance.GetBoolean("UseCustomErrorMessages", false);

        /// <summary>Gets the User Quota.</summary>
        public static int UserQuota => HostController.Instance.GetInteger("UserQuota", 0);

        /// <summary>Gets the window to use in minutes when determining if the user is online.</summary>
        [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled removal in v11.0.0.")]
        public static int UsersOnlineTimeWindow => HostController.Instance.GetInteger("UsersOnlineTime", 15);

        /// <summary>Gets the WebRequest Timeout value.</summary>
        public static int WebRequestTimeout => HostController.Instance.GetInteger("WebRequestTimeout", 10000);

        /// <summary>Gets a value indicating whether to use a hosted version of the MS Ajax Library.</summary>
        public static bool EnableMsAjaxCdn => HostController.Instance.GetBoolean("EnableMsAjaxCDN", false);

        /// <summary>Gets the time, in seconds, before asynchronous postbacks time out if no response is received.</summary>
        public static int AsyncTimeout
        {
            get
            {
                var timeout = HostController.Instance.GetInteger("AsyncTimeout", 90);
                if (timeout < 90)
                {
                    timeout = 90;
                }

                return timeout;
            }
        }

        /// <summary>Gets a value indicating whether to put the entire instance into maintenance mode.</summary>
        public static bool IsLocked
        {
            get { return HostController.Instance.GetBoolean("IsLocked", false); }
        }

        /// <summary>Gets or sets the PerformanceSettings.</summary>
        public static Globals.PerformanceSettings PerformanceSetting
        {
            get
            {
                if (!performanceSetting.HasValue)
                {
                    var s = HostController.Instance.GetString("PerformanceSetting");
                    if (string.IsNullOrEmpty(s))
                    {
                        return Globals.PerformanceSettings.ModerateCaching;
                    }

                    performanceSetting = (Globals.PerformanceSettings)Enum.Parse(typeof(Globals.PerformanceSettings), s);
                }

                return performanceSetting.Value;
            }

            set
            {
                performanceSetting = value;
            }
        }

        /// <summary>Gets a value indicating whether SMTP information is stored at the portal level.</summary>
        internal static bool SMTPPortalEnabled
        {
            get
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();

                if (portalSettings == null || TabController.CurrentPage == null)
                {
                    // without portal settings or active tab, we can't continue
                    return false;
                }

                // we don't want to load the portal smtp server when on a host tab.
                if (TabController.CurrentPage.PortalID == Null.NullInteger)
                {
                    return false;
                }

                var currentSmtpMode = PortalController.GetPortalSetting("SMTPmode", portalSettings.PortalId, Null.NullString);

                return currentSmtpMode.Equals("P", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>Gets the SMTP setting, if portal SMTP is configured, it will return items from the portal settings collection.</summary>
        private static string GetSmtpSetting(string settingName)
        {
            if (SMTPPortalEnabled)
            {
                return PortalController.GetPortalSetting(settingName, PortalSettings.Current.PortalId, Null.NullString);
            }

            return HostController.Instance.GetString(settingName);
        }
    }
}
