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
using System.Threading;
using System.Web.UI;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.UI;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using Globals = DotNetNuke.Common.Globals;
using DotNetNuke.Instrumentation;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Modules
// ReSharper restore CheckNamespace
{

    /// <summary>
    /// The ModuleSettingsPage PortalModuleBase is used to edit the settings for a 
    /// module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	10/18/2004	documented
    /// 	[cnurse]	10/19/2004	modified to support custm module specific settings
    /// </history>
    public partial class ModuleSettingsPage : PortalModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleSettingsPage));

        #region Private Members

        private int _moduleId = -1;
        private Control _control;
        private ModuleInfo _module;

        private ModuleInfo Module
        {
            get { return _module ?? (_module = ModuleController.Instance.GetModule(_moduleId, TabId, false)); }
        }

        private ISettingsControl SettingsControl
        {
            get
            {
                return _control as ISettingsControl;
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(Request.Params["ReturnURL"]) ?? Globals.NavigateURL();
            }
        }

        #endregion

        #region Private Methods

        private void BindData()
        {
            if (Module != null)
            {
                var desktopModule = DesktopModuleController.GetDesktopModule(Module.DesktopModuleID, PortalId);
                dgPermissions.ResourceFile = Globals.ApplicationPath + "/DesktopModules/" + desktopModule.FolderName + "/" + Localization.LocalResourceDirectory + "/" +
                                             Localization.LocalSharedResourceFile;
                if (!Module.IsShared)
                {
                    chkInheritPermissions.Checked = Module.InheritViewPermissions;
                    dgPermissions.InheritViewPermissionsFromTab = Module.InheritViewPermissions;
                }
                txtFriendlyName.Text = Module.DesktopModule.FriendlyName;
                txtTitle.Text = Module.ModuleTitle;
                ctlIcon.Url = Module.IconFile;

                if (cboTab.FindItemByValue(Module.TabID.ToString()) != null)
                {
                    cboTab.FindItemByValue(Module.TabID.ToString()).Selected = true;
                }

                rowTab.Visible = cboTab.Items.Count != 1;
                chkAllTabs.Checked = Module.AllTabs;
                trnewPages.Visible = chkAllTabs.Checked;
                allowIndexRow.Visible = desktopModule.IsSearchable;
                chkAllowIndex.Checked = Settings["AllowIndex"] == null || Settings["AllowIndex"] != null && bool.Parse(Settings["AllowIndex"].ToString());
                

                cboVisibility.SelectedIndex = (int)Module.Visibility;
                chkAdminBorder.Checked = Settings["hideadminborder"] != null && bool.Parse(Settings["hideadminborder"].ToString());

                var objModuleDef = ModuleDefinitionController.GetModuleDefinitionByID(Module.ModuleDefID);
                if (objModuleDef.DefaultCacheTime == Null.NullInteger)
                {
                    cacheWarningRow.Visible = true;
                    txtCacheDuration.Text = Module.CacheTime.ToString();
                }
                else
                {
                    cacheWarningRow.Visible = false;
                    txtCacheDuration.Text = Module.CacheTime.ToString();
                }
                BindModuleCacheProviderList();

                ShowCacheRows();

                cboAlign.Items.FindByValue(Module.Alignment).Selected = true;
                txtColor.Text = Module.Color;
                txtBorder.Text = Module.Border;

                txtHeader.Text = Module.Header;
                txtFooter.Text = Module.Footer;

                if (!Null.IsNull(Module.StartDate))
                {
                    startDatePicker.SelectedDate = Module.StartDate;
                }
                if (!Null.IsNull(Module.EndDate))
                {
                    endDatePicker.SelectedDate = Module.EndDate;
                }

                BindContainers();

                chkDisplayTitle.Checked = Module.DisplayTitle;
                chkDisplayPrint.Checked = Module.DisplayPrint;
                chkDisplaySyndicate.Checked = Module.DisplaySyndicate;

                chkWebSlice.Checked = Module.IsWebSlice;
                webSliceTitle.Visible = Module.IsWebSlice;
                webSliceExpiry.Visible = Module.IsWebSlice;
                webSliceTTL.Visible = Module.IsWebSlice;

                txtWebSliceTitle.Text = Module.WebSliceTitle;
                if (!Null.IsNull(Module.WebSliceExpiryDate))
                {
                    diWebSliceExpiry.SelectedDate = Module.WebSliceExpiryDate;
                }
                if (!Null.IsNull(Module.WebSliceTTL))
                {
                    txtWebSliceTTL.Text = Module.WebSliceTTL.ToString();
                }
                if (Module.ModuleID == PortalSettings.Current.DefaultModuleId && Module.TabID == PortalSettings.Current.DefaultTabId)
                {
                    chkDefault.Checked = true;
                }

                if (!Module.IsShared && Module.DesktopModule.Shareable != ModuleSharing.Unsupported)
                {
                    isShareableCheckBox.Checked = Module.IsShareable;
                    isShareableViewOnlyCheckBox.Checked = Module.IsShareableViewOnly;
                    isShareableRow.Visible = true;

                    chkInheritPermissions.Visible = true;
                }
            }
        }

        private void BindContainers()
        {
            moduleContainerCombo.PortalId = PortalId;
            moduleContainerCombo.RootPath = SkinController.RootContainer;
            moduleContainerCombo.Scope = SkinScope.All;
            moduleContainerCombo.IncludeNoneSpecificItem = true;
            moduleContainerCombo.NoneSpecificText = "<" + Localization.GetString("None_Specified") + ">";
            moduleContainerCombo.SelectedValue = Module.ContainerSrc;
        }

        private void BindModuleCacheProviderList()
        {
            cboCacheProvider.DataSource = GetFilteredProviders(ModuleCachingProvider.GetProviderList(), "ModuleCachingProvider");
            cboCacheProvider.DataBind();

            //cboCacheProvider.Items.Insert(0, new ListItem(Localization.GetString("None_Specified"), ""));
            cboCacheProvider.InsertItem(0, Localization.GetString("None_Specified"), "");

            //if (!string.IsNullOrEmpty(Module.GetEffectiveCacheMethod()) && cboCacheProvider.Items.FindByValue(Module.GetEffectiveCacheMethod()) != null)
            if (!string.IsNullOrEmpty(Module.GetEffectiveCacheMethod()) && cboCacheProvider.FindItemByValue(Module.GetEffectiveCacheMethod()) != null)
            {
                //cboCacheProvider.Items.FindByValue(Module.GetEffectiveCacheMethod()).Selected = true;
                cboCacheProvider.FindItemByValue(Module.GetEffectiveCacheMethod()).Selected = true;
            }
            else
            {
                //select the None Specified value
                cboCacheProvider.Items[0].Selected = true;
            }

            lblCacheInherited.Visible = Module.CacheMethod != Module.GetEffectiveCacheMethod();
        }

        private IEnumerable GetFilteredProviders<T>(Dictionary<string, T> providerList, string keyFilter)
        {
            var providers = from provider in providerList let filteredkey = provider.Key.Replace(keyFilter, String.Empty) select new { filteredkey, provider.Key };

            return providers;
        }

        private void ShowCacheRows()
        {
            divCacheDuration.Visible = !string.IsNullOrEmpty(cboCacheProvider.SelectedValue);
        }

        #endregion

        #region Protected Methods

        protected string GetInstalledOnLink(object dataItem)
        {
            var returnValue = new StringBuilder();
            var tab = dataItem as TabInfo;
            if (tab != null)
            {
                var index = 0;
                TabController.Instance.PopulateBreadCrumbs(ref tab);
                var defaultAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(tab.PortalID)
                                        .OrderByDescending(a => a.IsPrimary)
                                        .FirstOrDefault();
                var portalSettings = new PortalSettings(tab.PortalID)
                                         {
                                             PortalAlias = defaultAlias
                                         };

                var tabUrl = Globals.NavigateURL(tab.TabID, portalSettings, string.Empty);

                foreach (TabInfo t in tab.BreadCrumbs)
                {
                    if (index > 0)
                    {
                        returnValue.Append(" > ");
                    }
                    if (tab.BreadCrumbs.Count - 1 == index)
                    {
                        returnValue.AppendFormat("<a href=\"{0}\">{1}</a>", tabUrl, t.LocalizedTabName);
                    }
                    else
                    {
                        returnValue.AppendFormat("{0}", t.LocalizedTabName);
                    }
                    index = index + 1;
                }
            }
            return returnValue.ToString();
        }

        protected string GetInstalledOnSite(object dataItem)
        {
            string returnValue = String.Empty;
            var tab = dataItem as TabInfo;
            if (tab != null)
            {
                var portal = PortalController.Instance.GetPortal(tab.PortalID);
                if (portal != null)
                {
                    returnValue = portal.PortalName;
                }
            }
            return returnValue;
        }

        protected bool IsSharedViewOnly()
        {
            return ModuleContext.Configuration.IsShared && ModuleContext.Configuration.IsShareableViewOnly;
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            chkAllTabs.CheckedChanged += OnAllTabsCheckChanged;
            chkInheritPermissions.CheckedChanged += OnInheritPermissionsChanged;
            chkWebSlice.CheckedChanged += OnWebSliceCheckChanged;
            cboCacheProvider.TextChanged += OnCacheProviderIndexChanged;
            cmdDelete.Click += OnDeleteClick;
            cmdUpdate.Click += OnUpdateClick;
            dgOnTabs.NeedDataSource += OnPagesGridNeedDataSource;

            jQuery.RequestDnnPluginsRegistration();

            //get ModuleId
            if ((Request.QueryString["ModuleId"] != null))
            {
                _moduleId = Int32.Parse(Request.QueryString["ModuleId"]);
            }
            if (Module.ContentItemId == Null.NullInteger && Module.ModuleID != Null.NullInteger)
            {
                //This tab does not have a valid ContentItem
                ModuleController.Instance.CreateContentItem(Module);

                ModuleController.Instance.UpdateModule(Module);
            }

            //Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", Module))
            {
                if (!(IsSharedViewOnly() && TabPermissionController.CanAddContentToPage()))
                {
                    Response.Redirect(Globals.AccessDeniedURL(), true);
                }
            }
            if (Module != null)
            {
                //get module
                TabModuleId = Module.TabModuleID;

                //get Settings Control
                ModuleControlInfo moduleControlInfo = ModuleControlController.GetModuleControlByControlKey("Settings", Module.ModuleDefID);

                if (moduleControlInfo != null)
                {
                    _control = ControlUtilities.LoadControl<Control>(Page, moduleControlInfo.ControlSrc);

                    var settingsControl = _control as ISettingsControl;
                    if (settingsControl != null)
                    {
                        //Set ID
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(moduleControlInfo.ControlSrc);
                        if (fileNameWithoutExtension != null)
                        {
                            _control.ID = fileNameWithoutExtension.Replace('.', '-');
                        }

                        //add module settings
                        settingsControl.ModuleContext.Configuration = Module;

                        hlSpecificSettings.Text = Localization.GetString("ControlTitle_settings", settingsControl.LocalResourceFile);
                        if (String.IsNullOrEmpty(hlSpecificSettings.Text))
                        {
                            hlSpecificSettings.Text = String.Format(Localization.GetString("ControlTitle_settings", LocalResourceFile), Module.DesktopModule.FriendlyName);
                        }
                        pnlSpecific.Controls.Add(_control);
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                cancelHyperLink.NavigateUrl = ReturnURL;

                if (_moduleId != -1)
                {
                    ctlAudit.Entity = Module;
                }
                if (Page.IsPostBack == false)
                {
                    ctlIcon.FileFilter = Globals.glbImageFileTypes;

                    dgPermissions.TabId = PortalSettings.ActiveTab.TabID;
                    dgPermissions.ModuleID = _moduleId;


                    cboTab.DataSource = TabController.GetPortalTabs(PortalId, -1, false, Null.NullString, true, false, true, false, true);
                    cboTab.DataBind();

                    //if tab is a  host tab, then add current tab
                    if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                    {
                        cboTab.InsertItem(0, PortalSettings.ActiveTab.LocalizedTabName, PortalSettings.ActiveTab.TabID.ToString());
                    }
                    if (Module != null)
                    {
                        if (cboTab.FindItemByValue(Module.TabID.ToString()) == null)
                        {
                            var objTab = TabController.Instance.GetTab(Module.TabID, Module.PortalID, false);
                            cboTab.AddItem(objTab.LocalizedTabName, objTab.TabID.ToString());
                        }
                    }

                    //only Portal Administrators can manage the visibility on all Tabs
                    rowAllTabs.Visible = PortalSecurity.IsInRole("Administrators");

                    //tab administrators can only manage their own tab
                    if (!TabPermissionController.CanAdminPage())
                    {
                        chkNewTabs.Enabled = false;
                        chkDefault.Enabled = false;
                        chkAllModules.Enabled = false;
                        chkAllowIndex.Enabled = false;
                        cboTab.Enabled = false;
                    }
                    if (_moduleId != -1)
                    {
                        BindData();
                        cmdDelete.Visible = ModulePermissionController.CanDeleteModule(Module) || TabPermissionController.CanAddContentToPage();
                    }
                    else
                    {
                        isShareableCheckBox.Checked = true;
                        isShareableViewOnlyCheckBox.Checked = true;
                        isShareableRow.Visible = true;

                        cboVisibility.SelectedIndex = 0; //maximized
                        chkAllTabs.Checked = false;
                        cmdDelete.Visible = false;
                    }
                    if (Module != null)
                    {
                        cmdUpdate.Visible = ModulePermissionController.HasModulePermission(Module.ModulePermissions, "EDIT,MANAGE") || TabPermissionController.CanAddContentToPage();
                        permissionsRow.Visible = ModulePermissionController.CanAdminModule(Module) || TabPermissionController.CanAddContentToPage();
                    }

                    //Set visibility of Specific Settings
                    if (SettingsControl == null == false)
                    {
                        //Get the module settings from the PortalSettings and pass the
                        //two settings hashtables to the sub control to process
                        SettingsControl.LoadSettings();
                        specificSettingsTab.Visible = true;
                        fsSpecific.Visible = true;
                    }
                    else
                    {
                        specificSettingsTab.Visible = false;
                        fsSpecific.Visible = false;
                    }

                    if (Module != null)
                    {
                        termsSelector.PortalId = Module.PortalID;
                        termsSelector.Terms = Module.Terms;
                    }
                    termsSelector.DataBind();
                }
                if (Module != null)
                {
                    cultureLanguageLabel.Language = Module.CultureCode;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnAllTabsCheckChanged(object sender, EventArgs e)
        {
            trnewPages.Visible = chkAllTabs.Checked;
        }

        protected void OnCacheProviderIndexChanged(object sender, EventArgs e)
        {
            ShowCacheRows();
        }

        protected void OnDeleteClick(Object sender, EventArgs e)
        {
            try
            {
                ModuleController.Instance.DeleteTabModule(TabId, _moduleId, true);
                Response.Redirect(ReturnURL, true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnInheritPermissionsChanged(Object sender, EventArgs e)
        {
            dgPermissions.InheritViewPermissionsFromTab = chkInheritPermissions.Checked;
        }

        protected void OnPagesGridNeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            var tabsByModule = TabController.Instance.GetTabsByModuleID(_moduleId);
            tabsByModule.Remove(TabId);
            dgOnTabs.DataSource = tabsByModule.Values;
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    var allTabsChanged = false;
                    //TODO: REMOVE IF UNUSED
                    //var allowIndexChanged = false;

                    //tab administrators can only manage their own tab
                    if (!TabPermissionController.CanAdminPage())
                    {
                        chkAllTabs.Enabled = false;
                        chkNewTabs.Enabled = false;
                        chkDefault.Enabled = false;
                        chkAllModules.Enabled = false;
                        chkAllowIndex.Enabled = false;
                        cboTab.Enabled = false;
                    }
                    Module.ModuleID = _moduleId;
                    Module.ModuleTitle = txtTitle.Text;
                    Module.Alignment = cboAlign.SelectedItem.Value;
                    Module.Color = txtColor.Text;
                    Module.Border = txtBorder.Text;
                    Module.IconFile = ctlIcon.Url;
                    Module.CacheTime = !String.IsNullOrEmpty(txtCacheDuration.Text)
                                            ? Int32.Parse(txtCacheDuration.Text)
                                            : 0;
                    Module.CacheMethod = cboCacheProvider.SelectedValue;
                    Module.TabID = TabId;
                    if (Module.AllTabs != chkAllTabs.Checked)
                    {
                        allTabsChanged = true;
                    }
                    Module.AllTabs = chkAllTabs.Checked;
                    ModuleController.Instance.UpdateTabModuleSetting(Module.TabModuleID, "hideadminborder", chkAdminBorder.Checked.ToString());

                    //check whether allow index value is changed
                    var allowIndex = Settings.ContainsKey("AllowIndex") && Convert.ToBoolean(Settings["AllowIndex"]);
                    if (allowIndex != chkAllowIndex.Checked)
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(Module.TabModuleID, "AllowIndex", chkAllowIndex.Checked ? "True" : "False");
                    }
                    ModuleController.Instance.UpdateTabModuleSetting(Module.TabModuleID, "AllowIndex", chkAllowIndex.Checked.ToString());


                    switch (Int32.Parse(cboVisibility.SelectedItem.Value))
                    {
                        case 0:
                            Module.Visibility = VisibilityState.Maximized;
                            break;
                        case 1:
                            Module.Visibility = VisibilityState.Minimized;
                            break;
                        case 2:
                            Module.Visibility = VisibilityState.None;
                            break;
                    }
                    Module.IsDeleted = false;
                    Module.Header = txtHeader.Text;
                    Module.Footer = txtFooter.Text;

                    Module.StartDate = startDatePicker.SelectedDate != null
                                        ? startDatePicker.SelectedDate.Value
                                        : Null.NullDate;

                    Module.EndDate = endDatePicker.SelectedDate != null
                                        ? endDatePicker.SelectedDate.Value
                                        : Null.NullDate;

                    Module.ContainerSrc = moduleContainerCombo.SelectedValue;
                    Module.ModulePermissions.Clear();
                    Module.ModulePermissions.AddRange(dgPermissions.Permissions);
                    Module.Terms.Clear();
                    Module.Terms.AddRange(termsSelector.Terms);

                    if (!Module.IsShared)
                    {
                        Module.InheritViewPermissions = chkInheritPermissions.Checked;
                        Module.IsShareable = isShareableCheckBox.Checked;
                        Module.IsShareableViewOnly = isShareableViewOnlyCheckBox.Checked;
                    }

                    Module.DisplayTitle = chkDisplayTitle.Checked;
                    Module.DisplayPrint = chkDisplayPrint.Checked;
                    Module.DisplaySyndicate = chkDisplaySyndicate.Checked;
                    Module.IsWebSlice = chkWebSlice.Checked;
                    Module.WebSliceTitle = txtWebSliceTitle.Text;

                    Module.WebSliceExpiryDate = diWebSliceExpiry.SelectedDate != null
                                                ? diWebSliceExpiry.SelectedDate.Value
                                                : Null.NullDate;

                    if (!string.IsNullOrEmpty(txtWebSliceTTL.Text))
                    {
                        Module.WebSliceTTL = Convert.ToInt32(txtWebSliceTTL.Text);
                    }
                    Module.IsDefaultModule = chkDefault.Checked;
                    Module.AllModules = chkAllModules.Checked;
                    ModuleController.Instance.UpdateModule(Module);

                    //Update Custom Settings
                    if (SettingsControl != null)
                    {
                        try
                        {
                            SettingsControl.UpdateSettings();
                        }
                        catch (ThreadAbortException exc)
                        {
                            Logger.Debug(exc);

                            Thread.ResetAbort(); //necessary
                        }
                        catch (Exception ex)
                        {
                            Exceptions.LogException(ex);
                        }
                    }

                    //These Module Copy/Move statements must be 
                    //at the end of the Update as the Controller code assumes all the 
                    //Updates to the Module have been carried out.

                    //Check if the Module is to be Moved to a new Tab
                    if (!chkAllTabs.Checked)
                    {
                        var newTabId = Int32.Parse(cboTab.SelectedValue);
                        if (TabId != newTabId)
                        {
                            //First check if there already is an instance of the module on the target page
                            var tmpModule = ModuleController.Instance.GetModule(_moduleId, newTabId, false);
                            if (tmpModule == null)
                            {
                                //Move module
                                ModuleController.Instance.MoveModule(_moduleId, TabId, newTabId, Globals.glbDefaultPane);
                            }
                            else
                            {
                                //Warn user
                                Skin.AddModuleMessage(this, Localization.GetString("ModuleExists", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                                return;
                            }
                        }
                    }

                    //Check if Module is to be Added/Removed from all Tabs
                    if (allTabsChanged)
                    {
                        var listTabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, true);
                        if (chkAllTabs.Checked)
                        {
                            if (!chkNewTabs.Checked)
                            {
                                foreach (var destinationTab in listTabs)
                                {
                                    var module = ModuleController.Instance.GetModule(_moduleId, destinationTab.TabID, false);
                                    if (module != null)
                                    {
                                        if (module.IsDeleted)
                                        {
                                            ModuleController.Instance.RestoreModule(module);
                                        }
                                    }
                                    else
                                    {
                                        if (!PortalSettings.ContentLocalizationEnabled || (Module.CultureCode == destinationTab.CultureCode))
                                        {
                                            ModuleController.Instance.CopyModule(Module, destinationTab, Module.PaneName, true);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ModuleController.Instance.DeleteAllModules(_moduleId, TabId, listTabs, true, false, false);
                        }
                    }

                    //Navigate back to admin page
                    Response.Redirect(ReturnURL, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnWebSliceCheckChanged(object sender, EventArgs e)
        {
            webSliceTitle.Visible = chkWebSlice.Checked;
            webSliceExpiry.Visible = chkWebSlice.Checked;
            webSliceTTL.Visible = chkWebSlice.Checked;
        }

        #endregion

    }
}