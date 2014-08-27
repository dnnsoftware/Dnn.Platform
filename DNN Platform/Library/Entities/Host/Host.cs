#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Framework;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Entities.Host
{
    using DotNetNuke.Entities.Portals;

    using Web.Client;

    /// <summary>
	/// Contains most of the host settings.
	/// </summary>
    [Serializable]
    public class Host : BaseEntityInfo
    {
        #region Public Shared Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the AutoAccountUnlockDuration
        /// </summary>
        /// <remarks>
        ///   Defaults to 10
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int AutoAccountUnlockDuration
        {
            get
            {
                return HostController.Instance.GetInteger("AutoAccountUnlockDuration", 10);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the AuthenticatedCacheability
        /// </summary>
        /// <remarks>
        ///   Defaults to HttpCacheability.ServerAndNoCache
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string AuthenticatedCacheability
        {
            get
            {
                return HostController.Instance.GetString("AuthenticatedCacheability", "4");
            }
        }

        public static string UnauthenticatedCacheability
        {
            get
            {
                return HostController.Instance.GetString("UnauthenticatedCacheability", "4");
            }
        }

        /// <summary>
        /// gets whether or not CDN has been enabled for all registered javascript libraries
        /// </summary>
        public static bool CdnEnabled
        {
            get
            {
                return HostController.Instance.GetBoolean("CDNEnabled", false);
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether the Upgrade Indicator is enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool CheckUpgrade
        {
            get
            {
                return HostController.Instance.GetBoolean("CheckUpgrade", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Control Panel
        /// </summary>
        /// <remarks>
        ///   Defaults to glbDefaultControlPanel constant
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string ControlPanel
        {
            get
            {
                return HostController.Instance.GetString("ControlPanel", Globals.glbDefaultControlPanel);
            }
        }

        /// <summary>
        /// setting to control where the control panel is loaded by the core and allowed to control it's own visibility.
        /// this is useful when the control panel needs to be visible for all users regardless of edit page/module permissions.
        /// it's also for backwards compatibility, prior to 7.2 the control panel was always loaded. 
        /// </summary>
        public static bool AllowControlPanelToDetermineVisibility
        {
            get
            {
                return HostController.Instance.GetBoolean("AllowControlPanelToDetermineVisibility", Globals.glbAllowControlPanelToDetermineVisibility);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Indicates whether Composite Files are enabled at the host level.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static bool CrmEnableCompositeFiles
        {
            get
            {
                return HostController.Instance.GetBoolean(ClientResourceSettings.EnableCompositeFilesKey, false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether CSS Minification is enabled at the host level.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static bool CrmMinifyCss
        {
            get
            {
                return HostController.Instance.GetBoolean(ClientResourceSettings.MinifyCssKey);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Indicates whether JS Minification is enabled at the host level.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static bool CrmMinifyJs
        {
            get
            {
                return HostController.Instance.GetBoolean(ClientResourceSettings.MinifyJsKey);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the Client Resource Management version number.
        /// </summary>
        /// <remarks>
        ///   Defaults to 1
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int CrmVersion
        {
            get
            {
                return HostController.Instance.GetInteger(ClientResourceSettings.VersionKey, 1);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Default Admin Container
        /// </summary>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Default Admin Skin
        /// </summary>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Default Doc Type
        /// </summary>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Default Portal Container
        /// </summary>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Default Portal Skin
        /// </summary>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Demo Period for new portals
        /// </summary>
        /// <remarks>
        ///   Defaults to -1
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int DemoPeriod
        {
            get
            {
                return HostController.Instance.GetInteger("DemoPeriod");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether demo signups are enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool DemoSignup
        {
            get
            {
                return HostController.Instance.GetBoolean("DemoSignup", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether to dislpay the beta notice
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool DisplayBetaNotice
        {
            get
            {
                return HostController.Instance.GetBoolean("DisplayBetaNotice", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether to dislpay the copyright
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool DisplayCopyright
        {
            get
            {
                return HostController.Instance.GetBoolean("Copyright", true);
            }
        }

        /// <summary>
        /// Enable checking for banned words when setting password during registration
        /// </summary>
        public static bool EnableBannedList
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableBannedList", true);
            }
        }
        

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether Browser Language Detection is Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableBrowserLanguage
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableBrowserLanguage", true);
            }
        }

        public static bool EnableContentLocalization
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableContentLocalization", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether the installation runs in debug mode. This property can be used
        ///   by the framework and extensions alike to write more verbose logs/onscreen
        ///   information, etc. It is set in the host settings page.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static bool DebugMode
        {
            get
            {
                return HostController.Instance.GetBoolean("DebugMode", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether the installation participates in the improvements program.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static bool ParticipateInImprovementProg
        {
            get
            {
                return HostController.Instance.GetBoolean("DnnImprovementProgram", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether a css class based on the Module Name is automatically rendered
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableCustomModuleCssClass
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableCustomModuleCssClass", true);
            }
        }

        /// <summary>
        /// Whether force upgrade wizard open in ssl channel.
        /// </summary>
        public static bool UpgradeForceSsl
        {
            get
            {
                return HostController.Instance.GetBoolean("UpgradeForceSSL", false);
            }
        }

        /// <summary>
        /// The domain used when upgrade wizard forced to shown in ssl channel.
        /// </summary>
        public static string SslDomain
        {
            get
            {
                return HostController.Instance.GetString("SSLDomain");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether File AutoSync is Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableFileAutoSync
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableFileAutoSync", false);
            }
        }

        /// <summary>
        /// enable whether the IP address of the user is checked against a list during login
        /// </summary>
        public static bool EnableIPChecking
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableIPChecking", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether Module Online Help is Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableModuleOnLineHelp
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableModuleOnLineHelp", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether the Request Filters are Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableRequestFilters
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableRequestFilters", false);
            }
        }

        /// <summary>
        /// enable whether a client-side password strength meter is shown on registration screen
        /// </summary>
        public static bool EnableStrengthMeter
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableStrengthMeter", false);
            }
        }

        /// <summary>
        /// enable whether a previous passwords are stored to check if user is reusing them
        /// </summary>
        public static bool EnablePasswordHistory
        {
            get
            {
                return HostController.Instance.GetBoolean("EnablePasswordHistory", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether to use the Language in the Url
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableUrlLanguage
        {
            get
            {
                return HostController.Instance.GetBoolean("EnableUrlLanguage", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether Users Online are Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EnableUsersOnline
        {
            get
            {
                return !HostController.Instance.GetBoolean("DisableUsersOnline", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether SSL is Enabled for SMTP
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
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
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether the Event Log Buffer is Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool EventLogBuffer
        {
            get
            {
                return HostController.Instance.GetBoolean("EventLogBuffer", false);
            }
        }

        /// <summary>
        ///   Gets the allowed file extensions.
        /// </summary>
        public static FileExtensionWhitelist AllowedExtensionWhitelist
        {
            get
            {
                return new FileExtensionWhitelist(HostController.Instance.GetString("FileExtensions"));
            }
        }
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the GUID
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string GUID
        {
            get
            {
                return HostController.Instance.GetString("GUID");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Help URL
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string HelpURL
        {
            get
            {
                return HostController.Instance.GetString("HelpURL");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host Currency
        /// </summary>
        /// <remarks>
        ///   Defaults to USD
        /// </remarks>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host Email
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string HostEmail
        {
            get
            {
                return HostController.Instance.GetString("HostEmail");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host Fee
        /// </summary>
        /// <remarks>
        ///   Defaults to 0
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static double HostFee
        {
            get
            {
                return HostController.Instance.GetDouble("HostFee", 0);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host Portal's PortalId
        /// </summary>
        /// <remarks>
        ///   Defaults to Null.NullInteger
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int HostPortalID
        {
            get
            {
                return HostController.Instance.GetInteger("HostPortalId");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host Space
        /// </summary>
        /// <remarks>
        ///   Defaults to 0
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static double HostSpace
        {
            get
            {
                return HostController.Instance.GetDouble("HostSpace", 0);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host Title
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string HostTitle
        {
            get
            {
                return HostController.Instance.GetString("HostTitle");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Host URL
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string HostURL
        {
            get
            {
                return HostController.Instance.GetString("HostURL");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the HttpCompression Algorithm
        /// </summary>
        /// <remarks>
        ///   Defaults to Null.NullInteger(None)
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int HttpCompressionAlgorithm
        {
            get
            {
                return HostController.Instance.GetInteger("HttpCompression");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns size of the batch used to determine how many emails are sent per CoreMessaging Scheduler run
        /// </summary>
        /// <remarks>
        ///   Defaults to 50
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int MessageSchedulerBatchSize
        {
            get
            {
                return HostController.Instance.GetInteger("MessageSchedulerBatchSize",50);
            }
        }

        /// <summary>
        /// set length of time (in minutes) that reset links are valid for - default is 60
        /// </summary>
        public static int MembershipResetLinkValidity
        {
            get
            {
                return HostController.Instance.GetInteger("MembershipResetLinkValidity", 60);
            }
        }

        /// <summary>
        /// set length of time (in minutes) that reset links are valid for - default is 24 hours (1440 min)
        /// </summary>
        public static int AdminMembershipResetLinkValidity
        {
            get
            {
                return HostController.Instance.GetInteger("AdminMembershipResetLinkValidity", 1440);
            }
        }

        /// <summary>
        /// set number of passwords stored for password change comparison operations - default is 5
        /// </summary>
        public static int MembershipNumberPasswords
        {
            get
            {
                return HostController.Instance.GetInteger("MembershipNumberPasswords", 5);
            }
        }

        /// <summary>
        /// sets the HTTP Status code returned if IP address filtering is enabled on login
        /// and the users IP does not meet criteria -default is 403
        /// </summary>
        public static string MembershipFailedIPException
        {
            get
            {
                return HostController.Instance.GetString("MembershipFailedIPException", "403");
            }
        }



        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Module Caching method
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string ModuleCachingMethod
        {
            get
            {
                return HostController.Instance.GetString("ModuleCaching");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Page Caching method
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string PageCachingMethod
        {
            get
            {
                return HostController.Instance.GetString("PageCaching");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Page Quota
        /// </summary>
        /// <remarks>
        ///   Defaults to 0
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int PageQuota
        {
            get
            {
                return HostController.Instance.GetInteger("PageQuota", 0);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the PageState Persister
        /// </summary>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Password Expiry
        /// </summary>
        /// <remarks>
        ///   Defaults to 0
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int PasswordExpiry
        {
            get
            {
                return HostController.Instance.GetInteger("PasswordExpiry", 0);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Password Expiry Reminder window
        /// </summary>
        /// <remarks>
        ///   Defaults to 7 (1 week)
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int PasswordExpiryReminder
        {
            get
            {
                return HostController.Instance.GetInteger("PasswordExpiryReminder", 7);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Payment Processor
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string PaymentProcessor
        {
            get
            {
                return HostController.Instance.GetString("PaymentProcessor");
            }
        }

        private static Globals.PerformanceSettings? _performanceSetting;

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the PerformanceSettings
        /// </summary>
        /// <remarks>
        ///   Defaults to PerformanceSettings.ModerateCaching
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static Globals.PerformanceSettings PerformanceSetting
        {
            get
            {
                if (!_performanceSetting.HasValue)
                {
                    var s = HostController.Instance.GetString("PerformanceSetting");
                    if (string.IsNullOrEmpty(s))
                    {
                        return Globals.PerformanceSettings.ModerateCaching;
                    }

                    _performanceSetting = (Globals.PerformanceSettings)Enum.Parse(typeof(Globals.PerformanceSettings), s);
                }

                return _performanceSetting.Value;
            }
            set { _performanceSetting = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Payment Processor Password
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string ProcessorPassword
        {
            get
            {
                return HostController.Instance.GetString("ProcessorPassword");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Payment Processor User Id
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string ProcessorUserId
        {
            get
            {
                return HostController.Instance.GetString("ProcessorUserId");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Proxy Server Password
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string ProxyPassword
        {
            get
            {
                return HostController.Instance.GetString("ProxyPassword");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Proxy Server Port
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static int ProxyPort
        {
            get
            {
                return HostController.Instance.GetInteger("ProxyPort");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Proxy Server
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string ProxyServer
        {
            get
            {
                return HostController.Instance.GetString("ProxyServer");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Proxy Server UserName
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string ProxyUsername
        {
            get
            {
                return HostController.Instance.GetString("ProxyUsername");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether to use the remember me checkbox
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool RememberCheckbox
        {
            get
            {
                return HostController.Instance.GetBoolean("RememberCheckbox", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Scheduler Mode
        /// </summary>
        /// <remarks>
        ///   Defaults to SchedulerMode.TIMER_METHOD
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static SchedulerMode SchedulerMode
        {
            get
            {
                SchedulerMode setting = SchedulerMode.TIMER_METHOD;
                string s = HostController.Instance.GetString("SchedulerMode");
                if (!string.IsNullOrEmpty(s))
                {
                    setting = (SchedulerMode) Enum.Parse(typeof (SchedulerMode), s);
                }
                return setting;
            }
        }
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the delayAtAppStart value
        /// </summary>
        /// <remarks>
        ///   Defaults is 1 min(60 sec)
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int SchedulerdelayAtAppStart
        {
            get
            {
                return HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1);
            }
        }
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether to inlcude Common Words in the Search Index
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool SearchIncludeCommon
        {
            get
            {
                return HostController.Instance.GetBoolean("SearchIncludeCommon", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether to inlcude Numbers in the Search Index
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool SearchIncludeNumeric
        {
            get
            {
                return HostController.Instance.GetBoolean("SearchIncludeNumeric", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the maximum Search Word length to index
        /// </summary>
        /// <remarks>
        ///   Defaults to 25
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int SearchMaxWordlLength
        {
            get
            {
                return HostController.Instance.GetInteger("MaxSearchWordLength", 50);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the maximum Search Word length to index
        /// </summary>
        /// <remarks>
        ///   Defaults to 3
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int SearchMinWordlLength
        {
            get
            {
                return HostController.Instance.GetInteger("MinSearchWordLength", 4);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the filter used for inclusion of tag info
        /// </summary>
        /// <remarks>
        ///   Defaults to ""
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string SearchIncludedTagInfoFilter
        {
            get
            {
                return HostController.Instance.GetString("SearchIncludedTagInfoFilter", "");
            }
        }

        /// <summary>
        /// display the text of errors injected via the error querystring parameter
        /// </summary>
        public static bool ShowCriticalErrors
        {
            get
            {
                return HostController.Instance.GetBoolean("ShowCriticalErrors", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Site Log Buffer size
        /// </summary>
        /// <remarks>
        ///   Defaults to 1
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in 8.0.0")]
        public static int SiteLogBuffer
        {
            get
            {
                return 1;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Site Log History
        /// </summary>
        /// <remarks>
        ///   Defaults to -1
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in 8.0.0")]
        public static int SiteLogHistory
        {
            get
            {
                return 0;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the Site Log Storage location
        /// </summary>
        /// <remarks>
        ///   Defaults to "D"
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in 8.0.0")]
        public static string SiteLogStorage
        {
            get
            {
                return "D";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the SMTP Authentication
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string SMTPAuthentication
        {
            get
            {
                return GetSmtpSetting("SMTPAuthentication");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the SMTP mode (portal|host)
        /// </summary>
        /// -----------------------------------------------------------------------------
        internal static bool SMTPPortalEnabled
        {
            get
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                if (portalSettings == null || portalSettings.ActiveTab == null)
                {
                    //without portal settings or active tab, we can't continue
                    return false;
                }

                //we don't want to load the portal smtp server when on a host tab. 
                if (portalSettings.ActiveTab.PortalID == Null.NullInteger)
                {
                    return false;
                }

                var currentSmtpMode = PortalController.GetPortalSetting("SMTPmode", portalSettings.PortalId, Null.NullString);

                return currentSmtpMode.Equals("P", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get's the SMTP setting, if portal smtp is configured, it will return items from the portal settings collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static string GetSmtpSetting(string settingName)
        {
            if (SMTPPortalEnabled)
            {
                return PortalController.GetPortalSetting(settingName, PortalSettings.Current.PortalId, Null.NullString);
            }

            return HostController.Instance.GetString(settingName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the SMTP Password
        /// </summary>
        /// -----------------------------------------------------------------------------
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
                        //fixes case where smtppassword failed to encrypt due to failing upgrade
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the SMTP Server
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string SMTPServer
        {
            get
            {
                return GetSmtpSetting("SMTPServer");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the SMTP Username
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string SMTPUsername
        {
            get
            {
                return GetSmtpSetting("SMTPUsername");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the SMTP Connection Limit
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static int SMTPConnectionLimit
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", PortalSettings.Current.PortalId, 1);
                }
                return HostController.Instance.GetInteger("SMTPConnectionLimit", 1);
            }
        }
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the SMTP MaxIdleTime
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static int SMTPMaxIdleTime
        {
            get
            {
                if (SMTPPortalEnabled)
                {
                    return PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", PortalSettings.Current.PortalId, 0);
                }
                return HostController.Instance.GetInteger("SMTPMaxIdleTime", 0);
            }
        }
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether Exceptions are rethrown
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool ThrowCBOExceptions
        {
            get
            {
                return HostController.Instance.GetBoolean("ThrowCBOExceptions", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether Friendly Urls is Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool UseFriendlyUrls
        {
            get
            {
                return HostController.Instance.GetBoolean("UseFriendlyUrls", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets whether Custom Error Messages is Enabled
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool UseCustomErrorMessages
        {
            get
            {
                return HostController.Instance.GetBoolean("UseCustomErrorMessages", false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the User Quota
        /// </summary>
        /// <remarks>
        ///   Defaults to 0
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int UserQuota
        {
            get
            {
                return HostController.Instance.GetInteger("UserQuota", 0);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the window to use in minutes when determining if the user is online
        /// </summary>
        /// <remarks>
        ///   Defaults to 15
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int UsersOnlineTimeWindow
        {
            get
            {
                return HostController.Instance.GetInteger("UsersOnlineTime", 15);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the WebRequest Timeout value
        /// </summary>
        /// <remarks>
        ///   Defaults to 10000
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int WebRequestTimeout
        {
            get
            {
                return HostController.Instance.GetInteger("WebRequestTimeout", 10000);
            }
        }

        /// <summary>
        ///   Gets whether to use the minified or debug version of the jQuery scripts
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        [Obsolete("This is managed through the JavaScript Library package")]
        public static bool jQueryDebug
        {
            get
            {
                return HostController.Instance.GetBoolean("jQueryDebug", false);
            }
        }

        /// <summary>
        ///   Gets whether to use a hosted version of the jQuery script file
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        [Obsolete("This is managed through the JavaScript Library package")]
        public static bool jQueryHosted
        {
            get
            {
                return HostController.Instance.GetBoolean("jQueryHosted", false);
            }
        }

        /// <summary>
        ///   Gets the Url for a hosted version of jQuery
        /// </summary>
        /// <remarks>
        ///   Defaults to the DefaultHostedUrl constant in the jQuery class.
        ///   The framework will default to the latest released 1.x version hosted on Google.
        /// </remarks>
        [Obsolete("This is managed through the JavaScript Library package")]
        public static string jQueryUrl
        {
            get
            {
                if (HttpContext.Current.Request.IsSecureConnection)
                {
                    return HostController.Instance.GetString("jQueryUrl", jQuery.DefaultHostedUrl).Replace("http://", "https://");
                }
                else
                {
                    return HostController.Instance.GetString("jQueryUrl", jQuery.DefaultHostedUrl);
                }
            }
        }

		/// <summary>
		///   Gets the Url for a hosted version of jQuery Migrate plugin.
		/// </summary>
		/// <remarks>
		///   Defaults to the DefaultHostedUrl constant in the jQuery class.
		///   The framework will default to the latest released 1.x version hosted on Google.
		/// </remarks>
		[Obsolete("This is managed through the JavaScript Library package")]
		public static string jQueryMigrateUrl
		{
			get
			{
				if (HttpContext.Current.Request.IsSecureConnection)
				{
					return HostController.Instance.GetString("jQueryMigrateUrl", string.Empty).Replace("http://", "https://");
				}
				else
				{
					return HostController.Instance.GetString("jQueryMigrateUrl", string.Empty);
				}
			}
		}

        /// <summary>
        ///   Gets the Url for a hosted version of jQuery UI
        /// </summary>
        /// <remarks>
        ///   Defaults to the DefaultUIHostedUrl constant in the jQuery class.
        ///   The framework will default to the latest released 1.x version hosted on Google.
        /// </remarks>
        [Obsolete("This is managed through the JavaScript Library package")]
        public static string jQueryUIUrl
        {
            get
            {
                if (HttpContext.Current.Request.IsSecureConnection)
                {
                    return HostController.Instance.GetString("jQueryUIUrl", jQuery.DefaultUIHostedUrl).Replace("http://", "https://");
                }
                else
                {
                    return HostController.Instance.GetString("jQueryUIUrl", jQuery.DefaultUIHostedUrl);
                }
            }
        }

		/// <summary>
		///   Gets whether to use a hosted version of the MS Ajax Library
		/// </summary>
		/// <remarks>
		///   Defaults to False
		/// </remarks>
	    public static bool EnableMsAjaxCdn
	    {
		    get
		    {
				return HostController.Instance.GetBoolean("EnableMsAjaxCDN", false);
		    }
	    }

		/// <summary>
		///   Gets whether to use a hosted version of the Telerik Library
		/// </summary>
		/// <remarks>
		///   Defaults to False
		/// </remarks>
		[Obsolete("Not used anymore")]
		public static bool EnableTelerikCdn
		{
			get
			{
				return HostController.Instance.GetBoolean("EnableTelerikCDN", false);
			}
		}

        /// <summary>
        /// Get Telerik CDN Basic Path.
        /// </summary>
        [Obsolete("Not used anymore")]
        public static string TelerikCdnBasicUrl
	    {
			get
			{
				return HostController.Instance.GetString("TelerikCDNBasicUrl");
			}
	    }

        /// <summary>
        /// Get Telerik CDN Secure Path.
        /// </summary>
        [Obsolete("Not used anymore")]
        public static string TelerikCdnSecureUrl
		{
			get
			{
				return HostController.Instance.GetString("TelerikCDNSecureUrl");
			}
		}

		/// <summary>
		/// Get the time, in seconds, before asynchronous postbacks time out if no response is received.
		/// </summary>
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

        /// <summary>
        /// Get a value indicating whether to put the entire instance into maintenance mode
        /// </summary>
        public static bool IsLocked
        {
            get { return HostController.Instance.GetBoolean("IsLocked", false); }
        }

        /// <summary>
        /// Get the ID of the user that locked the entire instance
        /// </summary>
        public static int LockedByUserId
        {
            get { return HostController.Instance.GetInteger("LockedByUserId", Null.NullInteger); }
        }


        #endregion
    }
}
