#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Collections;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Framework;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Internals;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.Common;
using DotNetNuke.Web.UI.WebControls;
using DotNetNuke.Web.UI.WebControls.Extensions;

using System.Globalization;
using System.Web;

using DotNetNuke.Web.Client;

using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DesktopModules.Admin.Portals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SiteSettings PortalModuleBase is used to edit the main settings for a 
    /// portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/8/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class SiteSettings : PortalModuleBase
    {

        #region Private Members
        private IEnumerable<IEditPagePanelExtensionPoint> advancedSettingsExtensions;

        private int _portalId = -1;
        
        private string SelectedCultureCode
        {
            get
            {
                return LocaleController.Instance.GetCurrentLocale(PortalId).Code;
            }
        }

        #endregion

        #region Public Properties

        protected string CustomRegistrationFields { get; set; }

        #endregion

        #region Private Methods

        private void BindAliases(PortalInfo portal)
        {
            var portalSettings = new PortalSettings(portal);

            var portalAliasMapping = portalSettings.PortalAliasMappingMode.ToString().ToUpper();
            if (String.IsNullOrEmpty(portalAliasMapping))
            {
                portalAliasMapping = "CANONICALURL";
            }
            portalAliasModeButtonList.Select(portalAliasMapping, false);

            //Auto Add Portal Alias
            //if (Config.GetFriendlyUrlProvider() == "advanced")
            //{
            //    autoAddAlias.Visible = false;
            //}
            //else
            //{
                autoAddAlias.Visible = true;
                if (new PortalController().GetPortals().Count > 1)
                {
                    chkAutoAddPortalAlias.Enabled = false;
                    chkAutoAddPortalAlias.Checked = false;
                }
                else
                {
                    chkAutoAddPortalAlias.Checked = HostController.Instance.GetBoolean("AutoAddPortalAlias");
                }
            //}

        }

        private void BindDesktopModules()
        {
            var dicModules = DesktopModuleController.GetDesktopModules(Null.NullInteger);
            var dicPortalDesktopModules = DesktopModuleController.GetPortalDesktopModulesByPortalID(_portalId);

            ctlDesktopModules.Items.Clear();

            foreach (var objDicModule in dicModules.Values)
            {
                DnnComboBoxItem comboBoxItem = new DnnComboBoxItem(objDicModule.ModuleName, objDicModule.DesktopModuleID.ToString());
                foreach (var objPortalDesktopModule in dicPortalDesktopModules.Values)
                {
                    if (objPortalDesktopModule.DesktopModuleID == objDicModule.DesktopModuleID)
                    {
                        comboBoxItem.Checked = true;
                        break;
                    }
                }

                ctlDesktopModules.Items.Add(comboBoxItem);
            }
        }

        private void BindDetails(PortalInfo portal)
        {
            if (portal != null)
            {
                txtPortalName.Text = portal.PortalName;
                txtDescription.Text = portal.Description;
                txtKeyWords.Text = portal.KeyWords;
                lblGUID.Text = portal.GUID.ToString().ToUpper();
                txtFooterText.Text = portal.FooterText;
            }
        }

        private void BindHostSettings(PortalInfo portal)
        {
            if (!Null.IsNull(portal.ExpiryDate))
            {
                datepickerExpiryDate.SelectedDate = portal.ExpiryDate;
            }
            txtHostFee.Text = portal.HostFee.ToString();
            txtHostSpace.Text = portal.HostSpace.ToString();
            txtPageQuota.Text = portal.PageQuota.ToString();
            txtUserQuota.Text = portal.UserQuota.ToString();
            if (portal.SiteLogHistory != Null.NullInteger)
            {
                txtSiteLogHistory.Text = portal.SiteLogHistory.ToString();
            }
        }

        private void BindMarketing(PortalInfo portal)
        {
            //Load DocTypes
            var searchEngines = new Dictionary<string, string>
                               {
                                   { "Google", "http://www.google.com/addurl?q=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(Request))) }, 
                                   { "Yahoo", "http://siteexplorer.search.yahoo.com/submit" }, 
                                   { "Microsoft", "http://search.msn.com.sg/docs/submit.aspx" }
                               };

            cboSearchEngine.DataSource = searchEngines;
            cboSearchEngine.DataBind();

            string portalAlias = !String.IsNullOrEmpty(PortalSettings.DefaultPortalAlias) 
                                ? PortalSettings.DefaultPortalAlias 
                                : PortalSettings.PortalAlias.HTTPAlias;
            txtSiteMap.Text = Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx";

            optBanners.SelectedIndex = portal.BannerAdvertising;
            if (UserInfo.IsSuperUser)
            {
                lblBanners.Visible = false;
            }
            else
            {
                optBanners.Enabled = portal.BannerAdvertising != 2;
                lblBanners.Visible = portal.BannerAdvertising == 2;
            }

        }

        private void BindMessaging(PortalInfo portal)
        {
            var throttlingIntervals = new Dictionary<int, int>
                               {
                                   { 0, 0 }, { 1, 1 },  { 2, 2 },  { 3, 3 },  { 4, 4 },  { 5, 5 },  { 6, 6 },  { 7, 7 },  { 8, 8 },  { 9, 9 }, { 10, 10 }
                               };

            cboMsgThrottlingInterval.DataSource = throttlingIntervals;
            cboMsgThrottlingInterval.DataBind(PortalController.GetPortalSettingAsInteger("MessagingThrottlingInterval", portal.PortalID, 0).ToString());

            var recipientLimits = new Dictionary<int, int>
                               {
                                   { 1, 1 }, { 5, 5 },  { 10, 10 },  { 15, 15 },  { 25, 25 },  { 50, 50 },  { 75, 75 },  { 100, 100 }
                               };

            cboMsgRecipientLimit.DataSource = recipientLimits;
            cboMsgRecipientLimit.DataBind(PortalController.GetPortalSettingAsInteger("MessagingRecipientLimit", portal.PortalID, 5).ToString());

            optMsgAllowAttachments.Select(PortalController.GetPortalSetting("MessagingAllowAttachments", portal.PortalID, "NO"), false);
            optMsgProfanityFilters.Select(PortalController.GetPortalSetting("MessagingProfanityFilters", portal.PortalID, "NO"), false);
            optMsgSendEmail.Select(PortalController.GetPortalSetting("MessagingSendEmail", portal.PortalID, "YES"), false);        
        }

        private void BindPages(PortalInfo portal, string activeLanguage)
        {
            //Set up special page lists
            List<TabInfo> listTabs = TabController.GetPortalTabs(TabController.GetTabsBySortOrder(portal.PortalID, activeLanguage, true),
                                                                 Null.NullInteger,
                                                                 true,
                                                                 "<" + Localization.GetString("None_Specified") + ">",
                                                                 true,
                                                                 true,
                                                                 false,
                                                                 false,
                                                                 false);

            if (portal.SplashTabId > 0)
            {
                cboSplashTabId.SelectedPage = listTabs.SingleOrDefault(t => t.TabID == portal.SplashTabId);
            }

            cboSplashTabId.PortalId = portal.PortalID;

            if (portal.HomeTabId > 0)
            {
                cboHomeTabId.SelectedPage = listTabs.SingleOrDefault(t => t.TabID == portal.HomeTabId);
            }

            cboHomeTabId.PortalId = portal.PortalID;

            cboLoginTabId.DataSource = listTabs.Where(t => (t.TabID > 0 && Globals.ValidateLoginTabID(t.TabID)) || t.TabID == Null.NullInteger).ToList();
            cboLoginTabId.DataBind(portal.LoginTabId.ToString(CultureInfo.InvariantCulture));

            if (portal.RegisterTabId > 0)
            {
                cboRegisterTabId.SelectedPage = listTabs.SingleOrDefault(t => t.TabID == portal.RegisterTabId);
            }

            cboRegisterTabId.PortalId = portal.PortalID;

            cboSearchTabId.DataSource = listTabs.Where(t => (t.TabID > 0 && Globals.ValidateModuleInTab(t.TabID, "Search Results")) || t.TabID == Null.NullInteger).ToList();
            cboSearchTabId.DataBind(portal.SearchTabId.ToString(CultureInfo.InvariantCulture));

            pagesExtensionPoint.BindAction(portal.PortalID, -1, -1);

            if (portal.UserTabId > 0)
            {
                listTabs = TabController.GetPortalTabs(portal.PortalID, Null.NullInteger, false, true);
                cboUserTabId.SelectedPage = listTabs.SingleOrDefault(t => t.TabID == portal.UserTabId);
            }

            cboUserTabId.PortalId = portal.PortalID;
        }
        
        private void BindPaymentProcessor(PortalInfo portal)
        {
            var listController = new ListController();
            currencyCombo.DataSource = listController.GetListEntryInfoItems("Currency", "");
            var currency = portal.Currency;
            if (String.IsNullOrEmpty(currency))
            {
                currency = "USD";
            }
            currencyCombo.DataBind(currency);

            processorCombo.DataSource = listController.GetListEntryInfoItems("Processor", "");
            processorCombo.DataBind();
            processorCombo.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "");
            processorCombo.Select(Host.PaymentProcessor, true);

            // use sandbox?
            var usePayPalSandbox = Boolean.Parse(PortalController.GetPortalSetting("paypalsandbox", portal.PortalID, "false"));
            chkPayPalSandboxEnabled.Checked = usePayPalSandbox;
            processorLink.NavigateUrl = usePayPalSandbox ? "https://developer.paypal.com" : Globals.AddHTTP(processorCombo.SelectedItem.Value);

            txtUserId.Text = portal.ProcessorUserId;

            // return url after payment or on cancel
            var strPayPalReturnUrl = PortalController.GetPortalSetting("paypalsubscriptionreturn", portal.PortalID, Null.NullString);
            txtPayPalReturnURL.Text = strPayPalReturnUrl;
            var strPayPalCancelUrl = PortalController.GetPortalSetting("paypalsubscriptioncancelreturn", portal.PortalID, Null.NullString);
            txtPayPalCancelURL.Text = strPayPalCancelUrl;
        }

        private void BindPortal(int portalId, string activeLanguage)
        {
            var portalController = new PortalController();
            var portal = portalController.GetPortal(portalId, activeLanguage);

            if (Page.IsPostBack == false)
            {
                //Ensure localization
                DataProvider.Instance().EnsureLocalizationExists(portalId, activeLanguage);


                BindDetails(portal);

                BindMarketing(portal);

                ctlLogo.FilePath = portal.LogoFile;
                ctlLogo.FileFilter = Globals.glbImageFileTypes;
                ctlBackground.FilePath = portal.BackgroundFile;
                ctlBackground.FileFilter = Globals.glbImageFileTypes;
                ctlFavIcon.FilePath = new FavIcon(portal.PortalID).GetSettingPath();
                chkSkinWidgestEnabled.Checked = PortalController.GetPortalSettingAsBoolean("EnableSkinWidgets", portalId, true);

                BindSkins(portal);

                BindPages(portal, activeLanguage);

                lblHomeDirectory.Text = portal.HomeDirectory;

                optUserRegistration.SelectedIndex = portal.UserRegistration;
                chkEnableRegisterNotification.Checked = PortalController.GetPortalSettingAsBoolean("EnableRegisterNotification", portalId, true);

                BindPaymentProcessor(portal);

                BindUsability(portal);

                BindMessaging(portal);

                var roleController = new RoleController();
                cboAdministratorId.DataSource = roleController.GetUserRoles(portalId, null, portal.AdministratorRoleName);
                cboAdministratorId.DataBind(portal.AdministratorId.ToString());

                //PortalSettings for portal being edited
                var portalSettings = new PortalSettings(portal);

                chkHideLoginControl.Checked = portalSettings.HideLoginControl;

                cboTimeZone.DataBind(portalSettings.TimeZone.Id);


                if (UserInfo.IsSuperUser)
                {
                    BindAliases(portal);

                    BindSSLSettings(portal);

                    BindHostSettings(portal);

                }

                BindUrlSettings(portal);

                SiteSettingAdvancedSettingExtensionControl.BindAction(portalId, TabId, ModuleId);
                SiteSettingsTabExtensionControl.BindAction(portalId, TabId, ModuleId);

                LoadStyleSheet(portal);

                ctlAudit.Entity = portal;

                var overrideDefaultSettings = Boolean.Parse(PortalController.GetPortalSetting(ClientResourceSettings.OverrideDefaultSettingsKey, portalId, "false"));
                chkOverrideDefaultSettings.Checked = overrideDefaultSettings;
                BindClientResourceManagementUi(portal.PortalID, overrideDefaultSettings);
                ManageMinificationUi();
            }

            BindUserAccountSettings(portal, activeLanguage);
        }

        private void BindClientResourceManagementUi(int portalId, bool overrideDefaultSettings)
        {
            EnableCompositeFilesRow.Visible = overrideDefaultSettings;
            CrmVersionRow.Visible = overrideDefaultSettings;
            MinifyCssRow.Visible = overrideDefaultSettings;
            MinifyJsRow.Visible = overrideDefaultSettings;
            DebugEnabledRow.Visible = HttpContext.Current.IsDebuggingEnabled;
            
            // set up host settings information
            var hostVersion = HostController.Instance.GetInteger(ClientResourceSettings.VersionKey, 1).ToString(CultureInfo.InvariantCulture);
            var hostEnableCompositeFiles = HostController.Instance.GetBoolean(ClientResourceSettings.EnableCompositeFilesKey, false);
            var hostEnableMinifyCss = HostController.Instance.GetBoolean(ClientResourceSettings.MinifyCssKey, false);
            var hostEnableMinifyJs = HostController.Instance.GetBoolean(ClientResourceSettings.MinifyJsKey, false);

            string yes = Localization.GetString("Yes.Text", Localization.SharedResourceFile);
            string no = Localization.GetString("No.Text", Localization.SharedResourceFile);

            CrmHostSettingsSummary.Text = string.Format(LocalizeString("CrmHostSettingsSummary"),
                hostVersion, // {0} = version
                hostEnableCompositeFiles ? yes : no, // {1} = enable composite files
                hostEnableMinifyCss ? yes : no, // {2} = minify css
                hostEnableMinifyJs ? yes : no); // {3} = minify js

            // set up UI for portal-specific options
            if (overrideDefaultSettings)
            {
                chkEnableCompositeFiles.Checked = Boolean.Parse(PortalController.GetPortalSetting(ClientResourceSettings.EnableCompositeFilesKey, portalId, "false"));
                chkMinifyCss.Checked = Boolean.Parse(PortalController.GetPortalSetting(ClientResourceSettings.MinifyCssKey, portalId, "false"));
                chkMinifyJs.Checked = Boolean.Parse(PortalController.GetPortalSetting(ClientResourceSettings.MinifyJsKey, portalId, "false"));

                var settingValue = PortalController.GetPortalSetting(ClientResourceSettings.VersionKey, portalId, "0");
                int version;
                if (int.TryParse(settingValue, out version))
                {
                    if (version == 0)
                    {
                        version = 1;
                        PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.VersionKey, "1", true);
                    }
                    CrmVersionLabel.Text = version.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void BindSkins(PortalInfo portal)
        {
            var skins = SkinController.GetSkins(portal, SkinController.RootSkin, SkinScope.All)
                                                     .ToDictionary(skin => skin.Key, skin => skin.Value);
            var containers = SkinController.GetSkins(portal, SkinController.RootContainer, SkinScope.All)
                                                    .ToDictionary(skin => skin.Key, skin => skin.Value);
            portalSkinCombo.DataSource = skins;
            portalSkinCombo.DataBind(PortalController.GetPortalSetting("DefaultPortalSkin", portal.PortalID, Host.DefaultPortalSkin));

            portalContainerCombo.DataSource = containers;
            portalContainerCombo.DataBind(PortalController.GetPortalSetting("DefaultPortalContainer", portal.PortalID, Host.DefaultPortalContainer));

            editSkinCombo.DataSource = skins;
            editSkinCombo.DataBind(PortalController.GetPortalSetting("DefaultAdminSkin", portal.PortalID, Host.DefaultAdminSkin));

            editContainerCombo.DataSource = containers;
            editContainerCombo.DataBind(PortalController.GetPortalSetting("DefaultAdminContainer", portal.PortalID, Host.DefaultAdminContainer));

            if (ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                uploadSkinLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "");

                if (PortalSettings.EnablePopUps)
                {
                    uploadSkinLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(uploadSkinLink.NavigateUrl, this, PortalSettings, true, false));
                }
            }
            else
            {
                uploadSkinLink.Visible = false;
            }
        }

        private void BindSSLSettings(PortalInfo portal)
        {
            chkSSLEnabled.Checked = PortalController.GetPortalSettingAsBoolean("SSLEnabled", portal.PortalID, false);
            chkSSLEnforced.Checked = PortalController.GetPortalSettingAsBoolean("SSLEnforced", portal.PortalID, false);
            txtSSLURL.Text = PortalController.GetPortalSetting("SSLURL", portal.PortalID, Null.NullString);
            txtSTDURL.Text = PortalController.GetPortalSetting("STDURL", portal.PortalID, Null.NullString);
        }
        
        private void BindUrlSettings(PortalInfo portal)
        {
            if (Config.GetFriendlyUrlProvider() == "advanced")
            {
                var urlSettings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(portal.PortalID);
                redirectOldProfileUrls.Checked = urlSettings.RedirectOldProfileUrl;
            }
        }

        private void BindUsability(PortalInfo portal)
        {
            //PortalSettings for portal being edited
            var portalSettings = new PortalSettings(portal);
            chkInlineEditor.Checked = portalSettings.InlineEditorEnabled;
            enablePopUpsCheckBox.Checked = portalSettings.EnablePopUps;
            chkHideSystemFolders.Checked = portalSettings.HideFoldersEnabled;

            var mode = (portalSettings.DefaultControlPanelMode == PortalSettings.Mode.Edit) ? "EDIT" : "VIEW";
            optControlPanelMode.Select(mode, false);

            optControlPanelVisibility.Select(PortalController.GetPortalSetting("ControlPanelVisibility", portal.PortalID, "MAX"), false);

            optControlPanelSecurity.Select(PortalController.GetPortalSetting("ControlPanelSecurity", portal.PortalID, "MODULE"), false);
        }

        private void BindUserAccountSettings(PortalInfo portal, string activeLanguage)
        {
            if (!Page.IsPostBack)
            {
                var settings = UserController.GetUserSettings(portal.PortalID);

                basicRegistrationSettings.DataSource = settings;
                basicRegistrationSettings.DataBind();

                var setting = PortalController.GetPortalSettingAsInteger("Registration_RegistrationFormType", portal.PortalID, 0);
                registrationFormType.Select(setting.ToString(CultureInfo.InvariantCulture));

                standardRegistrationSettings.DataSource = settings;
                standardRegistrationSettings.DataBind();

                validationRegistrationSettings.DataSource = settings;
                validationRegistrationSettings.DataBind();

                var customRegistrationFields = PortalController.GetPortalSetting("Registration_RegistrationFields", portal.PortalID, String.Empty);

                CustomRegistrationFields = BuildCustomRegistrationFields(customRegistrationFields);

                passwordRegistrationSettings.DataSource = settings;
                passwordRegistrationSettings.DataBind();

                otherRegistrationSettings.DataSource = settings;
                otherRegistrationSettings.DataBind();

                //Set up special page lists
                List<TabInfo> listTabs = TabController.GetPortalTabs(TabController.GetTabsBySortOrder(portal.PortalID, activeLanguage, true),
                                                                     Null.NullInteger,
                                                                     true,
                                                                     "<" + Localization.GetString("None_Specified") + ">",
                                                                     true,
                                                                     false,
                                                                     false,
                                                                     false,
                                                                     false);

                var tabs = listTabs.Where(t => t.DisableLink == false).ToList();

                //using values from current portal
                var redirectTab = PortalController.GetPortalSettingAsInteger("Redirect_AfterRegistration", portal.PortalID, 0);
                if (redirectTab > 0)
                {
                    RedirectAfterRegistration.SelectedPage = tabs.SingleOrDefault(t => t.TabID == redirectTab);
                }
                RedirectAfterRegistration.PortalId = portal.PortalID;

                RequiresUniqueEmailLabel.Text = MembershipProviderConfig.RequiresUniqueEmail.ToString(CultureInfo.InvariantCulture);
                PasswordFormatLabel.Text = MembershipProviderConfig.PasswordFormat.ToString();
                PasswordRetrievalEnabledLabel.Text = MembershipProviderConfig.PasswordRetrievalEnabled.ToString(CultureInfo.InvariantCulture);
                PasswordResetEnabledLabel.Text = MembershipProviderConfig.PasswordResetEnabled.ToString(CultureInfo.InvariantCulture);
                MinPasswordLengthLabel.Text = MembershipProviderConfig.MinPasswordLength.ToString(CultureInfo.InvariantCulture);
                MinNonAlphanumericCharactersLabel.Text = MembershipProviderConfig.MinNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture);
                RequiresQuestionAndAnswerLabel.Text = MembershipProviderConfig.RequiresQuestionAndAnswer.ToString(CultureInfo.InvariantCulture);
                PasswordStrengthRegularExpressionLabel.Text = MembershipProviderConfig.PasswordStrengthRegularExpression;
                MaxInvalidPasswordAttemptsLabel.Text = MembershipProviderConfig.MaxInvalidPasswordAttempts.ToString(CultureInfo.InvariantCulture);
                PasswordAttemptWindowLabel.Text = MembershipProviderConfig.PasswordAttemptWindow.ToString(CultureInfo.InvariantCulture);

                loginSettings.DataSource = settings;
                loginSettings.DataBind();

                //using values from current portal
                redirectTab = PortalController.GetPortalSettingAsInteger("Redirect_AfterLogin", portal.PortalID, 0);
                if (redirectTab > 0)
                {
                    RedirectAfterLogin.SelectedPage = tabs.SingleOrDefault(t => t.TabID == redirectTab);
                }
                RedirectAfterLogin.PortalId = portal.PortalID;

                //using values from current portal
                redirectTab = PortalController.GetPortalSettingAsInteger("Redirect_AfterLogout", portal.PortalID, 0);

                if (redirectTab > 0)
                {
                    RedirectAfterLogout.SelectedPage = tabs.SingleOrDefault(t => t.TabID == redirectTab);
                }
                RedirectAfterLogout.PortalId = portal.PortalID;

                //This needs to be kept explicit to avoid naming conflicts ewith iFinity
                var urlSettings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(portal.PortalID);

                VanityUrlAlias.Text = String.Format("{0}/", PortalSettings.PortalAlias.HTTPAlias);
                vanilyUrlPrefixTextBox.Text = urlSettings.VanityUrlPrefix;
                VanityUrlExample.Text = String.Format("/{0}", LocalizeString("VanityUrlExample"));

                userVisiblity.EnumType = "DotNetNuke.Entities.Users.UserVisibilityMode, DotNetNuke";
                profileSettings.DataSource = settings;
                profileSettings.DataBind();
            }
            else
            {
                CustomRegistrationFields = BuildCustomRegistrationFields(registrationFields.Text);
            }
            passwordSettings.EditMode = UserInfo.IsSuperUser ? PropertyEditorMode.Edit : PropertyEditorMode.View;
            passwordSettings.LocalResourceFile = LocalResourceFile;
            passwordSettings.DataSource = new PasswordConfig();
            passwordSettings.DataBind();

        }

        private string BuildCustomRegistrationFields(string customRegistrationFields)
        {
            if (!String.IsNullOrEmpty(customRegistrationFields))
            {
                var sb = new StringBuilder();
                sb.Append("[ ");
                int i = 0;
                foreach (var field in customRegistrationFields.Split(','))
                {
                    if (i != 0) sb.Append(",");
                    sb.Append("{ id: \"" + field + "\", name: \"" + field + "\"}");
                    i++;
                }
                sb.Append(" ]");
                return sb.ToString();
            }

            return "null";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadStyleSheet loads the stylesheet
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/8/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void LoadStyleSheet(PortalInfo portalInfo)
        {
            string uploadDirectory = "";
            if (portalInfo != null)
            {
                uploadDirectory = portalInfo.HomeDirectoryMapPath;
            }

            //read CSS file
            if (File.Exists(uploadDirectory + "portal.css"))
            {
                using (var text = File.OpenText(uploadDirectory + "portal.css"))
                {
                    txtStyleSheet.Text = text.ReadToEnd();
                }
            }
        }

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatCurrency formats the currency.
        /// control.
        /// </summary>
        /// <returns>A formatted string</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/8/2004	Modified
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FormatCurrency()
        {
            var retValue = "";
            try
            {
                retValue = Host.HostCurrency + " / " + Localization.GetString("Month");
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatFee formats the fee.
        /// control.
        /// </summary>
        /// <returns>A formatted string</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/8/2004	Modified
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FormatFee(object objHostFee)
        {
            var retValue = "";
            try
            {
                retValue = objHostFee != DBNull.Value ? ((float)objHostFee).ToString("#,##0.00") : "0";
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsSubscribed determines whether the portal has subscribed to the premium 
        /// control.
        /// </summary>
        /// <returns>True if Subscribed, False if not</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/8/2004	Modified
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool IsSubscribed(int portalModuleDefinitionId)
        {
            try
            {
                return Null.IsNull(portalModuleDefinitionId) == false;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                return false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsSuperUser determines whether the cuurent user is a SuperUser
        /// control.
        /// </summary>
        /// <returns>True if SuperUser, False if not</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	10/4/2004	Added
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool IsSuperUser()
        {
            return UserInfo.IsSuperUser;
        }

        protected string AddPortalAlias(string portalAlias, int portalID)
        {
            if (!String.IsNullOrEmpty(portalAlias))
            {
                if (portalAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    portalAlias = portalAlias.Remove(0, portalAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
                var objPortalAliasController = new PortalAliasController();
                var objPortalAlias = objPortalAliasController.GetPortalAlias(portalAlias, portalID);
                if (objPortalAlias == null)
                {
                    objPortalAlias = new PortalAliasInfo { PortalID = portalID, HTTPAlias = portalAlias };
                    TestablePortalAliasController.Instance.AddPortalAlias(objPortalAlias);
                }
            }
            return portalAlias;
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            chkPayPalSandboxEnabled.CheckedChanged += OnChkPayPalSandboxChanged;
            IncrementCrmVersionButton.Click += IncrementCrmVersion;
            chkOverrideDefaultSettings.CheckedChanged += OverrideDefaultSettingsChanged;
            chkEnableCompositeFiles.CheckedChanged += EnableCompositeFilesChanged;

            InitializeDropDownLists();

        }

        /// <summary>
        /// Initializes DropDownLists
        /// </summary>
        private void InitializeDropDownLists()
        {
            var undefinedItem = new ListItem(SharedConstants.Unspecified, String.Empty);
            cboSplashTabId.UndefinedItem = undefinedItem;
            cboHomeTabId.UndefinedItem = undefinedItem;
            cboRegisterTabId.UndefinedItem = undefinedItem;
            cboUserTabId.UndefinedItem = undefinedItem;
            RedirectAfterLogin.UndefinedItem = undefinedItem;
            RedirectAfterRegistration.UndefinedItem = undefinedItem;
            RedirectAfterLogout.UndefinedItem = undefinedItem;
        }

        private void EnableCompositeFilesChanged(object sender, EventArgs e)
        {
            ManageMinificationUi();
        }

        private void ManageMinificationUi()
        {
            var enableCompositeFiles = chkEnableCompositeFiles.Checked;

            if (!enableCompositeFiles)
            {
                chkMinifyCss.Checked = false;
                chkMinifyJs.Checked = false;
            }

            chkMinifyCss.Enabled = enableCompositeFiles;
            chkMinifyJs.Enabled = enableCompositeFiles;
        }

        private void OverrideDefaultSettingsChanged(object sender, EventArgs e)
        {
            BindClientResourceManagementUi(_portalId, chkOverrideDefaultSettings.Checked);
        }

        private void IncrementCrmVersion(object sender, EventArgs e)
        {
            PortalController.IncrementCrmVersion(_portalId);
            Response.Redirect(Request.RawUrl, true); // reload page
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/8/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdDelete.Click += DeletePortal;
            cmdRestore.Click += OnRestoreClick;
            cmdSave.Click += OnSaveClick;
            cmdUpdate.Click += UpdatePortal;
            cmdVerification.Click += OnVerifyClick;
            ctlDesktopModules.ItemChecked += ctlDesktopModules_ItemChecked;
            
            try
            {
                if ((Request.QueryString["pid"] != null) && (Globals.IsHostTab(PortalSettings.ActiveTab.TabID) || UserInfo.IsSuperUser))
                {
                    _portalId = Int32.Parse(Request.QueryString["pid"]);
	                cancelHyperLink.NavigateUrl = Globals.NavigateURL();
                }
                else
                {
                    _portalId = PortalId;
                    cancelHyperLink.Visible = false;
                }

				ctlLogo.PortalId = ctlBackground.PortalId = ctlFavIcon.PortalId = _portalId;

                ////this needs to execute always to the client script code is registred in InvokePopupCal
                
                BindDesktopModules();
                
                BindPortal(_portalId, SelectedCultureCode);

                if (UserInfo.IsSuperUser)
                {
                    hostSections.Visible = true;
                    cmdDelete.Visible = (_portalId != PortalId && !PortalController.IsMemberOfPortalGroup(_portalId));
                }
                else
                {
                    hostSections.Visible = false;
                    cmdDelete.Visible = false;
                }

            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }      

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the Delete LinkButton is clicked.
        /// It deletes the current portal form the Database.  It can only run in Host
        /// (SuperUser) mode
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/9/2004	Modified
        ///     [VMasanas]  9/12/2004   Move skin deassignment to DeletePortalInfo.
        ///     [jmarino]  16/06/2011   Modify redirection after deletion of portal 
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void DeletePortal(object sender, EventArgs e)
        {
            try
            {
                var objPortalController = new PortalController();
                PortalInfo objPortalInfo = objPortalController.GetPortal(_portalId);
                if (objPortalInfo != null)
                {
                    string strMessage = PortalController.DeletePortal(objPortalInfo, Globals.GetAbsoluteServerPath(Request));

                    if (string.IsNullOrEmpty(strMessage))
                    {
                        var objEventLog = new EventLogController();
                        objEventLog.AddLog("PortalName", objPortalInfo.PortalName, PortalSettings, UserId, EventLogController.EventLogType.PORTAL_DELETED);

                        //Redirect to another site
                        if (_portalId == PortalId)
                        {
                            if (!string.IsNullOrEmpty(Host.HostURL))
                            {
                                Response.Redirect(Globals.AddHTTP(Host.HostURL));
                            }
                            else
                            {
                                Response.End();
                            }
                        }
                        else
                        {
                            if (ViewState["UrlReferrer"] != null)
                            {
                                Response.Redirect(Convert.ToString(ViewState["UrlReferrer"]), true);
                            }
                            else
                            {
                                Response.Redirect(Globals.NavigateURL(), true);
                            }
                        }
                    }
                    else
                    {
                        Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRestore_Click runs when the Restore Default Stylesheet Linkbutton is clicked. 
        /// It reloads the default stylesheet (copies from _default Portal to current Portal)
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/9/2004	Modified
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnRestoreClick(object sender, EventArgs e)
        {
            try
            {
                var portalController = new PortalController();
                PortalInfo portal = portalController.GetPortal(_portalId);
                if (portal != null)
                {
                    if (File.Exists(portal.HomeDirectoryMapPath + "portal.css"))
                    {
                        //delete existing style sheet
                        File.Delete(portal.HomeDirectoryMapPath + "portal.css");
                    }

                    //copy file from Host
                    if (File.Exists(Globals.HostMapPath + "portal.css"))
                    {
                        File.Copy(Globals.HostMapPath + "portal.css", portal.HomeDirectoryMapPath + "portal.css");
                    }
                }
                LoadStyleSheet(portal);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdSave_Click runs when the Save Stylesheet Linkbutton is clicked.  It saves
        /// the edited Stylesheet
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/9/2004	Modified
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                string strUploadDirectory = "";

                var objPortalController = new PortalController();
                PortalInfo objPortal = objPortalController.GetPortal(_portalId);
                if (objPortal != null)
                {
                    strUploadDirectory = objPortal.HomeDirectoryMapPath;
                }

                //reset attributes
                if (File.Exists(strUploadDirectory + "portal.css"))
                {
                    File.SetAttributes(strUploadDirectory + "portal.css", FileAttributes.Normal);
                }

                //write CSS file
                using (var writer = File.CreateText(strUploadDirectory + "portal.css"))
                {
                    writer.WriteLine(txtStyleSheet.Text);
                }

                //Clear client resource cache
                var overrideSetting = PortalController.GetPortalSetting(ClientResourceSettings.OverrideDefaultSettingsKey, _portalId, "False");
                bool overridePortal;
                if (bool.TryParse(overrideSetting, out overridePortal))
                {
                    if (overridePortal)
                    {
                        // increment this portal version only
                        PortalController.IncrementCrmVersion(_portalId);
                    }
                    else
                    {
                        // increment host version, do not increment other portal versions though.
                        HostController.Instance.IncrementCrmVersion(false);
                    }   
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update LinkButton is clicked.
        /// It saves the current Site Settings
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/9/2004	Modified
        /// 	[aprasad]	1/17/2011	New setting AutoAddPortalAlias
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void UpdatePortal(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    var portalController = new PortalController();
                    PortalInfo existingPortal = portalController.GetPortal(_portalId);

                    string logo = String.Format("FileID={0}", ctlLogo.FileID);
                    string background = String.Format("FileID={0}", ctlBackground.FileID);

                    //Refresh if Background or Logo file have changed
                    bool refreshPage = (background == existingPortal.BackgroundFile || logo == existingPortal.LogoFile);

                    float hostFee = existingPortal.HostFee;
                    if (!String.IsNullOrEmpty(txtHostFee.Text))
                    {
                        hostFee = float.Parse(txtHostFee.Text);
                    }

                    int hostSpace = existingPortal.HostSpace;
                    if (!String.IsNullOrEmpty(txtHostSpace.Text))
                    {
                        hostSpace = int.Parse(txtHostSpace.Text);
                    }

                    int pageQuota = existingPortal.PageQuota;
                    if (!String.IsNullOrEmpty(txtPageQuota.Text))
                    {
                        pageQuota = int.Parse(txtPageQuota.Text);
                    }

                    int userQuota = existingPortal.UserQuota;
                    if (!String.IsNullOrEmpty(txtUserQuota.Text))
                    {
                        userQuota = int.Parse(txtUserQuota.Text);
                    }

                    int siteLogHistory = existingPortal.SiteLogHistory;
                    if (!String.IsNullOrEmpty(txtSiteLogHistory.Text))
                    {
                        siteLogHistory = int.Parse(txtSiteLogHistory.Text);
                    }

                    DateTime expiryDate = existingPortal.ExpiryDate;
                    if (datepickerExpiryDate.SelectedDate.HasValue)
                    {
                        expiryDate = datepickerExpiryDate.SelectedDate.Value;
                    }

                    var intSplashTabId = cboSplashTabId.SelectedItemValueAsInt;

                    var intHomeTabId = cboHomeTabId.SelectedItemValueAsInt;

                    var intLoginTabId = Null.NullInteger;
                    if (cboLoginTabId.SelectedItem != null)
                    {
                        int.TryParse(cboLoginTabId.SelectedItem.Value, out intLoginTabId);
                    }

                    var intRegisterTabId = cboRegisterTabId.SelectedItemValueAsInt;

                    var intUserTabId = cboUserTabId.SelectedItemValueAsInt;

                    var intSearchTabId = Null.NullInteger;
                    if (cboSearchTabId.SelectedItem != null)
                    {
                        int.TryParse(cboSearchTabId.SelectedItem.Value, out intSearchTabId);
                    }

                    var portal = new PortalInfo
                                            {
                                                PortalID = _portalId,
                                                PortalGroupID = existingPortal.PortalGroupID,
                                                PortalName = txtPortalName.Text,
                                                LogoFile = logo,
                                                FooterText = txtFooterText.Text,
                                                ExpiryDate = expiryDate,
                                                UserRegistration = optUserRegistration.SelectedIndex,
                                                BannerAdvertising = optBanners.SelectedIndex,
                                                Currency = currencyCombo.SelectedItem.Value,
                                                AdministratorId = Convert.ToInt32(cboAdministratorId.SelectedItem.Value),
                                                HostFee = hostFee,
                                                HostSpace = hostSpace,
                                                PageQuota = pageQuota,
                                                UserQuota = userQuota,
                                                PaymentProcessor =
                                                    String.IsNullOrEmpty(processorCombo.SelectedValue)
                                                        ? ""
                                                        : processorCombo.SelectedItem.Text,
                                                ProcessorUserId = txtUserId.Text,
                                                ProcessorPassword = !string.IsNullOrEmpty(txtPassword.Text) ? txtPassword.Text : existingPortal.ProcessorPassword,
                                                Description = txtDescription.Text,
                                                KeyWords = txtKeyWords.Text,
                                                BackgroundFile = background,
                                                SiteLogHistory = siteLogHistory,
                                                SplashTabId = intSplashTabId,
                                                HomeTabId = intHomeTabId,
                                                LoginTabId = intLoginTabId,
                                                RegisterTabId = intRegisterTabId,
                                                UserTabId = intUserTabId,
                                                SearchTabId = intSearchTabId,
                                                DefaultLanguage = existingPortal.DefaultLanguage,
                                                HomeDirectory = lblHomeDirectory.Text,
                                                CultureCode = SelectedCultureCode
                                            };
                    portalController.UpdatePortalInfo(portal);

                    if (!refreshPage)
                    {
                        refreshPage = (PortalSettings.DefaultAdminSkin == editSkinCombo.SelectedValue) ||
                                        (PortalSettings.DefaultAdminContainer == editContainerCombo.SelectedValue);
                    }

                    PortalController.UpdatePortalSetting(_portalId, ClientResourceSettings.OverrideDefaultSettingsKey, chkOverrideDefaultSettings.Checked.ToString(CultureInfo.InvariantCulture), false);
                    PortalController.UpdatePortalSetting(_portalId, ClientResourceSettings.EnableCompositeFilesKey, chkEnableCompositeFiles.Checked.ToString(CultureInfo.InvariantCulture), false);
                    PortalController.UpdatePortalSetting(_portalId, ClientResourceSettings.MinifyCssKey, chkMinifyCss.Checked.ToString(CultureInfo.InvariantCulture), false);
                    PortalController.UpdatePortalSetting(_portalId, ClientResourceSettings.MinifyJsKey, chkMinifyJs.Checked.ToString(CultureInfo.InvariantCulture), false);

                    PortalController.UpdatePortalSetting(_portalId, "EnableSkinWidgets", chkSkinWidgestEnabled.Checked.ToString(), false);
                    PortalController.UpdatePortalSetting(_portalId, "DefaultAdminSkin", editSkinCombo.SelectedValue, false);
                    PortalController.UpdatePortalSetting(_portalId, "DefaultPortalSkin", portalSkinCombo.SelectedValue, false);
                    PortalController.UpdatePortalSetting(_portalId, "DefaultAdminContainer", editContainerCombo.SelectedValue, false);
                    PortalController.UpdatePortalSetting(_portalId, "DefaultPortalContainer", portalContainerCombo.SelectedValue, false);
                    PortalController.UpdatePortalSetting(_portalId, "EnablePopups", enablePopUpsCheckBox.Checked.ToString(), false);
                    PortalController.UpdatePortalSetting(_portalId, "InlineEditorEnabled", chkInlineEditor.Checked.ToString(), false);
                    PortalController.UpdatePortalSetting(_portalId, "HideFoldersEnabled", chkHideSystemFolders.Checked.ToString(), false);
                    PortalController.UpdatePortalSetting(_portalId, "ControlPanelMode", optControlPanelMode.SelectedItem.Value, false);
                    PortalController.UpdatePortalSetting(_portalId, "ControlPanelVisibility", optControlPanelVisibility.SelectedItem.Value, false);
                    PortalController.UpdatePortalSetting(_portalId, "ControlPanelSecurity", optControlPanelSecurity.SelectedItem.Value, false);

                    PortalController.UpdatePortalSetting(_portalId, "MessagingThrottlingInterval", cboMsgThrottlingInterval.SelectedItem.Value, false);
                    PortalController.UpdatePortalSetting(_portalId, "MessagingRecipientLimit", cboMsgRecipientLimit.SelectedItem.Value, false);
                    PortalController.UpdatePortalSetting(_portalId, "MessagingAllowAttachments", optMsgAllowAttachments.SelectedItem.Value, false);
                    PortalController.UpdatePortalSetting(_portalId, "MessagingProfanityFilters", optMsgProfanityFilters.SelectedItem.Value, false);
                    PortalController.UpdatePortalSetting(_portalId, "MessagingSendEmail", optMsgSendEmail.SelectedItem.Value, false);

                    PortalController.UpdatePortalSetting(_portalId, "paypalsandbox", chkPayPalSandboxEnabled.Checked.ToString(), false);
                    PortalController.UpdatePortalSetting(_portalId, "paypalsubscriptionreturn", txtPayPalReturnURL.Text, false);
                    PortalController.UpdatePortalSetting(_portalId, "paypalsubscriptioncancelreturn", txtPayPalCancelURL.Text, false);
                    PortalController.UpdatePortalSetting(_portalId, "TimeZone", cboTimeZone.SelectedValue, false);

					PortalController.UpdatePortalSetting(_portalId, "HideLoginControl", chkHideLoginControl.Checked.ToString(), false);
					PortalController.UpdatePortalSetting(_portalId, "EnableRegisterNotification", chkEnableRegisterNotification.Checked.ToString(), false);

                    pagesExtensionPoint.SaveAction(_portalId, -1, -1);

                    SiteSettingAdvancedSettingExtensionControl.SaveAction(_portalId, TabId, ModuleId);
                    SiteSettingsTabExtensionControl.SaveAction(_portalId, TabId, ModuleId);

                    if (Config.GetFriendlyUrlProvider() == "advanced")
                    {
						PortalController.UpdatePortalSetting(_portalId, DotNetNuke.Entities.Urls.FriendlyUrlSettings.RedirectOldProfileUrlSetting, redirectOldProfileUrls.Checked ? "Y" : "N", false);
                    }

                    new FavIcon(_portalId).Update(ctlFavIcon.FileID);

                    if (IsSuperUser())
                    {
                        PortalController.UpdatePortalSetting(_portalId, "PortalAliasMapping", portalAliasModeButtonList.SelectedValue, false);
                        HostController.Instance.Update("AutoAddPortalAlias", chkAutoAddPortalAlias.Checked ? "Y" : "N", true);

                        PortalController.UpdatePortalSetting(_portalId, "SSLEnabled", chkSSLEnabled.Checked.ToString(), false);
                        PortalController.UpdatePortalSetting(_portalId, "SSLEnforced", chkSSLEnforced.Checked.ToString(), false);
                        PortalController.UpdatePortalSetting(_portalId, "SSLURL", AddPortalAlias(txtSSLURL.Text, _portalId), false);
                        PortalController.UpdatePortalSetting(_portalId, "STDURL", AddPortalAlias(txtSTDURL.Text, _portalId), false);
                    }

                    if(registrationFormType.SelectedValue == "1")
                    {
                        var setting = registrationFields.Text;
                        if (!setting.Contains("Email"))
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("NoEmail", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        if (!setting.Contains("DisplayName") && Convert.ToBoolean(requireUniqueDisplayName.Value))
                        {
                            PortalController.UpdatePortalSetting(_portalId, "Registration_RegistrationFormType", "0", false);
                            Skin.AddModuleMessage(this, Localization.GetString("NoDisplayName", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        PortalController.UpdatePortalSetting(_portalId, "Registration_RegistrationFields", setting);
                    }

                    PortalController.UpdatePortalSetting(_portalId, "Registration_RegistrationFormType", registrationFormType.SelectedValue, false);

                    foreach (DnnFormItemBase item in basicRegistrationSettings.Items)
                    {
                        PortalController.UpdatePortalSetting(_portalId, item.DataField, item.Value.ToString());
                    }

                    foreach (DnnFormItemBase item in standardRegistrationSettings.Items)
                    {
                        PortalController.UpdatePortalSetting(_portalId, item.DataField, item.Value.ToString());
                    }

                    foreach (DnnFormItemBase item in validationRegistrationSettings.Items)
                    {
                        try
                        {
                            var regex = new Regex(item.Value.ToString());
                            PortalController.UpdatePortalSetting(_portalId, item.DataField, item.Value.ToString());
                        }
                        catch
                        {

                            string message = String.Format(Localization.GetString("InvalidRegularExpression", LocalResourceFile),
                                                           Localization.GetString(item.DataField, LocalResourceFile), item.Value);
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                    }

                    foreach (DnnFormItemBase item in passwordRegistrationSettings.Items)
                    {
                        PortalController.UpdatePortalSetting(_portalId, item.DataField, item.Value.ToString());
                    }

                    foreach (DnnFormItemBase item in otherRegistrationSettings.Items)
                    {
                        PortalController.UpdatePortalSetting(_portalId, item.DataField, item.Value.ToString());
                    }
                    var redirectTabId = !String.IsNullOrEmpty(RedirectAfterRegistration.SelectedItem.Value) ?
                                        RedirectAfterRegistration.SelectedItem.Value
                                        : "-1";
                    PortalController.UpdatePortalSetting(_portalId, "Redirect_AfterRegistration", redirectTabId);

                    foreach (DnnFormItemBase item in loginSettings.Items)
                    {
                        PortalController.UpdatePortalSetting(_portalId, item.DataField, item.Value.ToString());
                    }
                    redirectTabId = !String.IsNullOrEmpty(RedirectAfterLogin.SelectedItem.Value) ?
                                        RedirectAfterLogin.SelectedItem.Value
                                        : "-1";
                    PortalController.UpdatePortalSetting(_portalId, "Redirect_AfterLogin", redirectTabId);
                    redirectTabId = !String.IsNullOrEmpty(RedirectAfterLogout.SelectedItem.Value) ?
                                        RedirectAfterLogout.SelectedItem.Value
                                        : "-1";
                    PortalController.UpdatePortalSetting(_portalId, "Redirect_AfterLogout", redirectTabId);

                    PortalController.UpdatePortalSetting(_portalId, DotNetNuke.Entities.Urls.FriendlyUrlSettings.VanityUrlPrefixSetting, vanilyUrlPrefixTextBox.Text, false);
                    foreach (DnnFormItemBase item in profileSettings.Items)
                    {
                        PortalController.UpdatePortalSetting(_portalId, item.DataField,
                                                                item.Value.GetType().IsEnum
                                                                    ? Convert.ToInt32(item.Value).ToString(CultureInfo.InvariantCulture)
                                                                    : item.Value.ToString()
                                                                );
                    }

                    profileDefinitions.Update();

                    DataCache.ClearPortalCache(PortalId, false);

                    //Because portal info changed, we need update current portal setting to load the correct value.
                    HttpContext.Current.Items["PortalSettings"] = new PortalSettings(TabId, PortalSettings.PortalAlias);

                    //Redirect to this site to refresh only if admin skin changed or either of the images have changed
                    if (refreshPage)
                    {
                        Response.Redirect(Request.RawUrl, true);
                    }
                    
                    BindPortal(_portalId, SelectedCultureCode);
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
                finally
                {
                    DataCache.ClearPortalCache(_portalId, false);
                }
            }
        }

        protected void OnVerifyClick(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtVerification.Text) && txtVerification.Text.EndsWith(".html"))
            {
                if (!File.Exists(Globals.ApplicationMapPath + "\\" + txtVerification.Text))
                {
                    //write SiteMap verification file
                    using (var writer = File.CreateText(Globals.ApplicationMapPath + "\\" + txtVerification.Text))
                    {
                        writer.WriteLine("google-site-verification: " + txtVerification.Text);
                    }
                }
            }
        }

        protected void ctlDesktopModules_ItemChecked(object sender, Telerik.Web.UI.RadComboBoxItemEventArgs e)
        {
            if (e.Item != null)
            {
                if (!e.Item.Checked) // there is a bug in client side, the checked status..
                {
                    DesktopModuleController.AddDesktopModuleToPortal(_portalId, int.Parse(e.Item.Value), true, true);
                }
                else
                {
                    DesktopModuleController.RemoveDesktopModuleFromPortal(_portalId, int.Parse(e.Item.Value), true);
                }

                BindDesktopModules();
            }
        }

        protected void OnChkPayPalSandboxChanged(object sender, EventArgs e)
        {
            processorLink.NavigateUrl = chkPayPalSandboxEnabled.Checked ? "https://developer.paypal.com" : Globals.AddHTTP(processorCombo.SelectedItem.Value);
        }

        #endregion

    }
}