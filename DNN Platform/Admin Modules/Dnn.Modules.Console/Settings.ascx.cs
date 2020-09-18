// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.Console
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    using Dnn.Modules.Console.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Common;
    using DotNetNuke.Web.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls.Internal;

    public partial class Settings : ModuleSettingsBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Settings));

        public override void LoadSettings()
        {
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    if (this.Settings.ContainsKey("ParentTabId") && !string.IsNullOrEmpty(Convert.ToString(this.Settings["ParentTabId"])))
                    {
                        var tabId = Convert.ToInt32(this.Settings["ParentTabId"]);
                        this.ParentTab.SelectedPage = TabController.Instance.GetTab(tabId, this.PortalId);
                    }

                    foreach (string val in ConsoleController.GetSizeValues())
                    {
                        // DefaultSize.Items.Add(new ListItem(Localization.GetString(val, LocalResourceFile), val));
                        this.DefaultSize.AddItem(Localization.GetString(val, this.LocalResourceFile), val);
                    }

                    this.SelectDropDownListItem(ref this.DefaultSize, "DefaultSize");

                    this.SelectDropDownListItem(ref this.modeList, "Mode");

                    if (this.Settings.ContainsKey("AllowSizeChange"))
                    {
                        this.AllowResize.Checked = Convert.ToBoolean(this.Settings["AllowSizeChange"]);
                    }

                    foreach (var val in ConsoleController.GetViewValues())
                    {
                        // DefaultView.Items.Add(new ListItem(Localization.GetString(val, LocalResourceFile), val));
                        this.DefaultView.AddItem(Localization.GetString(val, this.LocalResourceFile), val);
                    }

                    this.SelectDropDownListItem(ref this.DefaultView, "DefaultView");
                    if (this.Settings.ContainsKey("IncludeParent"))
                    {
                        this.IncludeParent.Checked = Convert.ToBoolean(this.Settings["IncludeParent"]);
                    }

                    if (this.Settings.ContainsKey("AllowViewChange"))
                    {
                        this.AllowViewChange.Checked = Convert.ToBoolean(this.Settings["AllowViewChange"]);
                    }

                    if (this.Settings.ContainsKey("ShowTooltip"))
                    {
                        this.ShowTooltip.Checked = Convert.ToBoolean(this.Settings["ShowTooltip"]);
                    }

                    if (this.Settings.ContainsKey("OrderTabsByHierarchy"))
                    {
                        this.OrderTabsByHierarchy.Checked = Convert.ToBoolean(this.Settings["OrderTabsByHierarchy"]);
                    }

                    if (this.Settings.ContainsKey("IncludeHiddenPages"))
                    {
                        this.IncludeHiddenPages.Checked = Convert.ToBoolean(this.Settings["IncludeHiddenPages"]);
                    }

                    if (this.Settings.ContainsKey("ConsoleWidth"))
                    {
                        this.ConsoleWidth.Text = Convert.ToString(this.Settings["ConsoleWidth"]);
                    }

                    this.SwitchMode();
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
                // validate console width value
                var wdth = string.Empty;
                if (this.ConsoleWidth.Text.Trim().Length > 0)
                {
                    try
                    {
                        wdth = Unit.Parse(this.ConsoleWidth.Text.Trim()).ToString();
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);

                        throw new Exception("ConsoleWidth value is invalid. Value must be numeric.");
                    }
                }

                if (this.ParentTab.SelectedItemValueAsInt == Null.NullInteger)
                {
                    ModuleController.Instance.DeleteModuleSetting(this.ModuleId, "ParentTabID");
                }
                else
                {
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ParentTabID", this.ParentTab.SelectedItem.Value);
                }

                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "Mode", this.modeList.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "DefaultSize", this.DefaultSize.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "AllowSizeChange", this.AllowResize.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "DefaultView", this.DefaultView.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "AllowViewChange", this.AllowViewChange.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowTooltip", this.ShowTooltip.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "OrderTabsByHierarchy", this.OrderTabsByHierarchy.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeHiddenPages", this.IncludeHiddenPages.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeParent", this.IncludeParent.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ConsoleWidth", wdth);

                foreach (RepeaterItem item in this.tabs.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var tabPath = (item.FindControl("tabPath") as HiddenField).Value;
                        var visibility = (item.FindControl("tabVisibility") as DnnComboBox).SelectedValue;

                        var key = string.Format("TabVisibility{0}", tabPath.Replace("//", "-"));
                        ModuleController.Instance.UpdateModuleSetting(this.ModuleId, key, visibility);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.tabs.ItemDataBound += this.tabs_ItemDataBound;
            this.modeList.SelectedIndexChanged += this.modeList_SelectedIndexChanged;

            this.ParentTab.UndefinedItem = new ListItem(DynamicSharedConstants.Unspecified, string.Empty);
        }

        protected void parentTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindTabs(this.ParentTab.SelectedItemValueAsInt, this.IncludeParent.Checked);
        }

        private void BindTabs(int tabId, bool includeParent)
        {
            List<TabInfo> tempTabs = TabController.GetTabsBySortOrder(this.PortalId).OrderBy(t => t.Level).ThenBy(t => t.HasChildren).ToList();

            IList<TabInfo> tabList = new List<TabInfo>();

            IList<int> tabIdList = new List<int>();
            tabIdList.Add(tabId);

            if (includeParent)
            {
                TabInfo consoleTab = TabController.Instance.GetTab(tabId, this.PortalId);
                if (consoleTab != null)
                {
                    tabList.Add(consoleTab);
                }
            }

            foreach (TabInfo tab in tempTabs)
            {
                bool canShowTab = TabPermissionController.CanViewPage(tab) &&
                                  !tab.IsDeleted &&
                                  (tab.StartDate < DateTime.Now || tab.StartDate == Null.NullDate);

                if (!canShowTab)
                {
                    continue;
                }

                if (tabIdList.Contains(tab.ParentId))
                {
                    if (!tabIdList.Contains(tab.TabID))
                    {
                        tabIdList.Add(tab.TabID);
                    }

                    tabList.Add(tab);
                }
            }

            this.tabs.DataSource = tabList;
            this.tabs.DataBind();
        }

        private void SwitchMode()
        {
            int parentTabId = -1;
            if (this.Settings.ContainsKey("ParentTabID") && !string.IsNullOrEmpty(Convert.ToString(this.Settings["ParentTabID"])))
            {
                parentTabId = Convert.ToInt32(this.Settings["ParentTabID"]);
            }

            switch (this.modeList.SelectedValue)
            {
                case "Normal":
                    this.parentTabRow.Visible = true;
                    this.includeParentRow.Visible = true;
                    this.tabVisibilityRow.Visible = false;
                    break;
                case "Profile":
                    this.parentTabRow.Visible = false;
                    this.includeParentRow.Visible = false;
                    this.tabVisibilityRow.Visible = true;
                    parentTabId = this.PortalSettings.UserTabId;
                    break;
                case "Group":
                    this.parentTabRow.Visible = true;
                    this.includeParentRow.Visible = true;
                    this.tabVisibilityRow.Visible = true;
                    break;
            }

            this.ParentTab.SelectedPage = TabController.Instance.GetTab(parentTabId, this.PortalId);
            this.BindTabs(parentTabId, this.IncludeParent.Checked);
        }

        private void modeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SwitchMode();
        }

        private void tabs_ItemDataBound(object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var tab = (TabInfo)e.Item.DataItem;
                DnnComboBox visibilityDropDown = (DnnComboBox)e.Item.FindControl("tabVisibility");

                var tabLabel = (Label)e.Item.FindControl("tabLabel");
                var tabPathField = (HiddenField)e.Item.FindControl("tabPath");

                visibilityDropDown.Items.Clear();

                // visibilityDropDown.Items.Add(new ListItem(LocalizeString("AllUsers"), "AllUsers"));
                visibilityDropDown.AddItem(this.LocalizeString("AllUsers"), "AllUsers");
                if (this.modeList.SelectedValue == "Profile")
                {
                    // visibilityDropDown.Items.Add(new ListItem(LocalizeString("Friends"), "Friends"));
                    // visibilityDropDown.Items.Add(new ListItem(LocalizeString("User"), "User"));
                    visibilityDropDown.AddItem(this.LocalizeString("Friends"), "Friends");
                    visibilityDropDown.AddItem(this.LocalizeString("User"), "User");
                }
                else
                {
                    // visibilityDropDown.Items.Add(new ListItem(LocalizeString("Owner"), "Owner"));
                    // visibilityDropDown.Items.Add(new ListItem(LocalizeString("Members"), "Members"));
                    visibilityDropDown.AddItem(this.LocalizeString("Owner"), "Owner");
                    visibilityDropDown.AddItem(this.LocalizeString("Members"), "Members");
                }

                tabLabel.Text = tab.TabName;
                tabPathField.Value = tab.TabPath;

                var key = string.Format("TabVisibility{0}", tab.TabPath.Replace("//", "-"));
                this.SelectDropDownListItem(ref visibilityDropDown, key);
            }
        }

        private void SelectDropDownListItem(ref DnnComboBox ddl, string key)
        {
            if (this.Settings.ContainsKey(key))
            {
                ddl.ClearSelection();
                var selItem = ddl.FindItemByValue(Convert.ToString(this.Settings[key]));
                if (selItem != null)
                {
                    selItem.Selected = true;
                }
            }
        }
    }
}
