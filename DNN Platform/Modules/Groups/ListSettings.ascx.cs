// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ListSettings : GroupsSettingsBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    this.BindGroups();
                    this.BindPages();

                    if (this.Settings.ContainsKey(Constants.DefaultRoleGroupSetting))
                    {
                        this.drpRoleGroup.SelectedIndex = this.drpRoleGroup.Items.IndexOf(this.drpRoleGroup.Items.FindByValue(this.Settings[Constants.DefaultRoleGroupSetting].ToString()));
                    }

                    if (this.Settings.ContainsKey(Constants.GroupViewPage))
                    {
                        this.drpGroupViewPage.SelectedIndex = this.drpGroupViewPage.Items.IndexOf(this.drpGroupViewPage.Items.FindByValue(this.Settings[Constants.GroupViewPage].ToString()));
                    }

                    if (this.Settings.ContainsKey(Constants.GroupListTemplate))
                    {
                        this.txtListTemplate.Text = this.Settings[Constants.GroupListTemplate].ToString();
                    }

                    if (this.Settings.ContainsKey(Constants.GroupViewTemplate))
                    {
                        this.txtViewTemplate.Text = this.Settings[Constants.GroupViewTemplate].ToString();
                    }

                    if (this.Settings.ContainsKey(Constants.GroupModerationEnabled))
                    {
                        this.chkGroupModeration.Checked = Convert.ToBoolean(this.Settings[Constants.GroupModerationEnabled].ToString());
                    }

                    if (this.Settings.ContainsKey(Constants.GroupLoadView))
                    {
                        this.drpViewMode.SelectedIndex = this.drpViewMode.Items.IndexOf(this.drpViewMode.Items.FindByValue(this.Settings[Constants.GroupLoadView].ToString()));
                    }

                    if (this.Settings.ContainsKey(Constants.GroupListPageSize))
                    {
                        this.txtPageSize.Text = this.Settings[Constants.GroupListPageSize].ToString();
                    }

                    if (this.Settings.ContainsKey(Constants.GroupListUserGroupsOnly))
                    {
                        this.chkUserGroups.Checked = Convert.ToBoolean(this.Settings[Constants.GroupListUserGroupsOnly].ToString());
                    }

                    if (this.Settings.ContainsKey(Constants.GroupListSearchEnabled))
                    {
                        this.chkEnableSearch.Checked = Convert.ToBoolean(this.Settings[Constants.GroupListSearchEnabled].ToString());
                    }

                    if (this.Settings.ContainsKey(Constants.GroupListSortField))
                    {
                        this.lstSortField.SelectedIndex = this.lstSortField.Items.IndexOf(this.lstSortField.Items.FindByValue(this.Settings[Constants.GroupListSortField].ToString()));
                    }

                    if (this.Settings.ContainsKey(Constants.GroupListSortDirection))
                    {
                        this.radSortDirection.SelectedIndex = this.radSortDirection.Items.IndexOf(this.radSortDirection.Items.FindByValue(this.Settings[Constants.GroupListSortDirection].ToString()));
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.DefaultRoleGroupSetting, this.drpRoleGroup.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewPage, this.drpGroupViewPage.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListTemplate, this.txtListTemplate.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewTemplate, this.txtViewTemplate.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupModerationEnabled, this.chkGroupModeration.Checked.ToString());
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupLoadView, this.drpViewMode.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListPageSize, this.txtPageSize.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListSearchEnabled, this.chkEnableSearch.Checked.ToString());
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListSortField, this.lstSortField.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListSortDirection, this.radSortDirection.SelectedItem.Value);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupListUserGroupsOnly, this.chkUserGroups.Checked.ToString());
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindGroups()
        {
            var arrGroups = RoleController.GetRoleGroups(this.PortalId);
            this.drpRoleGroup.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
            this.drpRoleGroup.Items.Add(new ListItem(Localization.GetString("GlobalRoles"), "-1"));

            foreach (RoleGroupInfo roleGroup in arrGroups)
            {
                this.drpRoleGroup.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString()));
            }
        }

        private void BindPages()
        {
            foreach (ModuleInfo moduleInfo in ModuleController.Instance.GetModules(this.PortalId))
            {
                if (moduleInfo.DesktopModule.ModuleName.Contains("Social Groups") && moduleInfo.IsDeleted == false)
                {
                    TabInfo tabInfo = TabController.Instance.GetTab(moduleInfo.TabID, this.PortalId, false);
                    if (tabInfo != null)
                    {
                        if (tabInfo.IsDeleted == false)
                        {
                            foreach (KeyValuePair<string, ModuleDefinitionInfo> def in moduleInfo.DesktopModule.ModuleDefinitions)
                            {
                                if (moduleInfo.ModuleDefinition.FriendlyName == def.Key)
                                {
                                    if (this.drpGroupViewPage.Items.FindByValue(tabInfo.TabID.ToString()) == null)
                                    {
                                        this.drpGroupViewPage.Items.Add(new ListItem(tabInfo.TabName + " - " + def.Key, tabInfo.TabID.ToString()));
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
