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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Languages
// ReSharper restore CheckNamespace
{

    public partial class CLControl : UserControl
    {
        #region Private
        private string _localResourceFile;

        protected string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + "CLTools.ascx";
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                _localResourceFile = fileRoot;

                return fileRoot;
            }
        }

        private TabController _tabController;
        private TabController TabController
        {
            get
            {
                return _tabController ?? (_tabController = new TabController());
            }
        }

        private ModuleController _moduleController;
        private ModuleController ModuleController
        {
            get
            {
                return _moduleController ?? (_moduleController = new ModuleController());
            }
        }

        #endregion

        #region Protected

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }


        protected DnnPages Data { get; set; }

        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.GetCurrentPortalSettings();
            }
        }

        protected void cmdDeleteTranslatedPage(object sender, EventArgs eventArgs)
        {
            if ((sender) is LinkButton)
            {
                var cmdDeleteTranslation = (LinkButton)sender;
                var args = cmdDeleteTranslation.CommandArgument.Split('|');
                int tabId = int.Parse(args[0]);
                TabController.DeleteTab(tabId, PortalSettings.PortalId);
                Response.Redirect(Request.RawUrl, false);
            }
        }

        protected void cmdDeleteModule(object sender, EventArgs e)
        {
            if ((sender) is LinkButton)
            {
                var cmdDeleteModule = (LinkButton)sender;
                var tabModuleId = int.Parse(cmdDeleteModule.CommandArgument);
                var moduleInfo = ModuleController.GetTabModule(tabModuleId);
                ModuleController.DeleteTabModule(moduleInfo.TabID, moduleInfo.ModuleID, false);
                Response.Redirect(Request.RawUrl, false);
            }

        }

        protected void cmdRestoreModule(object sender, EventArgs e)
        {
            if ((sender) is LinkButton)
            {
                var cmdRestoreModule = (LinkButton)sender;
                var tabModuleId = int.Parse(cmdRestoreModule.CommandArgument);
                var moduleInfo = ModuleController.GetTabModule(tabModuleId);
                ModuleController.RestoreModule(moduleInfo);
                Response.Redirect(Request.RawUrl, false);
            }
        }

        protected string GetModuleTitleHint(bool isDeleted)
        {
            return LocalizeString(isDeleted ? "ModuleDeleted.Text" : "ModuleTitle.Text");
        }


        protected string GetModuleInfo(object moduleID)
        {
            string returnValue = "";
            if (moduleID != null)
            {
                var moduleInfo = ModuleController.GetModule((int)moduleID);
                if (moduleInfo != null)
                {
                    if (moduleInfo.IsDeleted)
                    {
                        returnValue = LocalizeString("ModuleDeleted.Text");
                    }
                    else
                    {
                        if (ModulePermissionController.CanAdminModule(moduleInfo))
                        {
                            returnValue = string.Format(LocalizeString("ModuleInfo.Text"), moduleInfo.ModuleDefinition.FriendlyName, moduleInfo.ModuleTitle, moduleInfo.PaneName);
                        }
                        else
                        {
                            returnValue = LocalizeString("ModuleInfoForNonAdmins.Text");
                        }
                    }
                }
            }
            return returnValue;
        }

        protected string CultureName(object cultureCode)
        {
            string returnValue = "Neutral Language";
            if (cultureCode != null)
            {
                returnValue = new CultureInfo((string)cultureCode).NativeName;
            }
            return returnValue;
        }

        protected string BuildSettingsURL(int tabId)
        {
            string url = Globals.NavigateURL(tabId, "Tab", "action=edit&activeTab=settingTab");
            if (PortalSettings != null && PortalSettings.EnablePopUps)
            {
                url = UrlUtils.PopUpUrl(url, null, PortalSettings, false, false);
            }

            return url;
        }

        protected bool PublishVisible(string code)
        {

            bool isPublished = Null.NullBoolean;
            Locale enabledLanguage;
            if (LocaleController.Instance.GetLocales(PortalSettings.PortalId).TryGetValue(code, out enabledLanguage))
            {
                isPublished = enabledLanguage.IsPublished && (enabledLanguage.Code != PortalSettings.DefaultLanguage);
            }
            return isPublished;
        }

        protected string DeletedClass(bool isDeleted)
        {
            return isDeleted ? "moduleDeleted" : "";
        }

        #endregion

        #region public

        public bool ErrorExists { get; set; }

        public int TabID { get; set; }

        public CLControl()
        {
            enablePageEdit = false;
        }

        public bool enablePageEdit { get; set; }

        public void BindAll(int tabID)
        {
            TabID = tabID;
            var currentTab = TabController.GetTab(tabID, PortalSettings.PortalId, false);

            //Unique id of default language page
            var uniqueId = currentTab.DefaultLanguageGuid != Null.NullGuid ? currentTab.DefaultLanguageGuid : currentTab.UniqueId;

            // get all non admin pages and not deleted
            var allPages = TabController.GetTabsByPortal(PortalSettings.PortalId).Values.Where(t => t.TabID != PortalSettings.AdminTabId && (Null.IsNull(t.ParentId) || t.ParentId != PortalSettings.AdminTabId));
            allPages = allPages.Where(t => t.IsDeleted == false);
            // get all localized pages of current page
            var tabInfos = allPages as IList<TabInfo> ?? allPages.ToList();
            var localizedPages = tabInfos.Where(t => t.DefaultLanguageGuid == uniqueId || t.UniqueId == uniqueId).OrderBy(t => t.DefaultLanguageGuid).ToList();
            Dictionary<string, TabInfo> localizedTabs = null;

            // we are going to build up a list of locales
            // this is a bit more involved, since we want the default language to be first.
            // also, we do not want to add any locales the user has no access to
            var locales = new List<string>();
            var localeController = new LocaleController();
            var localeDict = localeController.GetLocales(PortalSettings.PortalId);
            if (localeDict.Count > 0)
            {
                if (localizedPages.Count() == 1 && localizedPages.First().CultureCode == "")
                {
                    // locale neutral page
                    locales.Add("");
                }
                else if (localizedPages.Count() == 1 && localizedPages.First().CultureCode != PortalSettings.DefaultLanguage)
                {
                    locales.Add(localizedPages.First().CultureCode);
                    localizedTabs = new Dictionary<string, TabInfo>();
                    localizedTabs.Add(localizedPages.First().CultureCode, localizedPages.First());
                }
                else
                {

                    //force sort order, so first add default language
                    locales.Add(PortalSettings.DefaultLanguage);

                    // build up a list of localized tabs.
                    // depending on whether or not the selected page is in the default langauge
                    // we will add the localized tabs from the current page
                    // or from the defaultlanguage page
                    if (currentTab.CultureCode == PortalSettings.DefaultLanguage)
                    {
                        localizedTabs = currentTab.LocalizedTabs;
                    }
                    else
                    {
                        // selected page is not in default language
                        // add localizedtabs from defaultlanguage page
                        if (currentTab.DefaultLanguageTab != null)
                        {
                            localizedTabs = currentTab.DefaultLanguageTab.LocalizedTabs;
                        }
                    }

                    if (localizedTabs != null)
                    {
                        // only add locales from tabs the user has at least view permissions to. 
                        // we will handle the edit permissions at a later stage
                        locales.AddRange(from localizedTab in localizedTabs where TabPermissionController.CanViewPage(localizedTab.Value) select localizedTab.Value.CultureCode);
                    }


                }
            }
            else
            {
                locales.Add("");
            }

            Data = new DnnPages(locales);

            // filter the list of localized pages to only those that have a culture we want to see
            var viewableLocalizedPages = localizedPages.Where(localizedPage => locales.Find(locale => locale == localizedPage.CultureCode) != null).ToList();
            if (viewableLocalizedPages.Count() > 4)
            {
                mainContainer.Attributes.Add("class", "container RadGrid RadGrid_Default overflow");
            }

            foreach (var tabInfo in viewableLocalizedPages)
            {
                var localTabInfo = tabInfo;
                var dnnPage = Data.Page(localTabInfo.CultureCode);
                if (!TabPermissionController.CanViewPage(tabInfo))
                {
                    Data.RemoveLocale(localTabInfo.CultureCode);
                    Data.Pages.Remove(dnnPage);
                    break;
                }
                dnnPage.TabID = localTabInfo.TabID;
                dnnPage.TabName = localTabInfo.TabName;
                dnnPage.Title = localTabInfo.Title;
                dnnPage.Description = localTabInfo.Description;
                dnnPage.Path = localTabInfo.TabPath.Substring(0, localTabInfo.TabPath.LastIndexOf("//", StringComparison.Ordinal)).Replace("//", "");
                dnnPage.HasChildren = (TabController.GetTabsByPortal(PortalSettings.PortalId).WithParentId(tabInfo.TabID).Count != 0);
                dnnPage.CanAdminPage = TabPermissionController.CanAdminPage(tabInfo);
                dnnPage.CanViewPage = TabPermissionController.CanViewPage(tabInfo);
                dnnPage.LocalResourceFile = LocalResourceFile;

                // calculate position in the form of 1.3.2...
                var SiblingTabs = tabInfos.Where(t => t.ParentId == localTabInfo.ParentId && t.CultureCode == localTabInfo.CultureCode || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                dnnPage.Position = (SiblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture);
                int ParentTabId = localTabInfo.ParentId;
                while (ParentTabId > 0)
                {
                    TabInfo ParentTab = tabInfos.Single(t => t.TabID == ParentTabId);
                    int id = ParentTabId;
                    SiblingTabs = tabInfos.Where(t => t.ParentId == id && t.CultureCode == localTabInfo.CultureCode || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                    dnnPage.Position = (SiblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture) + "." + dnnPage.Position;
                    ParentTabId = ParentTab.ParentId;
                }

                dnnPage.DefaultLanguageGuid = localTabInfo.DefaultLanguageGuid;
                dnnPage.IsTranslated = localTabInfo.IsTranslated;
                dnnPage.IsPublished = TabController.IsTabPublished(localTabInfo);
                // generate modules information
                foreach (var moduleInfo in ModuleController.GetTabModules(localTabInfo.TabID).Values)
                //foreach (var moduleInfo in ModuleController.GetTabModules(localTabInfo.TabID).Values.Where(m => !m.IsDeleted))
                {
                    var guid = moduleInfo.DefaultLanguageGuid == Null.NullGuid ? moduleInfo.UniqueId : moduleInfo.DefaultLanguageGuid;

                    var dnnModules = Data.Module(guid); // modules of each language
                    var dnnModule = dnnModules.Module(localTabInfo.CultureCode);
                    // detect error : 2 modules with same uniqueId on the same page
                    dnnModule.LocalResourceFile = LocalResourceFile;
                    if (dnnModule.TabModuleID > 0)
                    {
                        dnnModule.ErrorDuplicateModule = true;
                        ErrorExists = true;
                        continue;
                    }

                    dnnModule.ModuleTitle = moduleInfo.ModuleTitle;
                    dnnModule.DefaultLanguageGuid = moduleInfo.DefaultLanguageGuid;
                    dnnModule.TabId = localTabInfo.TabID;
                    dnnModule.TabModuleID = moduleInfo.TabModuleID;
                    dnnModule.ModuleID = moduleInfo.ModuleID;
                    dnnModule.CanAdminModule = ModulePermissionController.CanAdminModule(moduleInfo);
                    dnnModule.CanViewModule = ModulePermissionController.CanViewModule(moduleInfo);
                    dnnModule.IsDeleted = moduleInfo.IsDeleted;
                    if (moduleInfo.DefaultLanguageGuid != Null.NullGuid)
                    {
                        ModuleInfo defaultLanguageModule = ModuleController.GetModuleByUniqueID(moduleInfo.DefaultLanguageGuid);
                        if (defaultLanguageModule != null)
                        {
                            dnnModule.DefaultModuleID = defaultLanguageModule.ModuleID;
                            if (defaultLanguageModule.ParentTab.UniqueId != moduleInfo.ParentTab.DefaultLanguageGuid)
                                dnnModule.DefaultTabName = defaultLanguageModule.ParentTab.TabName;
                        }
                    }
                    dnnModule.IsTranslated = moduleInfo.IsTranslated;
                    dnnModule.IsLocalized = moduleInfo.IsLocalized;

                    dnnModule.IsShared = TabController.GetTabsByModuleID(moduleInfo.ModuleID).Values.Count(t => t.CultureCode == moduleInfo.CultureCode) > 1;

                    // detect error : the default language module is on an other page
                    dnnModule.ErrorDefaultOnOtherTab = moduleInfo.DefaultLanguageGuid != Null.NullGuid && moduleInfo.DefaultLanguageModule == null;

                    // detect error : different culture on tab and module
                    dnnModule.ErrorCultureOfModuleNotCultureOfTab = moduleInfo.CultureCode != localTabInfo.CultureCode;

                    ErrorExists = ErrorExists || dnnModule.ErrorDefaultOnOtherTab || dnnModule.ErrorCultureOfModuleNotCultureOfTab;
                }
            }

            rDnnModules.DataSource = Data.Modules;
            rDnnModules.DataBind();

        }

        public void SaveData()
        {

            // pages actions
            var rDnnPage = (Repeater)Globals.FindControlRecursiveDown(rDnnModules, "rDnnPage");
            var tabsToPublish = new List<TabInfo>();
            var moduleTranslateOverrides = new Dictionary<int, bool>();

            foreach (RepeaterItem repeaterItem in rDnnPage.Items)
            {
                var tbTabName = (TextBox)repeaterItem.FindControl("tbTabName");
                var tabName = tbTabName.Text;
                var tbTabTitle = (TextBox)repeaterItem.FindControl("tbTitle");
                var tabTitle = tbTabTitle.Text;
                var tbTabDescription = (TextBox)repeaterItem.FindControl("tbDescription");
                var tabDescription = tbTabDescription.Text;

                var hfTabId = (HiddenField)repeaterItem.FindControl("hfTabID");
                int tabID;
                var ConvertSuccess = int.TryParse(hfTabId.Value, out tabID);
                if (!ConvertSuccess)
                {
                    tabID = -1;
                }
                if (tabID > 0)
                {
                    var tabInfo = TabController.GetTab(tabID, PortalSettings.PortalId, true);
                    var updateTab = false;
                    if (tabInfo.TabName != tabName)
                    {
                        tabInfo.TabName = tabName;
                        updateTab = true;
                    }
                    if (tabInfo.Title != tabTitle)
                    {
                        tabInfo.Title = tabTitle;
                        updateTab = true;
                    }
                    if (tabInfo.Description != tabDescription)
                    {
                        tabInfo.Description = tabDescription;
                        updateTab = true;
                    }
                    if (updateTab)
                    {
                        TabController.UpdateTab(tabInfo);
                    }

                }

            }


            // manage all actions we need to take for all modules on all pages
            foreach (RepeaterItem repeaterItem in rDnnModules.Items)
            {
                var rDnnModule = (Repeater)repeaterItem.FindControl("rDnnModule");
                foreach (RepeaterItem riDnnModule in rDnnModule.Items)
                {
                    var hfTabModuleID = (HiddenField)riDnnModule.FindControl("hfTabModuleID");
                    int tabModuleID = int.Parse(hfTabModuleID.Value);
                    if (tabModuleID > 0)
                    {
                        var tbModuleTitle = (TextBox)riDnnModule.FindControl("tbModuleTitle");
                        var moduleTitle = tbModuleTitle.Text;
                        var cbLocalized = (CheckBox)riDnnModule.FindControl("cbLocalized");
                        var moduleLocalized = cbLocalized.Checked;
                        var cbTranslated = (CheckBox)riDnnModule.FindControl("cbTranslated");
                        var moduleTranslated = cbTranslated.Checked;
                        var tabModule = ModuleController.GetTabModule(tabModuleID);
                        if (tabModule.ModuleTitle != moduleTitle)
                        {
                            tabModule.ModuleTitle = moduleTitle;
                            ModuleController.UpdateModule(tabModule);
                        }
                        if (tabModule.DefaultLanguageGuid != Null.NullGuid && tabModule.IsLocalized != moduleLocalized)
                        {
                            var locale = LocaleController.Instance.GetLocale(tabModule.CultureCode);
                            if (moduleLocalized)
                                ModuleController.LocalizeModule(tabModule, locale);
                            else
                                ModuleController.DeLocalizeModule(tabModule);
                        }

                        bool moduleTranslateOverride;
                        moduleTranslateOverrides.TryGetValue(tabModule.TabID, out moduleTranslateOverride);

                        if (!moduleTranslateOverride && tabModule.IsTranslated != moduleTranslated)
                        {
                            ModuleController.UpdateTranslationStatus(tabModule, moduleTranslated);
                        }

                    }
                    else
                    {
                        var cbAddModule = (CheckBox)riDnnModule.FindControl("cbAddModule");
                        if (cbAddModule.Checked)
                        {
                            // find the first existing module on the line
                            foreach (RepeaterItem riCopy in rDnnModule.Items)
                            {
                                var hfTabModuleIDCopy = (HiddenField)riCopy.FindControl("hfTabModuleID");
                                int tabModuleIDCopy = int.Parse(hfTabModuleIDCopy.Value);
                                if (tabModuleIDCopy > 0)
                                {
                                    ModuleInfo miCopy = ModuleController.GetTabModule(tabModuleIDCopy);
                                    if (miCopy.DefaultLanguageGuid == Null.NullGuid)
                                    { // default 
                                        var hfTabID = (HiddenField)rDnnPage.Items[riDnnModule.ItemIndex].FindControl("hfTabID");
                                        var tabId = int.Parse(hfTabID.Value);
                                        var toTabInfo = TabController.GetTab(tabId, PortalSettings.PortalId, false);
                                        ModuleController.CopyModule(miCopy, toTabInfo, Null.NullString, true);
                                        var localizedModule = ModuleController.GetModule(miCopy.ModuleID, tabId);
                                        ModuleController.LocalizeModule(localizedModule,LocaleController.Instance.GetLocale(localizedModule.CultureCode));
                                    }
                                    else
                                    {
                                        var miCopyDefault = ModuleController.GetModuleByUniqueID(miCopy.DefaultLanguageGuid);
                                        var hfTabID = (HiddenField)rDnnPage.Items[riDnnModule.ItemIndex].FindControl("hfTabID");
                                        var tabId = int.Parse(hfTabID.Value);
                                        var toTabInfo = TabController.GetTab(tabId, PortalSettings.PortalId, false);
                                        ModuleController.CopyModule(miCopyDefault, toTabInfo, Null.NullString, true);
                                    }

                                    if (riDnnModule.ItemIndex == 0)
                                    { // default language
                                        ModuleInfo miDefault = null;
                                        foreach (RepeaterItem ri in rDnnPage.Items)
                                        {
                                            var hfTabID = (HiddenField)ri.FindControl("hfTabID");
                                            int tabID = int.Parse(hfTabID.Value);
                                            if (ri.ItemIndex == 0)
                                            {
                                                miDefault = ModuleController.GetModule(miCopy.ModuleID, tabID);
                                            }
                                            else
                                            {
                                                ModuleInfo moduleInfo = ModuleController.GetModule(miCopy.ModuleID, tabID);
                                                if (moduleInfo != null)
                                                {
                                                    if (miDefault != null)
                                                    {
                                                        moduleInfo.DefaultLanguageGuid = miDefault.UniqueId;
                                                    }
                                                    ModuleController.UpdateModule(moduleInfo);
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }

                            }

                        }
                    }
                }
            }

            var rFooter = (Repeater)Globals.FindControlRecursiveDown(rDnnModules, "rFooter");
            foreach (RepeaterItem repeaterItem in rFooter.Items)
            {
                var moduleTranslateOverride = false;

                var cbTranslated = (CheckBox)repeaterItem.FindControl("cbTranslated");
                var tabTranslated = cbTranslated.Checked;
                var cbPublish = (CheckBox)repeaterItem.FindControl("cbPublish");

                var hfTabId = (HiddenField)repeaterItem.FindControl("hfTabID");
                int tabID;
                var ConvertSuccess = int.TryParse(hfTabId.Value, out tabID);
                if (!ConvertSuccess)
                {
                    tabID = -1;
                }
                if (tabID > 0)
                {
                    var tabInfo = TabController.GetTab(tabID, PortalSettings.PortalId, true);
                    if (!tabInfo.IsDefaultLanguage)
                    {
                        if (tabInfo.IsTranslated != tabTranslated)
                        {
                            TabController.UpdateTranslationStatus(tabInfo, tabTranslated);

                            if (tabTranslated)
                            {
                                moduleTranslateOverride = true;

                                var tabModules = ModuleController.GetTabModules(tabInfo.TabID);
                                foreach (
                                    var moduleKVP in
                                        tabModules.Where(
                                            moduleKVP =>
                                            moduleKVP.Value.DefaultLanguageModule != null && moduleKVP.Value.LocalizedVersionGuid != moduleKVP.Value.DefaultLanguageModule.LocalizedVersionGuid))
                                {
                                    ModuleController.UpdateTranslationStatus(moduleKVP.Value, true);
                                }
                            }
                        }
                        if (cbPublish.Checked)
                        {
                            tabsToPublish.Add(tabInfo);
                        }
                    }
                }
                moduleTranslateOverrides.Add(tabID, moduleTranslateOverride);
            }

            // if we have tabs to publish, do it.
            // marks all modules as translated, marks page as translated
            foreach (var tabInfo in tabsToPublish)
            {
                //First mark all modules as translated
                foreach (ModuleInfo m in ModuleController.GetTabModules(tabInfo.TabID).Values)
                {
                    ModuleController.UpdateTranslationStatus(m, true);
                }

                //First mark tab as translated
                TabController.UpdateTranslationStatus(tabInfo, true);

                //Next publish Tab (update Permissions)
                TabController.PublishTab(tabInfo);
            }

            // manage translated status of tab. In order to do that, we need to check if all modules on the page are translated
            var tabTranslatedStatus = true;
            foreach (RepeaterItem repeaterItem in rDnnPage.Items)
            {
                var hfTabId = (HiddenField)repeaterItem.FindControl("hfTabID");
                int tabID;
                var ConvertSuccess = int.TryParse(hfTabId.Value, out tabID);
                if (!ConvertSuccess)
                {
                    tabID = -1;
                }
                if (tabID > 0)
                {
                    var tabInfo = TabController.GetTab(tabID, PortalSettings.PortalId, true);
                    if (tabInfo != null)
                    {
                        if (tabInfo.ChildModules.Any(moduleKVP => !moduleKVP.Value.IsTranslated))
                        {
                            tabTranslatedStatus = false;
                        }

                        if (tabTranslatedStatus && !tabInfo.IsTranslated)
                        {
                            TabController.UpdateTranslationStatus(tabInfo, true);
                        }
                    }
                }

            }


        }

        public void FixLocalizationErrors(int tabId)
        {

            BindAll(tabId);
            foreach (var dnnModules in Data.Modules)
            {
                foreach (var dnnModule in dnnModules.Modules)
                {
                    if (dnnModule.ErrorDefaultOnOtherTab) //the default language module is on an other page
                    {

                    }
                    else if (dnnModule.ErrorCultureOfModuleNotCultureOfTab)
                    { // # culture tab and module
                        ModuleInfo moduleInfo = ModuleController.GetTabModule(dnnModule.TabModuleID);
                        moduleInfo.CultureCode = dnnModule.CultureCode;
                        ModuleController.UpdateModule(moduleInfo);
                    }
                    else if (dnnModule.ErrorDuplicateModule) // duplicate
                    {
                        ModuleController.DeleteTabModule(dnnModule.TabId, dnnModule.ModuleID, true);
                    }
                }
            }
        }

        #endregion

        #region eventHandlers


        protected void Page_Load(object sender, EventArgs e)
        {
            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/Admin/Languages/CLTools.css");
            dummyGrid.Attributes.Add("style", "display:none");

        }


        protected void rDnnModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Header)
            {
                var rDnnPage = (Repeater)item.FindControl("rDnnPage");
                rDnnPage.DataSource = Data.Pages;
                rDnnPage.DataBind();

                var rHeader = (Repeater)item.FindControl("rHeader");
                rHeader.DataSource = Data.Pages;
                rHeader.DataBind();

                var rColHeader = (Repeater)item.FindControl("rColHeader");
                rColHeader.DataSource = Data.Pages;
                rColHeader.DataBind();

            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                var rFooter = (Repeater)item.FindControl("rFooter");
                rFooter.DataSource = Data.Pages;
                rFooter.DataBind();
            }
            else if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
            {
                var rDnnModule = (Repeater)item.FindControl("rDnnModule");
                var dnnModules = (DnnModules)item.DataItem;
                rDnnModule.DataSource = dnnModules.Modules;
                rDnnModule.DataBind();
            }
        }

        #endregion

        #region "classes for DataModel"


        protected class DnnPages
        {
            public DnnPages(List<string> locales)
            {
                _Locales = locales;
                Pages = new List<DnnPage>(); // one of each language
                Modules = new List<DnnModules>(); // one for each module on the page
                foreach (var locale in locales)
                {
                    Pages.Add(new DnnPage { CultureCode = locale });
                }
            }

            private readonly List<string> _Locales;
            public string[] Locales
            {
                get
                {
                    return _Locales.ToArray();
                }
            }

            public List<DnnPage> Pages { get; private set; }
            public List<DnnModules> Modules { get; private set; }

            public DnnPage Page(string locale)
            {
                return Pages.Single(pa => pa.CultureCode == locale);
            }

            public DnnModules Module(Guid UniqueId)
            {
                DnnModules m = Modules.SingleOrDefault(dm => dm.UniqueId == UniqueId);
                if (m == null)
                {
                    m = new DnnModules(Locales) { UniqueId = UniqueId };
                    Modules.Add(m);
                }
                return m;

            }

            public bool Error1(int ModuleId, Guid UniqueId, string CultureCode)
            {
                return Modules.Any(dm => dm.Modules.Any(mm => mm.ModuleID == ModuleId && mm.CultureCode != CultureCode) && dm.UniqueId != UniqueId);
            }

            public void RemoveLocale(string locale)
            {
                _Locales.Remove(locale);
            }

        }

        protected class DnnPage
        {
            public int? TabID { get; set; }
            public String TabName { get; set; }
            public String Title { get; set; }
            public String Description { get; set; }
            public String CultureCode { get; set; }
            public Guid DefaultLanguageGuid { get; set; }
            public bool IsTranslated { get; set; }
            public bool IsPublished { get; set; }
            public String Position { get; set; }
            public String Path { get; set; }
            public bool HasChildren { get; set; }
            public bool CanViewPage { get; set; }
            public bool CanAdminPage { get; set; }
            public string LocalResourceFile { get; set; }

            public bool TranslatedVisible
            {
                get
                {
                    return !Default && TabName != null;
                }
            }

            public bool PublishedVisible
            {
                get
                {
                    return !Default && IsTranslated;
                }
            }

            public bool Default
            {
                get
                {
                    return DefaultLanguageGuid == Null.NullGuid;
                }
            }

            public bool NotDefault
            {
                get
                {
                    return !Default;
                }
            }

            public string LanguageStatus
            {
                get
                {
                    var portalSettings = PortalController.GetCurrentPortalSettings();
                    if (CultureCode == portalSettings.DefaultLanguage) return Localization.GetString("Default.Text", LocalResourceFile);
                    if (!IsLanguagePublished(portalSettings.PortalId, CultureCode)) return Localization.GetString("NotActive.Text", LocalResourceFile);
                    return "";
                }
            }

            private bool IsLanguagePublished(int portalId, string Code)
            {
                bool isPublished = Null.NullBoolean;
                Locale enabledLanguage;
                if (LocaleController.Instance.GetLocales(portalId).TryGetValue(Code, out enabledLanguage))
                {
                    isPublished = enabledLanguage.IsPublished;
                }
                return isPublished;
            }


        }

        protected class DnnModules
        {
            public DnnModules(IEnumerable<string> Locales)
            {
                Modules = new List<DnnModule>(); // one module for each language
                foreach (var locale in Locales)
                {
                    Modules.Add(new DnnModule { CultureCode = locale });
                }
            }
            public Guid UniqueId { get; set; }
            public List<DnnModule> Modules { get; private set; }
            public DnnModule Module(string Locale)
            {
                return Modules.Single(mo => mo.CultureCode == Locale);
            }
        }

        protected class DnnModule
        {
            public String ModuleTitle { get; set; }
            public String CultureCode { get; set; }
            public Guid DefaultLanguageGuid { get; set; }
            public int TabId { get; set; }
            public int TabModuleID { get; set; }
            public int ModuleID { get; set; }
            public int DefaultModuleID { get; set; }
            public string DefaultTabName { get; set; }
            public bool IsTranslated { get; set; }
            public bool IsLocalized { get; set; }
            public bool IsShared { get; set; }
            public bool IsDeleted { get; set; }
            public bool ErrorDuplicateModule { get; set; }
            public bool ErrorDefaultOnOtherTab { get; set; }
            public bool ErrorCultureOfModuleNotCultureOfTab { get; set; }
            public string LocalResourceFile { get; set; }
            public bool CanViewModule { get; set; }
            public bool CanAdminModule { get; set; }
            public bool TranslatedVisible
            {
                get
                {
                    if (ErrorVisible)
                        return false;

                    if (CultureCode == null)
                        return false;
                    if (DefaultLanguageGuid == Null.NullGuid)
                        return false;
                    return ModuleID != DefaultModuleID;
                }
            }

            public bool LocalizedVisible
            {
                get
                {
                    if (ErrorVisible)
                        return false;

                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return false;
                    if (DefaultLanguageGuid == Null.NullGuid)
                        return false;
                    return true;
                }
            }

            public bool Exist
            {
                get
                {
                    return TabModuleID > 0;
                }
            }

            public bool NotExist
            {
                get
                {
                    return !Exist;
                }
            }

            public string TranslatedTooltip
            {
                get
                {
                    if (CultureCode == null)
                        return "";
                    if (DefaultLanguageGuid == Null.NullGuid)
                    {
                        return "";
                    }
                    string PageName = "";
                    if (DefaultTabName != null)
                        PageName = " / " + DefaultTabName;

                    if (ModuleID == DefaultModuleID)
                    {
                        return string.Format(Localization.GetString("Reference.Text", LocalResourceFile), PageName);

                    }
                    if (IsTranslated)
                    {
                        return string.Format(Localization.GetString("Translated.Text", LocalResourceFile), PageName);

                    }
                    return string.Format(Localization.GetString("NotTranslated.Text", LocalResourceFile), PageName);
                }
            }

            public string LocalizedTooltip
            {
                get
                {
                    if (CultureCode == null)
                        return "";
                    if (DefaultLanguageGuid == Null.NullGuid)
                    {
                        return "";
                    }
                    string PageName = "";
                    if (DefaultTabName != null)
                        PageName = " / " + DefaultTabName;

                    if (ModuleID == DefaultModuleID)
                    {
                        return string.Format(Localization.GetString("ReferenceDefault.Text", LocalResourceFile), PageName);

                    }
                    return string.Format(Localization.GetString("Detached.Text", LocalResourceFile), PageName);
                }
            }

            public bool ErrorVisible
            {
                get
                {
                    return ErrorDefaultOnOtherTab || ErrorCultureOfModuleNotCultureOfTab || ErrorDuplicateModule;
                }
            }

            public string ErrorToolTip
            {
                get
                {
                    if (ErrorDefaultOnOtherTab)
                    {
                        return "Default module on other tab";
                    }
                    if (ErrorCultureOfModuleNotCultureOfTab)
                    {
                        return "culture of module # culture of tab";
                    }
                    if (ErrorDuplicateModule)
                    {
                        return "Duplicate module";
                    }
                    return "";
                }
            }
        }

        #endregion

    }
}