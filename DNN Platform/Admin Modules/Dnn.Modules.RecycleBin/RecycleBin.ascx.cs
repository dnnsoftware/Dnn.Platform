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
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DesktopModules.Admin.RecycleBin
{

	/// <summary>
	/// The RecycleBin PortalModuleBase allows Tabs and Modules to be recovered or
	/// prmanentl deleted
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// 	[cnurse]	9/15/2004	Updated to reflect design changes for Help, 508 support
	///                       and localisation
	/// </history>
	public partial class RecycleBin : PortalModuleBase
    {
        #region Protected Properties

        protected List<TabInfo> DeletedTabs { get; private set; }

        protected List<ModuleInfo> DeletedModules { get; private set; }

        #endregion

        #region Private Methods

		private void BindData(bool refresh)
		{
            if (refresh)
            {
                LoadData();
            }

            modulesListBox.Items.Clear();
            tabsListBox.Items.Clear();

            foreach (ModuleInfo module in DeletedModules)
            {
                if (String.IsNullOrEmpty(module.ModuleTitle))
                {
                    module.ModuleTitle = module.DesktopModule.FriendlyName;
                }

                var locale = LocaleController.Instance.GetLocale(module.CultureCode);

                TabInfo tab = locale != null
                                ? TabController.Instance.GetTabByCulture(module.TabID, PortalId, locale)
                                : TabController.Instance.GetTab(module.TabID, PortalId, false);

                if (tab == null)
                {
                    modulesListBox.Items.Add(new ListItem(module.ModuleTitle + " - " + module.LastModifiedOnDate, module.TabID + "-" + module.ModuleID));
                }
                else if (tab.TabID == module.TabID)
                {
                    modulesListBox.Items.Add(new ListItem(tab.TabName + " - " + module.ModuleTitle + " - " + module.LastModifiedOnDate, module.TabID + "-" + module.ModuleID));
                }
            }

            foreach (var tab in DeletedTabs)
            {
	            var item = new ListItem(tab.IndentedTabName + " - " + tab.LastModifiedOnDate, tab.TabID.ToString());
				item.Attributes.Add("ParentId", tab.ParentId.ToString());
				tabsListBox.Items.Add(item);
            }

            cmdRestoreTab.Enabled = (DeletedTabs.Count > 0);
            cmdDeleteTab.Enabled = (DeletedTabs.Count > 0);
            cmdRestoreModule.Enabled = (modulesListBox.Items.Count > 0);
            cmdDeleteModule.Enabled = (modulesListBox.Items.Count > 0);
            cmdEmpty.Enabled = DeletedTabs.Count > 0 || modulesListBox.Items.Count > 0;
		}

        private void DeleteModule(ModuleInfo module)
        {
            //hard-delete Tab Module Instance
            ModuleController.Instance.DeleteTabModule(module.TabID, module.ModuleID, false);
            EventLogController.Instance.AddLog(module, PortalSettings, UserId, "", EventLogController.EventLogType.MODULE_DELETED);
        }

	    private void DeleteTab(TabInfo tab, bool deleteDescendants)
		{
			//get tab modules before deleting page
            var tabModules = ModuleController.Instance.GetTabModules(tab.TabID);

			//hard delete the tab
			TabController.Instance.DeleteTab(tab.TabID, tab.PortalID, deleteDescendants);
			
			//delete modules that do not have other instances
			foreach (var kvp in tabModules)
			{
				//check if all modules instances have been deleted
                var delModule = ModuleController.Instance.GetModule(kvp.Value.ModuleID, Null.NullInteger, false);
				if (delModule == null || delModule.TabID == Null.NullInteger)
				{
                    ModuleController.Instance.DeleteModule(kvp.Value.ModuleID);
				}
			}
            EventLogController.Instance.AddLog(tab, PortalSettings, UserId, "", EventLogController.EventLogType.TAB_DELETED);
		}

        private void LoadData()
        {
            var currentLocale = LocaleController.Instance.GetCurrentLocale(PortalId);

            TabCollection tabsList = modeButtonList.SelectedValue == "ALL"
                             ? TabController.Instance.GetTabsByPortal(PortalId)
                             : TabController.Instance.GetTabsByPortal(PortalId).WithCulture(currentLocale.Code, true);

            DeletedTabs = tabsList.Values.Where(tab => tab.IsDeleted)
                                            .OrderBy(tab => tab.TabPath)
                                            .ToList();

            DeletedModules = ModuleController.Instance.GetModules(PortalId)
                                                .Cast<ModuleInfo>()
                                                .Where(module => module.IsDeleted && (modeButtonList.SelectedValue == "ALL" || module.CultureCode == currentLocale.Code))
                                                .ToList();
        }

        private void RestoreModule(int moduleId, int tabId)
        {
            // restore module
            var module = ModuleController.Instance.GetModule(moduleId, tabId, false);
            if ((module != null))
            {
                if (DeletedTabs.Any(t => t.TabID == module.TabID))
                {
                    var title = !string.IsNullOrEmpty(module.ModuleTitle) ? module.ModuleTitle : module.DesktopModule.FriendlyName;
                    Skin.AddModuleMessage(this, string.Format(Localization.GetString("TabDeleted.ErrorMessage", LocalResourceFile), title),
                                                   ModuleMessage.ModuleMessageType.RedError);
                    return;
                }
                ModuleController.Instance.RestoreModule(module);
                
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                var currentModuleVersion = TabVersionBuilder.Instance.GetModuleContentLatestVersion(module);
                TabChangeTracker.Instance.TrackModuleAddition(module, currentModuleVersion, currentUser.UserID);
                EventLogController.Instance.AddLog(module, PortalSettings, UserId, "", EventLogController.EventLogType.MODULE_RESTORED);
            }
        }

        private bool RestoreTab(TabInfo tab)
		{
			var success = true;

			if (tab != null)
			{
				if (!Null.IsNull(tab.ParentId) && DeletedTabs.Any(t => t.TabID == tab.ParentId))
				{
					Skin.AddModuleMessage(this,
												   string.Format(Localization.GetString("ChildTab.ErrorMessage", LocalResourceFile), tab.TabName),
												   ModuleMessage.ModuleMessageType.YellowWarning);
					success = false;
				}
				else
				{
                    var changeControlStateForTab = TabChangeSettings.Instance.GetChangeControlState(tab.PortalID, tab.TabID);
                    if (changeControlStateForTab.IsChangeControlEnabledForTab)
                    {
                        TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, false);
                        TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
                    }

                    TabController.Instance.RestoreTab(tab, PortalSettings);
					DeletedTabs.Remove(tab);

					//restore modules in this tab
					modulesListBox.Items.Cast<ListItem>().ToList().ForEach(i =>
					                                                   	{
																			var values = i.Value.Split('-');
																			var tabId = int.Parse(values[0]);
																			var moduleId = int.Parse(values[1]);
																			if(tabId == tab.TabID)
																			{
																				RestoreModule(moduleId, tabId);
																			}
					                                                   	});

                    if (changeControlStateForTab.IsChangeControlEnabledForTab)
                    {
                        TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, changeControlStateForTab.IsVersioningEnabledForTab);
                        TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, changeControlStateForTab.IsWorkflowEnabledForTab);
                    }
				}
			}
			return success;
		}

		#endregion

		#region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();

            cmdDeleteModule.Click += OnModuleDeleteClick;
            cmdDeleteTab.Click += OnTabDeleteClick;
            cmdEmpty.Click += OnEmptyBinClick;
            cmdRestoreModule.Click += OnModuleRestoreClick;
            cmdRestoreTab.Click += OnTabRestoreClick;
            modeButtonList.SelectedIndexChanged += OnModeIndexChanged;
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var resourceFileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + ID;

            LoadData();

			if ((Page.IsPostBack == false))
			{
				if (PortalSettings.ContentLocalizationEnabled)
				{
					ClientAPI.AddButtonConfirm(cmdRestoreTab, Localization.GetString("RestoreTab", resourceFileRoot));
				}
				divModuleButtons.Visible = ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "Edit");
                divTabButtons.Visible = ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "Edit");
                cmdEmpty.Visible = ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "Edit");
				divMode.Visible = PortalSettings.ContentLocalizationEnabled;

				var mode = "ALL";
				if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
				{
					mode = Request.QueryString["mode"];
				}
				modeButtonList.SelectedValue = mode.ToUpperInvariant() == "SINGLE" ? "SINGLE" : "ALL";

                BindData(false);
            }

        }

		protected void OnEmptyBinClick(Object sender, EventArgs e)
		{
            foreach(var module in DeletedModules)
            {
                DeleteModule(module);
            }

            //Delete tabs starting with the deepest children
            foreach(var tab in DeletedTabs.OrderByDescending(t => t.Level))
            {
                DeleteTab(tab, true);
            }

			BindData(true);
		}

        protected void OnModeIndexChanged(object sender, EventArgs e)
        {
            BindData(true);
        }

        protected void OnModuleDeleteClick(Object sender, EventArgs e)
        {
            foreach (ListItem item in modulesListBox.Items)
            {
                if (item.Selected)
                {
                    var values = item.Value.Split('-');
                    var module = DeletedModules.SingleOrDefault(m => m.ModuleID == int.Parse(values[1])
                                                                    && m.TabID == int.Parse(values[0]));
                    if (module != null)
                    {
                        DeleteModule(module);
                    }
                }
            }

            BindData(true);
        }

        protected void OnModuleRestoreClick(Object sender, EventArgs e)
		{
			foreach (ListItem item in modulesListBox.Items)
			{
				if (item.Selected)
				{
					var values = item.Value.Split('-');

                    // restore module
                    RestoreModule(int.Parse(values[1]), int.Parse(values[0]));
				}
			}

			BindData(true);
		}

        protected void OnTabDeleteClick(Object sender, EventArgs e)
        {
            foreach (ListItem item in tabsListBox.Items)
            {
                if (item.Selected)
                {
                    var tab = DeletedTabs.SingleOrDefault(t => t.TabID == int.Parse(item.Value));
                    if (tab != null)
                    {
                        if (tab.HasChildren)
                        {
                            Skin.AddModuleMessage(this, String.Format(Localization.GetString("ParentTab.ErrorMessage", LocalResourceFile), tab.TabName), ModuleMessage.ModuleMessageType.YellowWarning);
                        }
                        else
                        {
                            DeleteTab(tab, false);
                        }
                    }
                }
            }
            BindData(true);
        }

        protected void OnTabRestoreClick(Object sender, EventArgs e)
		{
			var errors = false;

			foreach (ListItem item in tabsListBox.Items)
			{
				if (item.Selected)
				{
                    var tab = DeletedTabs.SingleOrDefault(t => t.TabID == int.Parse(item.Value));

                    if (!RestoreTab(tab))
					{
						errors = true;
					}
				}
			}
			if (!errors)
			{
				Response.Redirect(Globals.NavigateURL(TabId, "", "mode=" + modeButtonList.SelectedValue.ToLowerInvariant()));
			}
			else
			{
				BindData(true);
			}
		}

		#endregion
	}
}