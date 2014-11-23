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
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.Console.Components;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Common;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DesktopModules.Admin.Console
{

    public partial class Settings : ModuleSettingsBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Settings));

        private void BindTabs(int tabId, bool includeParent)
        {
            List<TabInfo> tempTabs = TabController.GetTabsBySortOrder(PortalId).OrderBy(t => t.Level).ThenBy(t => t.HasChildren).ToList();

            IList<TabInfo> tabList = new List<TabInfo>();

            IList<int> tabIdList = new List<int>();
            tabIdList.Add(tabId);

            if (includeParent)
            {
                TabInfo consoleTab = TabController.Instance.GetTab(tabId, PortalId);
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
                if ((tabIdList.Contains(tab.ParentId)))
                {
                    if ((!tabIdList.Contains(tab.TabID)))
                    {
                        tabIdList.Add(tab.TabID);
                    }
                    tabList.Add(tab);
                }
            }

            tabs.DataSource = tabList;
            tabs.DataBind();
        }

        private void SwitchMode()
        {
            int parentTabId = -1;
            if (Settings.ContainsKey("ParentTabID") && !string.IsNullOrEmpty(Convert.ToString(Settings["ParentTabID"])))
            {
                parentTabId = Convert.ToInt32(Settings["ParentTabID"]);
            }
            switch (modeList.SelectedValue)
            {
                case "Normal":
                    parentTabRow.Visible = true;
                    includeParentRow.Visible = true;
                    tabVisibilityRow.Visible = false;
                    break;
                case "Profile":
                    parentTabRow.Visible = false;
                    includeParentRow.Visible = false;
                    tabVisibilityRow.Visible = true;
                    parentTabId = PortalSettings.UserTabId;
                    break;
                case "Group":
                    parentTabRow.Visible = true;
                    includeParentRow.Visible = true;
                    tabVisibilityRow.Visible = true;
                   break;
            }

            ParentTab.SelectedPage = TabController.Instance.GetTab(parentTabId, PortalId);
            BindTabs(parentTabId, IncludeParent.Checked);
        }

        public override void LoadSettings()
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    if (Settings.ContainsKey("ParentTabId") && !string.IsNullOrEmpty(Convert.ToString(Settings["ParentTabId"])))
                    {
                        var tabId = Convert.ToInt32(Settings["ParentTabId"]);
                        ParentTab.SelectedPage = TabController.Instance.GetTab(tabId, PortalId);
                    }

                    foreach (string val in ConsoleController.GetSizeValues())
                    {
                        //DefaultSize.Items.Add(new ListItem(Localization.GetString(val, LocalResourceFile), val));
                        DefaultSize.AddItem(Localization.GetString(val, LocalResourceFile), val);
                    }
                    SelectDropDownListItem(ref DefaultSize, "DefaultSize");

                    SelectDropDownListItem(ref modeList, "Mode");

                    if (Settings.ContainsKey("AllowSizeChange"))
                    {
                        AllowResize.Checked = Convert.ToBoolean(Settings["AllowSizeChange"]);
                    }
                    foreach (var val in ConsoleController.GetViewValues())
                    {
                        //DefaultView.Items.Add(new ListItem(Localization.GetString(val, LocalResourceFile), val));
                        DefaultView.AddItem(Localization.GetString(val, LocalResourceFile), val);
                    }
                    SelectDropDownListItem(ref DefaultView, "DefaultView");
                    if (Settings.ContainsKey("IncludeParent"))
                    {
                        IncludeParent.Checked = Convert.ToBoolean(Settings["IncludeParent"]);
                    }
                    if (Settings.ContainsKey("AllowViewChange"))
                    {
                        AllowViewChange.Checked = Convert.ToBoolean(Settings["AllowViewChange"]);
                    }
                    if (Settings.ContainsKey("ShowTooltip"))
                    {
                        ShowTooltip.Checked = Convert.ToBoolean(Settings["ShowTooltip"]);
                    }
					if (Settings.ContainsKey("OrderTabsByHierarchy"))
					{
						OrderTabsByHierarchy.Checked = Convert.ToBoolean(Settings["OrderTabsByHierarchy"]);
					}
                    if (Settings.ContainsKey("IncludeHiddenPages"))
                    {
                        IncludeHiddenPages.Checked = Convert.ToBoolean(Settings["IncludeHiddenPages"]);
                    }
                    if (Settings.ContainsKey("ConsoleWidth"))
                    {
                        ConsoleWidth.Text = Convert.ToString(Settings["ConsoleWidth"]);
                    }

                    SwitchMode();
                }

            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
				//validate console width value
                var wdth = string.Empty;
                if ((ConsoleWidth.Text.Trim().Length > 0))
                {
                    try
                    {
                        wdth = Unit.Parse(ConsoleWidth.Text.Trim()).ToString();
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);

                        throw new Exception("ConsoleWidth value is invalid. Value must be numeric.");
                    }
                }
                if (ParentTab.SelectedItemValueAsInt == Null.NullInteger)
                {
                    ModuleController.Instance.DeleteModuleSetting(ModuleId, "ParentTabID");
                }
                else
                {
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ParentTabID", ParentTab.SelectedItem.Value);
                }

                ModuleController.Instance.UpdateModuleSetting(ModuleId, "Mode", modeList.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "DefaultSize", DefaultSize.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "AllowSizeChange", AllowResize.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "DefaultView", DefaultView.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "AllowViewChange", AllowViewChange.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowTooltip", ShowTooltip.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "OrderTabsByHierarchy", OrderTabsByHierarchy.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "IncludeHiddenPages", IncludeHiddenPages.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "IncludeParent", IncludeParent.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "ConsoleWidth", wdth);

                foreach (RepeaterItem item in tabs.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
	                    var tabPath = (item.FindControl("tabPath") as HiddenField).Value;
						var visibility = (item.FindControl("tabVisibility") as DnnComboBox).SelectedValue;

                        var key = String.Format("TabVisibility{0}", tabPath.Replace("//","-"));
                        ModuleController.Instance.UpdateModuleSetting(ModuleId, key, visibility);
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

            tabs.ItemDataBound +=  tabs_ItemDataBound;
            modeList.SelectedIndexChanged += modeList_SelectedIndexChanged;

            ParentTab.UndefinedItem = new ListItem(SharedConstants.Unspecified, string.Empty);
        }

        private void modeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchMode();

        }

        protected void parentTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindTabs(ParentTab.SelectedItemValueAsInt, IncludeParent.Checked);
        }

        void tabs_ItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var tab = (TabInfo) e.Item.DataItem;
                DnnComboBox visibilityDropDown = (DnnComboBox)e.Item.FindControl("tabVisibility");

                var tabLabel = (Label) e.Item.FindControl("tabLabel");
                var tabPathField = (HiddenField) e.Item.FindControl("tabPath");

                visibilityDropDown.Items.Clear();
                //visibilityDropDown.Items.Add(new ListItem(LocalizeString("AllUsers"), "AllUsers"));
                visibilityDropDown.AddItem(LocalizeString("AllUsers"), "AllUsers");
                if (modeList.SelectedValue == "Profile")
                {
                    //visibilityDropDown.Items.Add(new ListItem(LocalizeString("Friends"), "Friends"));
                    //visibilityDropDown.Items.Add(new ListItem(LocalizeString("User"), "User"));

                    visibilityDropDown.AddItem(LocalizeString("Friends"), "Friends");
                    visibilityDropDown.AddItem(LocalizeString("User"), "User");
                }
                else
                {
                    //visibilityDropDown.Items.Add(new ListItem(LocalizeString("Owner"), "Owner"));
                    //visibilityDropDown.Items.Add(new ListItem(LocalizeString("Members"), "Members"));

                    visibilityDropDown.AddItem(LocalizeString("Owner"), "Owner");
                    visibilityDropDown.AddItem(LocalizeString("Members"), "Members");
                }

                tabLabel.Text = tab.TabName;
                tabPathField.Value = tab.TabPath;

                var key = String.Format("TabVisibility{0}", tab.TabPath.Replace("//", "-"));
                SelectDropDownListItem(ref visibilityDropDown, key);
            }
        }    

        private void SelectDropDownListItem(ref DnnComboBox ddl, string key)
        {
            if (Settings.ContainsKey(key))
            {
                ddl.ClearSelection();
                var selItem = ddl.FindItemByValue(Convert.ToString(Settings[key]));
                if (selItem != null)
                {
                    selItem.Selected = true;
                }
            }
        }

    }
}