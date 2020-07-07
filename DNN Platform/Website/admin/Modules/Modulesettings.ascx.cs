// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Modules

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// The ModuleSettingsPage PortalModuleBase is used to edit the settings for a
    /// module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class ModuleSettingsPage : PortalModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleSettingsPage));
        private readonly INavigationManager _navigationManager;

        private int _moduleId = -1;
        private Control _control;
        private ModuleInfo _module;

        public ModuleSettingsPage()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private bool HideDeleteButton => this.Request.QueryString["HideDelete"] == "true";

        private bool HideCancelButton => this.Request.QueryString["HideCancel"] == "true";

        private bool DoNotRedirectOnUpdate => this.Request.QueryString["NoRedirectOnUpdate"] == "true";

        private ModuleInfo Module
        {
            get { return this._module ?? (this._module = ModuleController.Instance.GetModule(this._moduleId, this.TabId, false)); }
        }

        private ISettingsControl SettingsControl
        {
            get
            {
                return this._control as ISettingsControl;
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this._navigationManager.NavigateURL();
            }
        }

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
                    PortalAlias = defaultAlias,
                };

                var tabUrl = this._navigationManager.NavigateURL(tab.TabID, portalSettings, string.Empty);

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
            string returnValue = string.Empty;
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
            return this.ModuleContext.Configuration.IsShared && this.ModuleContext.Configuration.IsShareableViewOnly;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                this.chkAllTabs.CheckedChanged += this.OnAllTabsCheckChanged;
                this.chkInheritPermissions.CheckedChanged += this.OnInheritPermissionsChanged;
                this.chkWebSlice.CheckedChanged += this.OnWebSliceCheckChanged;
                this.cboCacheProvider.TextChanged += this.OnCacheProviderIndexChanged;
                this.cmdDelete.Click += this.OnDeleteClick;
                this.cmdUpdate.Click += this.OnUpdateClick;

                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                // get ModuleId
                if (this.Request.QueryString["ModuleId"] != null)
                {
                    this._moduleId = int.Parse(this.Request.QueryString["ModuleId"]);
                }

                if (this.Module.ContentItemId == Null.NullInteger && this.Module.ModuleID != Null.NullInteger)
                {
                    // This tab does not have a valid ContentItem
                    ModuleController.Instance.CreateContentItem(this.Module);

                    ModuleController.Instance.UpdateModule(this.Module);
                }

                // Verify that the current user has access to edit this module
                if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", this.Module))
                {
                    if (!(this.IsSharedViewOnly() && TabPermissionController.CanAddContentToPage()))
                    {
                        this.Response.Redirect(Globals.AccessDeniedURL(), true);
                    }
                }

                if (this.Module != null)
                {
                    // get module
                    this.TabModuleId = this.Module.TabModuleID;

                    // get Settings Control
                    ModuleControlInfo moduleControlInfo = ModuleControlController.GetModuleControlByControlKey("Settings", this.Module.ModuleDefID);

                    if (moduleControlInfo != null)
                    {
                        this._control = ModuleControlFactory.LoadSettingsControl(this.Page, this.Module, moduleControlInfo.ControlSrc);

                        var settingsControl = this._control as ISettingsControl;
                        if (settingsControl != null)
                        {
                            this.hlSpecificSettings.Text = Localization.GetString(
                                "ControlTitle_settings",
                                settingsControl.LocalResourceFile);
                            if (string.IsNullOrEmpty(this.hlSpecificSettings.Text))
                            {
                                this.hlSpecificSettings.Text =
                                    string.Format(
                                        Localization.GetString("ControlTitle_settings", this.LocalResourceFile),
                                        this.Module.DesktopModule.FriendlyName);
                            }

                            this.pnlSpecific.Controls.Add(this._control);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Exceptions.ProcessModuleLoadException(this, err);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                this.cancelHyperLink.NavigateUrl = this.ReturnURL;

                if (this._moduleId != -1)
                {
                    this.ctlAudit.Entity = this.Module;
                }

                if (this.Page.IsPostBack == false)
                {
                    this.ctlIcon.FileFilter = Globals.glbImageFileTypes;

                    this.dgPermissions.TabId = this.PortalSettings.ActiveTab.TabID;
                    this.dgPermissions.ModuleID = this._moduleId;

                    this.BindModulePages();

                    this.cboTab.DataSource = TabController.GetPortalTabs(this.PortalId, -1, false, Null.NullString, true, false, true, false, true);
                    this.cboTab.DataBind();

                    // if tab is a  host tab, then add current tab
                    if (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID))
                    {
                        this.cboTab.InsertItem(0, this.PortalSettings.ActiveTab.LocalizedTabName, this.PortalSettings.ActiveTab.TabID.ToString());
                    }

                    if (this.Module != null)
                    {
                        if (this.cboTab.FindItemByValue(this.Module.TabID.ToString()) == null)
                        {
                            var objTab = TabController.Instance.GetTab(this.Module.TabID, this.Module.PortalID, false);
                            this.cboTab.AddItem(objTab.LocalizedTabName, objTab.TabID.ToString());
                        }
                    }

                    // only Portal Administrators can manage the visibility on all Tabs
                    var isAdmin = PermissionProvider.Instance().IsPortalEditor();
                    this.rowAllTabs.Visible = isAdmin;
                    this.chkAllModules.Enabled = isAdmin;

                    if (this.HideCancelButton)
                    {
                        this.cancelHyperLink.Visible = false;
                    }

                    // tab administrators can only manage their own tab
                    if (!TabPermissionController.CanAdminPage())
                    {
                        this.chkNewTabs.Enabled = false;
                        this.chkDefault.Enabled = false;
                        this.chkAllowIndex.Enabled = false;
                        this.cboTab.Enabled = false;
                    }

                    if (this._moduleId != -1)
                    {
                        this.BindData();
                        this.cmdDelete.Visible = (ModulePermissionController.CanDeleteModule(this.Module) ||
                             TabPermissionController.CanAddContentToPage()) && !this.HideDeleteButton;
                    }
                    else
                    {
                        this.isShareableCheckBox.Checked = true;
                        this.isShareableViewOnlyCheckBox.Checked = true;
                        this.isShareableRow.Visible = true;

                        this.cboVisibility.SelectedIndex = 0; // maximized
                        this.chkAllTabs.Checked = false;
                        this.cmdDelete.Visible = false;
                    }

                    if (this.Module != null)
                    {
                        this.cmdUpdate.Visible = ModulePermissionController.HasModulePermission(this.Module.ModulePermissions, "EDIT,MANAGE") || TabPermissionController.CanAddContentToPage();
                        this.permissionsRow.Visible = ModulePermissionController.CanAdminModule(this.Module) || TabPermissionController.CanAddContentToPage();
                    }

                    // Set visibility of Specific Settings
                    if (this.SettingsControl == null == false)
                    {
                        // Get the module settings from the PortalSettings and pass the
                        // two settings hashtables to the sub control to process
                        this.SettingsControl.LoadSettings();
                        this.specificSettingsTab.Visible = true;
                        this.fsSpecific.Visible = true;
                    }
                    else
                    {
                        this.specificSettingsTab.Visible = false;
                        this.fsSpecific.Visible = false;
                    }

                    if (this.Module != null)
                    {
                        this.termsSelector.PortalId = this.Module.PortalID;
                        this.termsSelector.Terms = this.Module.Terms;
                    }

                    this.termsSelector.DataBind();
                }

                if (this.Module != null)
                {
                    this.cultureLanguageLabel.Language = this.Module.CultureCode;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnAllTabsCheckChanged(object sender, EventArgs e)
        {
            this.trnewPages.Visible = this.chkAllTabs.Checked;
        }

        protected void OnCacheProviderIndexChanged(object sender, EventArgs e)
        {
            this.ShowCacheRows();
        }

        protected void OnDeleteClick(object sender, EventArgs e)
        {
            try
            {
                ModuleController.Instance.DeleteTabModule(this.TabId, this._moduleId, true);
                this.Response.Redirect(this.ReturnURL, true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnInheritPermissionsChanged(object sender, EventArgs e)
        {
            this.dgPermissions.InheritViewPermissionsFromTab = this.chkInheritPermissions.Checked;
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                if (this.Page.IsValid)
                {
                    var allTabsChanged = false;

                    // only Portal Administrators can manage the visibility on all Tabs
                    var isAdmin = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
                    this.chkAllModules.Enabled = isAdmin;

                    // tab administrators can only manage their own tab
                    if (!TabPermissionController.CanAdminPage())
                    {
                        this.chkAllTabs.Enabled = false;
                        this.chkNewTabs.Enabled = false;
                        this.chkDefault.Enabled = false;
                        this.chkAllowIndex.Enabled = false;
                        this.cboTab.Enabled = false;
                    }

                    this.Module.ModuleID = this._moduleId;
                    this.Module.ModuleTitle = this.txtTitle.Text;
                    this.Module.Alignment = this.cboAlign.SelectedItem.Value;
                    this.Module.Color = this.txtColor.Text;
                    this.Module.Border = this.txtBorder.Text;
                    this.Module.IconFile = this.ctlIcon.Url;
                    this.Module.CacheTime = !string.IsNullOrEmpty(this.txtCacheDuration.Text)
                                            ? int.Parse(this.txtCacheDuration.Text)
                                            : 0;
                    this.Module.CacheMethod = this.cboCacheProvider.SelectedValue;
                    this.Module.TabID = this.TabId;
                    if (this.Module.AllTabs != this.chkAllTabs.Checked)
                    {
                        allTabsChanged = true;
                    }

                    this.Module.AllTabs = this.chkAllTabs.Checked;

                    // collect these first as any settings update will clear the cache
                    var originalChecked = this.Settings["hideadminborder"] != null && bool.Parse(this.Settings["hideadminborder"].ToString());
                    var allowIndex = this.GetBooleanSetting("AllowIndex", true);
                    var oldMoniker = ((string)this.Settings["Moniker"] ?? string.Empty).TrimToLength(100);
                    var newMoniker = this.txtMoniker.Text.TrimToLength(100);
                    if (!oldMoniker.Equals(this.txtMoniker.Text))
                    {
                        var ids = TabModulesController.Instance.GetTabModuleIdsBySetting("Moniker", newMoniker);
                        if (ids != null && ids.Count > 0)
                        {
                            // Warn user - duplicate moniker value
                            Skin.AddModuleMessage(this, Localization.GetString("MonikerExists", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        ModuleController.Instance.UpdateTabModuleSetting(this.Module.TabModuleID, "Moniker", newMoniker);
                    }

                    if (originalChecked != this.chkAdminBorder.Checked)
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(this.Module.TabModuleID, "hideadminborder", this.chkAdminBorder.Checked.ToString());
                    }

                    // check whether allow index value is changed
                    if (allowIndex != this.chkAllowIndex.Checked)
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(this.Module.TabModuleID, "AllowIndex", this.chkAllowIndex.Checked.ToString());
                    }

                    switch (int.Parse(this.cboVisibility.SelectedItem.Value))
                    {
                        case 0:
                            this.Module.Visibility = VisibilityState.Maximized;
                            break;
                        case 1:
                            this.Module.Visibility = VisibilityState.Minimized;
                            break;

                        // case 2:
                        default:
                            this.Module.Visibility = VisibilityState.None;
                            break;
                    }

                    this.Module.IsDeleted = false;
                    this.Module.Header = this.txtHeader.Text;
                    this.Module.Footer = this.txtFooter.Text;

                    this.Module.StartDate = this.startDatePicker.SelectedDate != null
                                        ? this.startDatePicker.SelectedDate.Value
                                        : Null.NullDate;

                    this.Module.EndDate = this.endDatePicker.SelectedDate != null
                                        ? this.endDatePicker.SelectedDate.Value
                                        : Null.NullDate;

                    this.Module.ContainerSrc = this.moduleContainerCombo.SelectedValue;
                    this.Module.ModulePermissions.Clear();
                    this.Module.ModulePermissions.AddRange(this.dgPermissions.Permissions);
                    this.Module.Terms.Clear();
                    this.Module.Terms.AddRange(this.termsSelector.Terms);

                    if (!this.Module.IsShared)
                    {
                        this.Module.InheritViewPermissions = this.chkInheritPermissions.Checked;
                        this.Module.IsShareable = this.isShareableCheckBox.Checked;
                        this.Module.IsShareableViewOnly = this.isShareableViewOnlyCheckBox.Checked;
                    }

                    this.Module.DisplayTitle = this.chkDisplayTitle.Checked;
                    this.Module.DisplayPrint = this.chkDisplayPrint.Checked;
                    this.Module.DisplaySyndicate = this.chkDisplaySyndicate.Checked;
                    this.Module.IsWebSlice = this.chkWebSlice.Checked;
                    this.Module.WebSliceTitle = this.txtWebSliceTitle.Text;

                    this.Module.WebSliceExpiryDate = this.diWebSliceExpiry.SelectedDate != null
                                                ? this.diWebSliceExpiry.SelectedDate.Value
                                                : Null.NullDate;

                    if (!string.IsNullOrEmpty(this.txtWebSliceTTL.Text))
                    {
                        this.Module.WebSliceTTL = Convert.ToInt32(this.txtWebSliceTTL.Text);
                    }

                    this.Module.IsDefaultModule = this.chkDefault.Checked;
                    this.Module.AllModules = this.chkAllModules.Checked;
                    ModuleController.Instance.UpdateModule(this.Module);

                    // Update Custom Settings
                    if (this.SettingsControl != null)
                    {
                        try
                        {
                            this.SettingsControl.UpdateSettings();
                        }
                        catch (ThreadAbortException exc)
                        {
                            Logger.Debug(exc);

                            Thread.ResetAbort(); // necessary
                        }
                        catch (Exception ex)
                        {
                            Exceptions.LogException(ex);
                        }
                    }

                    // These Module Copy/Move statements must be
                    // at the end of the Update as the Controller code assumes all the
                    // Updates to the Module have been carried out.

                    // Check if the Module is to be Moved to a new Tab
                    if (!this.chkAllTabs.Checked)
                    {
                        var newTabId = int.Parse(this.cboTab.SelectedValue);
                        if (this.TabId != newTabId)
                        {
                            // First check if there already is an instance of the module on the target page
                            var tmpModule = ModuleController.Instance.GetModule(this._moduleId, newTabId, false);
                            if (tmpModule == null)
                            {
                                // Move module
                                ModuleController.Instance.MoveModule(this._moduleId, this.TabId, newTabId, Globals.glbDefaultPane);
                            }
                            else
                            {
                                // Warn user
                                Skin.AddModuleMessage(this, Localization.GetString("ModuleExists", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                                return;
                            }
                        }
                    }

                    // Check if Module is to be Added/Removed from all Tabs
                    if (allTabsChanged)
                    {
                        var listTabs = TabController.GetPortalTabs(this.PortalSettings.PortalId, Null.NullInteger, false, true);
                        if (this.chkAllTabs.Checked)
                        {
                            if (!this.chkNewTabs.Checked)
                            {
                                foreach (var destinationTab in listTabs)
                                {
                                    var module = ModuleController.Instance.GetModule(this._moduleId, destinationTab.TabID, false);
                                    if (module != null)
                                    {
                                        if (module.IsDeleted)
                                        {
                                            ModuleController.Instance.RestoreModule(module);
                                        }
                                    }
                                    else
                                    {
                                        if (!this.PortalSettings.ContentLocalizationEnabled || (this.Module.CultureCode == destinationTab.CultureCode))
                                        {
                                            ModuleController.Instance.CopyModule(this.Module, destinationTab, this.Module.PaneName, true);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ModuleController.Instance.DeleteAllModules(this._moduleId, this.TabId, listTabs, true, false, false);
                        }
                    }

                    if (!this.DoNotRedirectOnUpdate)
                    {
                        // Navigate back to admin page
                        this.Response.Redirect(this.ReturnURL, true);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnWebSliceCheckChanged(object sender, EventArgs e)
        {
            this.webSliceTitle.Visible = this.chkWebSlice.Checked;
            this.webSliceExpiry.Visible = this.chkWebSlice.Checked;
            this.webSliceTTL.Visible = this.chkWebSlice.Checked;
        }

        protected void dgOnTabs_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            this.dgOnTabs.PageIndex = e.NewPageIndex;
            this.BindModulePages();
        }

        private void BindData()
        {
            if (this.Module != null)
            {
                var desktopModule = DesktopModuleController.GetDesktopModule(this.Module.DesktopModuleID, this.PortalId);
                this.dgPermissions.ResourceFile = Globals.ApplicationPath + "/DesktopModules/" + desktopModule.FolderName + "/" + Localization.LocalResourceDirectory + "/" +
                                             Localization.LocalSharedResourceFile;
                if (!this.Module.IsShared)
                {
                    this.chkInheritPermissions.Checked = this.Module.InheritViewPermissions;
                    this.dgPermissions.InheritViewPermissionsFromTab = this.Module.InheritViewPermissions;
                }

                this.txtFriendlyName.Text = this.Module.DesktopModule.FriendlyName;
                this.txtTitle.Text = this.Module.ModuleTitle;
                this.ctlIcon.Url = this.Module.IconFile;

                if (this.cboTab.FindItemByValue(this.Module.TabID.ToString()) != null)
                {
                    this.cboTab.FindItemByValue(this.Module.TabID.ToString()).Selected = true;
                }

                this.rowTab.Visible = this.cboTab.Items.Count != 1;
                this.chkAllTabs.Checked = this.Module.AllTabs;
                this.trnewPages.Visible = this.chkAllTabs.Checked;
                this.allowIndexRow.Visible = desktopModule.IsSearchable;
                this.chkAllowIndex.Checked = this.GetBooleanSetting("AllowIndex", true);
                this.txtMoniker.Text = (string)this.Settings["Moniker"] ?? string.Empty;

                this.cboVisibility.SelectedIndex = (int)this.Module.Visibility;
                this.chkAdminBorder.Checked = this.Settings["hideadminborder"] != null && bool.Parse(this.Settings["hideadminborder"].ToString());

                var objModuleDef = ModuleDefinitionController.GetModuleDefinitionByID(this.Module.ModuleDefID);
                if (objModuleDef.DefaultCacheTime == Null.NullInteger)
                {
                    this.cacheWarningRow.Visible = true;
                    this.txtCacheDuration.Text = this.Module.CacheTime.ToString();
                }
                else
                {
                    this.cacheWarningRow.Visible = false;
                    this.txtCacheDuration.Text = this.Module.CacheTime.ToString();
                }

                this.BindModuleCacheProviderList();

                this.ShowCacheRows();

                this.cboAlign.Items.FindByValue(this.Module.Alignment).Selected = true;
                this.txtColor.Text = this.Module.Color;
                this.txtBorder.Text = this.Module.Border;

                this.txtHeader.Text = this.Module.Header;
                this.txtFooter.Text = this.Module.Footer;

                if (!Null.IsNull(this.Module.StartDate))
                {
                    this.startDatePicker.SelectedDate = this.Module.StartDate;
                }

                if (!Null.IsNull(this.Module.EndDate) && this.Module.EndDate <= this.endDatePicker.MaxDate)
                {
                    this.endDatePicker.SelectedDate = this.Module.EndDate;
                }

                this.BindContainers();

                this.chkDisplayTitle.Checked = this.Module.DisplayTitle;
                this.chkDisplayPrint.Checked = this.Module.DisplayPrint;
                this.chkDisplaySyndicate.Checked = this.Module.DisplaySyndicate;

                this.chkWebSlice.Checked = this.Module.IsWebSlice;
                this.webSliceTitle.Visible = this.Module.IsWebSlice;
                this.webSliceExpiry.Visible = this.Module.IsWebSlice;
                this.webSliceTTL.Visible = this.Module.IsWebSlice;

                this.txtWebSliceTitle.Text = this.Module.WebSliceTitle;
                if (!Null.IsNull(this.Module.WebSliceExpiryDate))
                {
                    this.diWebSliceExpiry.SelectedDate = this.Module.WebSliceExpiryDate;
                }

                if (!Null.IsNull(this.Module.WebSliceTTL))
                {
                    this.txtWebSliceTTL.Text = this.Module.WebSliceTTL.ToString();
                }

                if (this.Module.ModuleID == PortalSettings.Current.DefaultModuleId && this.Module.TabID == PortalSettings.Current.DefaultTabId)
                {
                    this.chkDefault.Checked = true;
                }

                if (!this.Module.IsShared && this.Module.DesktopModule.Shareable != ModuleSharing.Unsupported)
                {
                    this.isShareableCheckBox.Checked = this.Module.IsShareable;
                    this.isShareableViewOnlyCheckBox.Checked = this.Module.IsShareableViewOnly;
                    this.isShareableRow.Visible = true;

                    this.chkInheritPermissions.Visible = true;
                }
            }
        }

        private void BindContainers()
        {
            this.moduleContainerCombo.PortalId = this.PortalId;
            this.moduleContainerCombo.RootPath = SkinController.RootContainer;
            this.moduleContainerCombo.Scope = SkinScope.All;
            this.moduleContainerCombo.IncludeNoneSpecificItem = true;
            this.moduleContainerCombo.NoneSpecificText = "<" + Localization.GetString("None_Specified") + ">";
            this.moduleContainerCombo.SelectedValue = this.Module.ContainerSrc;
        }

        private void BindModulePages()
        {
            var tabsByModule = TabController.Instance.GetTabsByModuleID(this._moduleId);
            tabsByModule.Remove(this.TabId);
            this.dgOnTabs.DataSource = tabsByModule.Values;
            this.dgOnTabs.DataBind();
        }

        private void BindModuleCacheProviderList()
        {
            this.cboCacheProvider.DataSource = this.GetFilteredProviders(ModuleCachingProvider.GetProviderList(), "ModuleCachingProvider");
            this.cboCacheProvider.DataBind();

            // cboCacheProvider.Items.Insert(0, new ListItem(Localization.GetString("None_Specified"), ""));
            this.cboCacheProvider.InsertItem(0, Localization.GetString("None_Specified"), string.Empty);

            // if (!string.IsNullOrEmpty(Module.GetEffectiveCacheMethod()) && cboCacheProvider.Items.FindByValue(Module.GetEffectiveCacheMethod()) != null)
            if (!string.IsNullOrEmpty(this.Module.GetEffectiveCacheMethod()) && this.cboCacheProvider.FindItemByValue(this.Module.GetEffectiveCacheMethod()) != null)
            {
                // cboCacheProvider.Items.FindByValue(Module.GetEffectiveCacheMethod()).Selected = true;
                this.cboCacheProvider.FindItemByValue(this.Module.GetEffectiveCacheMethod()).Selected = true;
            }
            else
            {
                // select the None Specified value
                this.cboCacheProvider.Items[0].Selected = true;
            }

            this.lblCacheInherited.Visible = this.Module.CacheMethod != this.Module.GetEffectiveCacheMethod();
        }

        private IEnumerable GetFilteredProviders<T>(Dictionary<string, T> providerList, string keyFilter)
        {
            var providers = from provider in providerList let filteredkey = provider.Key.Replace(keyFilter, string.Empty) select new { filteredkey, provider.Key };

            return providers;
        }

        private void ShowCacheRows()
        {
            this.divCacheDuration.Visible = !string.IsNullOrEmpty(this.cboCacheProvider.SelectedValue);
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
            var value = this.Settings[settingName];

            return value == null
                ? defaultValue
                : bool.Parse(value.ToString());
        }
    }
}
