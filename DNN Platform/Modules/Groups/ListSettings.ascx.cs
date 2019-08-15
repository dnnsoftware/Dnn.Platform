#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.Groups.Components;
using DotNetNuke.Entities.Tabs;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules.Definitions;


#endregion

namespace DotNetNuke.Modules.Groups
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ListSettings : GroupsSettingsBase
    {
        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    BindGroups();
                    BindPages();
                    
                    if (Settings.ContainsKey(Constants.DefaultRoleGroupSetting)) {
                        drpRoleGroup.SelectedIndex = drpRoleGroup.Items.IndexOf(drpRoleGroup.Items.FindByValue(Settings[Constants.DefaultRoleGroupSetting].ToString()));
                    }

                    if (Settings.ContainsKey(Constants.GroupViewPage)) {
                        drpGroupViewPage.SelectedIndex = drpGroupViewPage.Items.IndexOf(drpGroupViewPage.Items.FindByValue(Settings[Constants.GroupViewPage].ToString()));
                    }

                    if (Settings.ContainsKey(Constants.GroupListTemplate)) {
                        txtListTemplate.Text = Settings[Constants.GroupListTemplate].ToString();
                    }

                    if (Settings.ContainsKey(Constants.GroupViewTemplate))
                    {
                        txtViewTemplate.Text = Settings[Constants.GroupViewTemplate].ToString();
                    }

                    if (Settings.ContainsKey(Constants.GroupModerationEnabled)) 
                    {
                        chkGroupModeration.Checked = Convert.ToBoolean(Settings[Constants.GroupModerationEnabled].ToString());
                    }

                    if (Settings.ContainsKey(Constants.GroupLoadView)) {
                        drpViewMode.SelectedIndex = drpViewMode.Items.IndexOf(drpViewMode.Items.FindByValue(Settings[Constants.GroupLoadView].ToString()));
                    }

                    if (Settings.ContainsKey(Constants.GroupListPageSize))
                    {
                        txtPageSize.Text = Settings[Constants.GroupListPageSize].ToString();
                    }

                    if (Settings.ContainsKey(Constants.GroupListUserGroupsOnly))
                    {
                        chkUserGroups.Checked = Convert.ToBoolean(Settings[Constants.GroupListUserGroupsOnly].ToString());
                    }

                    if (Settings.ContainsKey(Constants.GroupListSearchEnabled))
                    {
                        chkEnableSearch.Checked = Convert.ToBoolean(Settings[Constants.GroupListSearchEnabled].ToString());
                    }

                    if (Settings.ContainsKey(Constants.GroupListSortField))
                    {
                        lstSortField.SelectedIndex = lstSortField.Items.IndexOf(lstSortField.Items.FindByValue(Settings[Constants.GroupListSortField].ToString()));
                    }

                    if (Settings.ContainsKey(Constants.GroupListSortDirection))
                    {
                        radSortDirection.SelectedIndex = radSortDirection.Items.IndexOf(radSortDirection.Items.FindByValue(Settings[Constants.GroupListSortDirection].ToString()));
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
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.DefaultRoleGroupSetting, drpRoleGroup.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewPage, drpGroupViewPage.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListTemplate, txtListTemplate.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewTemplate, txtViewTemplate.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupModerationEnabled, chkGroupModeration.Checked.ToString());
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupLoadView, drpViewMode.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListPageSize, txtPageSize.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListSearchEnabled, chkEnableSearch.Checked.ToString());
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListSortField, lstSortField.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListSortDirection, radSortDirection.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListUserGroupsOnly, chkUserGroups.Checked.ToString());
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
        private void BindGroups() {
            var arrGroups = RoleController.GetRoleGroups(PortalId);
			drpRoleGroup.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
            drpRoleGroup.Items.Add(new ListItem(Localization.GetString("GlobalRoles"), "-1"));

            foreach (RoleGroupInfo roleGroup in arrGroups) {
                drpRoleGroup.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString()));
            }
        }
        private void BindPages() {
            foreach (ModuleInfo moduleInfo in ModuleController.Instance.GetModules(PortalId)) 
            {
                if (moduleInfo.DesktopModule.ModuleName.Contains("Social Groups") && moduleInfo.IsDeleted == false)
                {
                    TabInfo tabInfo = TabController.Instance.GetTab(moduleInfo.TabID, PortalId, false);
                    if (tabInfo != null) 
                    {
                        if (tabInfo.IsDeleted == false) 
                        {
                            foreach (KeyValuePair<string, ModuleDefinitionInfo> def in moduleInfo.DesktopModule.ModuleDefinitions) 
                            {
                                if (moduleInfo.ModuleDefinition.FriendlyName == def.Key) 
                                {
                                    if (drpGroupViewPage.Items.FindByValue(tabInfo.TabID.ToString()) == null) 
                                    {
                                        drpGroupViewPage.Items.Add(new ListItem(tabInfo.TabName + " - " + def.Key, tabInfo.TabID.ToString()));
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}