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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.UI.WebControls;
using Telerik.Web.UI;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Languages
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Manage languages for the portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [erikvb]    20100224  Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class LanguageEnabler : PortalModuleBase
    {
        #region "Private Properties"
        private string _PortalDefault = "";

        private string PageSelectorCookieName
        {
            get
            {
                return string.Format("Languages_Page_{0}_{1}", PortalId, CultureInfo.CurrentUICulture.Name);
            }
        }

        #endregion

        #region "Protected Properties"

        protected string PortalDefault
        {
            get
            {
                return _PortalDefault;
            }
        }

        #endregion

        #region "Private Methods"

        private void BindDefaultLanguageSelector()
        {
            languagesComboBox.DataBind();
            languagesComboBox.SetLanguage(PortalDefault);
        }

        private void BindGrid()
        {
            //languagesGrid.DataSource = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
            languagesGrid.DataBind();
        }

        private TabCollection GetLocalizedPages(string code, bool includeNeutral)
        {
            return TabController.Instance.GetTabsByPortal(PortalId).WithCulture(code, includeNeutral);
        }

        #endregion

        #region "Protected Methods"

        protected bool CanEnableDisable(string code)
        {
            bool canEnable;
            if (IsLanguageEnabled(code))
            {
                canEnable = !IsDefaultLanguage(code) && !IsLanguagePublished(code);
            }
            else
            {
                canEnable = !IsDefaultLanguage(code);
            }
            return canEnable;
        }

        protected bool CanLocalize(string code)
        {

            int defaultPageCount = GetLocalizedPages(PortalSettings.DefaultLanguage, false).Count;
            int currentPageCount = GetLocalizedPages(code, false).Count;

            return PortalSettings.ContentLocalizationEnabled && IsLanguageEnabled(code) && !IsDefaultLanguage(code) && (currentPageCount < defaultPageCount);
        }

        protected string GetEditUrl(string id)
        {
            return ModuleContext.NavigateUrl(TabId, "Edit", false, string.Format("mid={0}", ModuleId), string.Format("locale={0}", id));
        }

        protected string GetEditKeysUrl(string code, string mode)
        {
            return ModuleContext.NavigateUrl(TabId, "Editor", false, string.Format("mid={0}", ModuleId), string.Format("locale={0}", code), string.Format("mode={0}", mode));
        }

        protected string GetLocalizedPages(string code)
        {
            string status = "";
            if (!IsDefaultLanguage(code) && IsLocalized(code))
            {
                status = GetLocalizedPages(code, false).Count(t => !t.Value.IsDeleted).ToString(CultureInfo.CurrentUICulture);
            }
            return status;
        }

        protected string GetLocalizablePages(string code)
        {
            int count = 0;
            foreach (KeyValuePair<int, TabInfo> t in GetLocalizedPages(code, false))
            {
                if (!t.Value.IsDeleted)
                {
                    count++;
                }
            }
            return count.ToString(CultureInfo.CurrentUICulture);
        }

        protected string GetLocalizedStatus(string code)
        {
            string status = "";
            if (!IsDefaultLanguage(code) && IsLocalized(code))
            {
                int defaultPageCount = GetLocalizedPages(PortalSettings.DefaultLanguage, false).Count;
                int currentPageCount = GetLocalizedPages(code, false).Count;
                status = string.Format("{0:#0%}", currentPageCount / (float)defaultPageCount);
            }
            return status;
        }

        protected string GetTranslatedPages(string code)
        {
            string status = "";
            if (!IsDefaultLanguage(code) && IsLocalized(code))
            {
                int translatedCount = (from t in TabController.Instance.GetTabsByPortal(PortalId).WithCulture(code, false).Values where t.IsTranslated && !t.IsDeleted select t).Count();
                status = translatedCount.ToString(CultureInfo.InvariantCulture);
            }
            return status;
        }

        protected string GetTranslatedStatus(string code)
        {
            string status = "";
            if (!IsDefaultLanguage(code) && IsLocalized(code))
            {
                int localizedCount = GetLocalizedPages(code, false).Count;
                int translatedCount = (from t in TabController.Instance.GetTabsByPortal(PortalId).WithCulture(code, false).Values where t.IsTranslated select t).Count();
                status = string.Format("{0:#0%}", translatedCount / (float)localizedCount);
            }
            return status;
        }

        protected bool IsDefaultLanguage(string code)
        {
            return code == PortalDefault;
        }

        protected bool IsLanguageEnabled(string Code)
        {
            Locale enabledLanguage;
            return LocaleController.Instance.GetLocales(ModuleContext.PortalId).TryGetValue(Code, out enabledLanguage);
        }

        protected bool IsLanguagePublished(string Code)
        {
            bool isPublished = Null.NullBoolean;
            Locale enabledLanguage;
            if (LocaleController.Instance.GetLocales(ModuleContext.PortalId).TryGetValue(Code, out enabledLanguage))
            {
                isPublished = enabledLanguage.IsPublished;
            }
            return isPublished;
        }

        protected bool IsLocalized(string code)
        {
            return (code != PortalDefault && GetLocalizedPages(code, false).Count > 0);
        }

        protected string BuildConfirmationJS(string controlName, string confirmResource)
        {

            string s = "";
            foreach (GridDataItem dataItem in languagesGrid.MasterTableView.Items)
            {
                var control = dataItem.FindControl(controlName);
                var button = control as LinkButton;
                if (button != null)
                {
                    if (button.Visible)
                    {
                        int languageId = int.Parse(button.CommandArgument);
                        Locale localeToDelete = LocaleController.Instance.GetLocale(languageId);
                        s += string.Format(@"$('#{0}').dnnConfirm({{text: '{1}', yesText: '{2}', noText: '{3}', title: '{4}'}});",
                            button.ClientID,
                            string.Format(Localization.GetSafeJSString(confirmResource, LocalResourceFile), localeToDelete.Code),
                            Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile),
                            Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile),
                            Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile)
                            );
                    }
                }
            }
            return s;
        }

        protected void BindCLControl()
        {
            NeutralMessage.Visible = false;
            MakeTranslatable.Visible = false;
            MakeNeutral.Visible = false;
            cmdUpdate.Visible = false;
            AddMissing.Visible = false;
            var tabInfo = ddlPages.SelectedPage;
            if (tabInfo != null)
            {
                if (String.IsNullOrEmpty(tabInfo.CultureCode))
                {
                    CLControl1.Visible = false;
                    if (UserInfo.IsSuperUser || UserInfo.IsInRole("Administrators"))
                    {
                        MakeTranslatable.Visible = true;
                    }
                    NeutralMessage.Visible = true;
                }
                else
                {
                    CLControl1.Visible = true;
                    CLControl1.enablePageEdit = true;
                    CLControl1.BindAll(tabInfo.TabID);
                    cmdUpdate.Visible = true;

                    if (UserInfo.IsSuperUser || UserInfo.IsInRole("Administrators"))
                    {
                        // only show "Convert to neutral" if page has no child pages
                    MakeNeutral.Visible = (TabController.Instance.GetTabsByPortal(PortalId).WithParentId(tabInfo.TabID).Count == 0);

                        // only show "add missing languages" if not all languages are available
                        AddMissing.Visible = TabController.Instance.HasMissingLanguages(PortalId, tabInfo.TabID);
                    }
                }
            }
        }

        #endregion

        #region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            languagesComboBox.ModeChanged += languagesComboBox_ModeChanged;
            languagesGrid.ItemDataBound += languagesGrid_ItemDataBound;
            languagesGrid.PreRender += languagesGrid_PreRender;
            updateButton.Click += updateButton_Click;
            cmdDisableLocalization.Click += cmdDisableLocalization_Click;
            cmdEnableLocalizedContent.NavigateUrl = ModuleContext.NavigateUrl(ModuleContext.TabId, "EnableContent", false, "mid=" + ModuleContext.ModuleId);

            AJAX.RegisterScriptManager();
            JavaScript.RequestRegistration(CommonJs.jQuery);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Locale enabledLanguage;
            LocaleController.Instance.GetLocales(ModuleContext.PortalId).TryGetValue("en-US", out enabledLanguage);

            DotNetNuke.Framework.JavaScriptLibraries.JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxScriptSupport();
            DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            try
            {
                _PortalDefault = PortalSettings.DefaultLanguage;
                if (!Page.IsPostBack)
                {
                    BindDefaultLanguageSelector();
                    BindGrid();
                    chkBrowser.Checked = ModuleContext.PortalSettings.EnableBrowserLanguage;
                    chkUserCulture.Checked = ModuleContext.PortalSettings.AllowUserUICulture;

                    urlRow.Visible = !PortalSettings.Current.ContentLocalizationEnabled;
                    chkUrl.Checked = ModuleContext.PortalSettings.EnableUrlLanguage;

                }

                if (!UserInfo.IsSuperUser && ModulePermissionController.CanAdminModule(ModuleConfiguration))
                {
                    UI.Skins.Skin.AddModuleMessage(this, LocalizeString("HostOnlyMessage"), ModuleMessage.ModuleMessageType.BlueInfo);
                }

                systemDefaultLanguageLabel.Language = Localization.SystemLocale;

                addLanguageLink.Visible = UserInfo.IsSuperUser;
                addLanguageLink.NavigateUrl = ModuleContext.EditUrl("Edit");

                createLanguagePackLink.Visible = UserInfo.IsSuperUser;
                createLanguagePackLink.NavigateUrl = ModuleContext.EditUrl("PackageWriter");

                verifyLanguageResourcesLink.Visible = UserInfo.IsSuperUser;
                verifyLanguageResourcesLink.NavigateUrl = ModuleContext.EditUrl("Verify");

                installLanguagePackLink.Visible = UserInfo.IsSuperUser;
                installLanguagePackLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "");

                installAvailableLanguagePackLink.Visible = UserInfo.IsSuperUser;
                var tab = TabController.Instance.GetTabByName("Extensions", Null.NullInteger);
                installAvailableLanguagePackLink.NavigateUrl = string.Format("{0}#availableExtensions", tab.FullUrl);

                if (!ModulePermissionController.CanAdminModule(ModuleConfiguration))
                {
                    tabLanguages.Visible = Convert.ToBoolean(ModuleContext.Settings["ShowLanguages"]);
                    tabSettings.Visible = Convert.ToBoolean(ModuleContext.Settings["ShowSettings"]);
                    PanelLanguages.Visible = tabLanguages.Visible;
                    panelSettings.Visible = tabSettings.Visible;

                    // only show CL tab if other tabs are visible. We are still going to show the CL Panel though
                    TabStrips.Visible = tabSettings.Visible || tabSettings.Visible;
                }

                if (PortalSettings.ContentLocalizationEnabled)
                {
                    defaultLanguageLabel.Language = PortalSettings.DefaultLanguage;
                    defaultLanguageLabel.Visible = true;
                    languagesComboBox.Visible = false;
                    cmdEnableLocalizedContent.Visible = false;
                    cmdDisableLocalization.Visible = true;
                    defaultPortalMessage.Text = LocalizeString("PortalDefaultPublished.Text");
                    enabledPublishedPlaceHolder.Visible = true;
                    if (!Page.IsPostBack)
                    {
                        SetSelectedPage();
                        ddlPages_SelectedIndexChanged(null, null);
                    }
                }
                else
                {
                    defaultLanguageLabel.Visible = false;
                    languagesComboBox.Visible = true;
                    cmdEnableLocalizedContent.Visible = Host.EnableContentLocalization;
                    cmdDisableLocalization.Visible = false;
                    defaultPortalMessage.Text = LocalizeString("PortalDefaultEnabled.Text");
                    enabledPublishedPlaceHolder.Visible = false;
                    tabContentLocalization.Visible = false;
                    panelContentLocalization.Visible = false;
                }

            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void SetSelectedPage()
        {
            string CultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
            // try to set the selected item in the page dropdown.
            // if we have a cookie value for language x, we also try to select the right page
            // if the language switched to y
            if (Request.Cookies[PageSelectorCookieName] != null)
            {
                int selectedPage;
                TabInfo tabInfo = null;
                bool goodValue = int.TryParse(Request.Cookies[PageSelectorCookieName].Value, out selectedPage);
                if (goodValue)
                {
                    tabInfo = TabController.Instance.GetTab(selectedPage, PortalId, false);
                    if (tabInfo.IsDeleted)
                    {
                        tabInfo = null;
                        Response.Cookies.Remove(PageSelectorCookieName);
                    }
                }
                if (tabInfo == null)
                {
                    tabInfo = TabController.Instance.GetTab(PortalSettings.HomeTabId, PortalId, false);
                }
                goodValue = (tabInfo != null);
                if (goodValue)
                {
                    if (tabInfo.CultureCode == CultureCode)
                    {
                        ddlPages.SelectedPage = tabInfo;
                    }
                    else
                    {
                        if (tabInfo.DefaultLanguageTab != null)
                        {
                            if (tabInfo.DefaultLanguageTab.CultureCode == CultureCode)
                            {
                                ddlPages.SelectedPage = tabInfo;
                            }
                            else
                            {
                                foreach (var tabKV in tabInfo.DefaultLanguageTab.LocalizedTabs.Where(tabKV => tabKV.Value.CultureCode == CultureCode))
                                {
                                    ddlPages.SelectedPage = tabKV.Value;
                                    break;
                                }
                            }
                        }
                        else if (tabInfo.LocalizedTabs != null)
                        {
                            foreach (var tabKV in tabInfo.LocalizedTabs.Where(tabKV => tabKV.Value.CultureCode == CultureCode))
                            {
                                ddlPages.SelectedPage = tabKV.Value;
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected void enabledCheckbox_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if ((sender) is CheckBox)
                {
                    var enabledCheckbox = (CheckBox)sender;
                    GridDataItem item = (GridDataItem)enabledCheckbox.NamingContainer;
                    DnnLanguageLabel code = item.FindControl("translationStatusLabel") as DnnLanguageLabel;
                    Locale locale = LocaleController.Instance.GetLocale(code.Language);
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);

                    Dictionary<string, Locale> enabledLanguages = LocaleController.Instance.GetLocales(PortalId);

                    var localizedTabs = PortalSettings.ContentLocalizationEnabled ?
                        TabController.Instance.GetTabsByPortal(PortalId).WithCulture(locale.Code, false).AsList() : new List<TabInfo>();

                    var redirectUrl = string.Empty;
                    if (enabledCheckbox.Enabled)
                    {
                        // do not touch default language
                        if (enabledCheckbox.Checked)
                        {
                            if (!enabledLanguages.ContainsKey(locale.Code))
                            {
                                //Add language to portal
                                Localization.AddLanguageToPortal(PortalId, locale.LanguageId, true);
                            }

                            //restore the tabs and modules
                            foreach (var tab in localizedTabs)
                            {
                                TabController.Instance.RestoreTab(tab, PortalSettings);
                                ModuleController.Instance.GetTabModules(tab.TabID).Values.ToList().ForEach(ModuleController.Instance.RestoreModule);
                            }
                        }
                        else
                        {
                            //remove language from portal
                            Localization.RemoveLanguageFromPortal(PortalId, locale.LanguageId);

                            //if the disable language is current language, should redirect to default language.
                            if (locale.Code.Equals(Thread.CurrentThread.CurrentUICulture.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                redirectUrl = Globals.NavigateURL(PortalSettings.ActiveTab.TabID,
                                                                    PortalSettings.ActiveTab.IsSuperTab,
                                                                    PortalSettings, "", defaultLocale.Code);
                            }

                            //delete the tabs in this language
                            foreach (var tab in localizedTabs)
                            {
                                tab.DefaultLanguageGuid = Guid.Empty;
                                TabController.Instance.SoftDeleteTab(tab.TabID, PortalSettings);
                            }
                        }
                    }

                    //Redirect to refresh page (and skinobjects)
                    if (string.IsNullOrEmpty(redirectUrl))
                    {
                        redirectUrl = Globals.NavigateURL();
                    }

                    Response.Redirect(redirectUrl, true);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void languagesComboBox_ModeChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void languagesGrid_ItemDataBound(object sender, GridItemEventArgs e)
        {
            var gridItem = e.Item as GridDataItem;
            if (gridItem != null)
            {
                var locale = gridItem.DataItem as Locale;
                if (locale != null)
                {
                    var localizeLinkAlt = gridItem.FindControl("localizeLinkAlt") as HyperLink;
                    if (localizeLinkAlt != null)
                    {
                        localizeLinkAlt.NavigateUrl = ModuleContext.NavigateUrl(ModuleContext.TabId,
                                                                            "LocalizePages",
                                                                            false,
                                                                            "mid=" + ModuleContext.ModuleId,
                                                                            "locale=" + locale.Code);
                    }
                    var localizeLink = gridItem.FindControl("localizeLink") as HyperLink;
                    if (localizeLink != null)
                    {
                        CultureDropDownTypes DisplayType;
                        string _ViewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", "ViewType" + PortalId));
                        switch (_ViewType)
                        {
                            case "NATIVE":
                                DisplayType = CultureDropDownTypes.NativeName;
                                break;
                            case "ENGLISH":
                                DisplayType = CultureDropDownTypes.EnglishName;
                                break;
                            default:
                                DisplayType = CultureDropDownTypes.DisplayName;
                                break;
                        }

                        localizeLink.NavigateUrl = ModuleContext.NavigateUrl(ModuleContext.TabId,
                                                                            "LocalizePages",
                                                                            false,
                                                                            "mid=" + ModuleContext.ModuleId,
                                                                            "locale=" + locale.Code);

                        var enabledCheckbox = gridItem.FindControl("enabledCheckbox") as CheckBox;
                        if (enabledCheckbox != null)
                        {
                            enabledCheckbox.Checked = IsLanguageEnabled(locale.Code);

                            if (enabledCheckbox.Checked)
                            {
                                string msg = String.Format(LocalizeString("Disable.Confirm"), Localization.GetLocaleName(locale.Code, DisplayType));
                                enabledCheckbox.Attributes.Add("onclick", "if (!confirm('" + Localization.GetSafeJSString(msg) + "')) return false;");
                            }
                        }

                        var publishedCheckbox = gridItem.FindControl("publishedCheckbox") as CheckBox;
                        if (publishedCheckbox != null)
                        {
                            publishedCheckbox.Checked = IsLanguagePublished(locale.Code);

                            if (publishedCheckbox.Checked)
                            {
                                string msg = String.Format(LocalizeString("Unpublish.Confirm"), Localization.GetLocaleName(locale.Code, DisplayType));
                                publishedCheckbox.Attributes.Add("onclick", "if (!confirm('" + Localization.GetSafeJSString(msg) + "')) return false;");
                            }
                        }
                    }
                }
            }
        }

        protected void languagesGrid_PreRender(object sender, EventArgs e)
        {
            foreach (GridColumn column in languagesGrid.Columns)
            {
                if ((column.UniqueName == "ContentLocalization"))
                {
                    column.Visible = PortalSettings.ContentLocalizationEnabled;
                }
            }
            languagesGrid.Rebind();
        }

        protected void publishedCheckbox_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if ((sender) is CheckBox)
                {
                    var publishedCheckbox = (CheckBox)sender;
                    GridDataItem item = (GridDataItem)publishedCheckbox.NamingContainer;
                    DnnLanguageLabel code = item.FindControl("translationStatusLabel") as DnnLanguageLabel;
                    Locale locale = LocaleController.Instance.GetLocale(code.Language);

                    if (publishedCheckbox.Enabled)
                    {
                        LocaleController.Instance.PublishLanguage(PortalId, locale.Code, publishedCheckbox.Checked);
                    }

                    //Redirect to refresh page (and skinobjects)
                    Response.Redirect(Globals.NavigateURL(), true);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void PublishPages(object sender, EventArgs eventArgs)
        {
            var cmdPublishPages = (LinkButton)sender;
            int languageId = int.Parse(cmdPublishPages.CommandArgument);
            var locale = new LocaleController().GetLocale(languageId);
            LocaleController.Instance.PublishLanguage(PortalId, locale.Code, true);
            
            //Redirect to refresh page (and skinObjects)
            Response.Redirect(Globals.NavigateURL(), true);
        }

        protected void MarkAllPagesTranslated(object sender, EventArgs eventArgs)
        {
            var cmdTranslateAll = (LinkButton)sender;
            int languageId = int.Parse(cmdTranslateAll.CommandArgument);
            var locale = new LocaleController().GetLocale(languageId);

            var nonTranslated = (from t in TabController.Instance.GetTabsByPortal(PortalId).WithCulture(locale.Code, false).Values where !t.IsTranslated && !t.IsDeleted select t);
            foreach (TabInfo page in nonTranslated)
            {
                page.LocalizedVersionGuid = page.DefaultLanguageTab.LocalizedVersionGuid;
                TabController.Instance.UpdateTab(page);
            }
            BindGrid();
        }

        protected void updateButton_Click(object sender, EventArgs e)
        {
            PortalController.UpdatePortalSetting(ModuleContext.PortalId, "EnableBrowserLanguage", chkBrowser.Checked.ToString());
            PortalController.UpdatePortalSetting(ModuleContext.PortalId, "AllowUserUICulture", chkUserCulture.Checked.ToString());

            // if contentlocalization is enabled, default language cannot be changed
            if (!PortalSettings.ContentLocalizationEnabled)
            {
                // first check whether or not portal default language has changed
                string newDefaultLanguage = languagesComboBox.SelectedValue;
                if (newDefaultLanguage != PortalSettings.DefaultLanguage)
                {
                    var needToRemoveOldDefaultLanguage = LocaleController.Instance.GetLocales(PortalId).Count == 1;
                    var OldDefaultLanguage = LocaleController.Instance.GetLocale(PortalDefault);
                    if (!IsLanguageEnabled(newDefaultLanguage))
                    {
                        var language = LocaleController.Instance.GetLocale(newDefaultLanguage);
                        Localization.AddLanguageToPortal(ModuleContext.PortalId, language.LanguageId, true);
                    }

                    // update portal default language
                    var portal = PortalController.Instance.GetPortal(PortalId);
                    portal.DefaultLanguage = newDefaultLanguage;
                    PortalController.Instance.UpdatePortalInfo(portal);

                    _PortalDefault = newDefaultLanguage;

                    if (needToRemoveOldDefaultLanguage)
                    {
                        Localization.RemoveLanguageFromPortal(PortalId, OldDefaultLanguage.LanguageId);
                    }
                }

                PortalController.UpdatePortalSetting(ModuleContext.PortalId, "EnableUrlLanguage", chkUrl.Checked.ToString());
            }
            BindDefaultLanguageSelector();
            BindGrid();
        }

        protected void cmdDeleteTranslation_Click(object sender, EventArgs e)
        {
            try
            {
                if ((sender) is LinkButton)
                {
                    var cmdDeleteTranslation = (LinkButton)sender;
                    int languageId = int.Parse(cmdDeleteTranslation.CommandArgument);
                    Locale locale = LocaleController.Instance.GetLocale(languageId);

                    TabController.Instance.DeleteTranslatedTabs(PortalId, locale.Code, false);

                    PortalController.Instance.RemovePortalLocalization(PortalId, locale.Code, false);

                    LocaleController.Instance.PublishLanguage(PortalId, locale.Code, false);


                    DataCache.ClearPortalCache(PortalId, true);

                    //Redirect to refresh page (and skinobjects)
                    Response.Redirect(Globals.NavigateURL(), true);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void cmdDisableLocalization_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Locale locale in LocaleController.Instance.GetLocales(PortalSettings.PortalId).Values)
                {
                    if (!IsDefaultLanguage(locale.Code))
                    {

                        LocaleController.Instance.PublishLanguage(PortalId, locale.Code, false);
                        TabController.Instance.DeleteTranslatedTabs(PortalId, locale.Code, false);
                        PortalController.Instance.RemovePortalLocalization(PortalId, locale.Code, false);

                    }
                }

                TabController.Instance.EnsureNeutralLanguage(PortalId, PortalDefault, false);

                PortalController.UpdatePortalSetting(PortalId, "ContentLocalizationEnabled", "False");

                DataCache.ClearPortalCache(PortalId, true);


                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void ddlPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPages.SelectedPage != null)
            {
                var pageCookie = new HttpCookie(PageSelectorCookieName, ddlPages.SelectedPage.TabID.ToString(CultureInfo.InvariantCulture) )
                {
                    Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                };
                Response.Cookies.Add(pageCookie);
            }
            BindCLControl();
        }

        protected void LanguagesGrid_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            languagesGrid.DataSource = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            CLControl1.SaveData();
            BindCLControl();
        }

        protected void cmdFix_Click(object sender, EventArgs e)
        {
            CLControl1.FixLocalizationErrors(ddlPages.SelectedPage.TabID);
            BindCLControl();
        }

        protected void MakeTranslatable_Click(object sender, EventArgs e)
        {
            var tab = ddlPages.SelectedPage;
            var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);
            TabController.Instance.LocalizeTab(tab, defaultLocale, false);
            TabController.Instance.AddMissingLanguages(PortalId, tab.TabID);
            TabController.Instance.ClearCache(PortalId);
            BindCLControl();
        }

        protected void MakeNeutral_Click(object sender, EventArgs e)
        {
            var tab = ddlPages.SelectedPage;
            if (TabController.Instance.GetTabsByPortal(PortalId).WithParentId(tab.TabID).Count == 0)
            {
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);

                TabController.Instance.ConvertTabToNeutralLanguage(PortalId, tab.TabID, defaultLocale.Code, true);

                BindCLControl();
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ChildrenExistWhenConvertingToNeutral", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        protected void AddMissing_Click(object sender, EventArgs e)
        {
            TabController.Instance.AddMissingLanguages(PortalId, ddlPages.SelectedPage.TabID);

            BindCLControl();

        }

        #endregion

    }
}