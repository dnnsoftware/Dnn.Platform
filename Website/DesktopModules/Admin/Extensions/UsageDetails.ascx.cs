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
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
	/// <summary>
	/// Add and Edit Servers for a Web Farm
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// </history>
    public partial class UsageDetails : PortalModuleBase
    {
        private bool _IsListBound;
        private PackageInfo _Package;
        private int _PackageID = Null.NullInteger;
        private IDictionary<int, PortalInfo> _Portals;

        protected int PackageID
        {
            get
            {
                if ((_PackageID == Null.NullInteger && Request.QueryString["PackageID"] != null))
                {
                    _PackageID = Int32.Parse(Request.QueryString["PackageID"]);
                }
                return _PackageID;
            }
        }

        protected PackageInfo Package
        {
            get
            {
                if (_Package == null)
                {
                    if (PackageID == Null.NullInteger)
                    {
                        _Package = new PackageInfo();
                    }
                    else
                    {
                        _Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID);
                    }
                }
                return _Package;
            }
        }

        protected IDictionary<int, PortalInfo> Portals
        {
            get
            {
                if (_Portals == null)
                {
                    _Portals = new Dictionary<int, PortalInfo>();
                    var items = PortalController.Instance.GetPortals();
                    foreach (PortalInfo item in items)
                    {
                        _Portals.Add(item.PortalID, item);
                    }
                }
                return _Portals;
            }
        }

        protected bool IsSuperTab
        {
            get
            {
                return (ModuleContext.PortalSettings.ActiveTab.IsSuperTab);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                lblTitle.Text = Localization.GetString("Usage", LocalResourceFile) + Package.FriendlyName;
                UsageList.PagerSettings.FirstPageText = Localization.GetString("grd.PagerSettings.FirstPageText", LocalResourceFile);
                UsageList.PagerSettings.LastPageText = Localization.GetString("grd.PagerSettings.LastPageText", LocalResourceFile);
                UsageList.PagerSettings.NextPageText = Localization.GetString("grd.PagerSettings.NextPageText", LocalResourceFile);
                UsageList.PagerSettings.PreviousPageText = Localization.GetString("grd.PagerSettings.PreviousPageText", LocalResourceFile);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                BindFilterList();
                if ((FilterUsageList.Visible))
                {
                    BindUsageList(int.Parse(FilterUsageList.SelectedValue), FilterUsageList.SelectedItem.Text);
                }
                else
                {
                    BindUsageList(PortalId, PortalController.Instance.GetCurrentPortalSettings().PortalName);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void FilterUsageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((FilterUsageList.SelectedValue != null))
                {
                    UsageList.PageIndex = 0;
                    BindUsageList(int.Parse(FilterUsageList.SelectedValue), FilterUsageList.SelectedItem.Text);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void UsageList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                UsageList.PageIndex = e.NewPageIndex;
                BindUsageList(int.Parse(FilterUsageList.SelectedValue), FilterUsageList.SelectedItem.Text);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected string GetFormattedLink(object dataItem)
        {
            var returnValue = new StringBuilder();
            if ((dataItem is TabInfo))
            {
                var tab = (TabInfo) dataItem;
                if ((tab != null))
                {
                    int index = 0;
                    TabController.Instance.PopulateBreadCrumbs(ref tab);
                    foreach (TabInfo t in tab.BreadCrumbs)
                    {
                        if ((index > 0))
                        {
                            returnValue.Append(" > ");
                        }
                        if ((tab.BreadCrumbs.Count - 1 == index))
                        {
                            string url;
                            var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(t.PortalID).ToList();
                            var alias = aliases[0];
                            url = Globals.AddHTTP(alias.HTTPAlias) + "/Default.aspx?tabId=" + t.TabID;
                            returnValue.AppendFormat("<a href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                        }
                        else
                        {
                            returnValue.AppendFormat("{0}", t.LocalizedTabName);
                        }
                        index = index + 1;
                    }
                }
            }
            return returnValue.ToString();
        }

        private void BindFilterList()
        {
            if ((PackageID != Null.NullInteger && Package != null && Package.PackageType.ToUpper() == "MODULE"))
            {
                tblFilterUsage.Visible = IsSuperTab;
                if (!IsPostBack)
                {
                    if ((FilterUsageList.Visible))
                    {
                        FilterUsageList.DataSource = Portals.Values;
                        FilterUsageList.DataTextField = "PortalName";
                        FilterUsageList.DataValueField = "PortalID";
                        FilterUsageList.DataBind();
                        //FilterUsageList.Items.Insert(0, new ListItem(Localization.GetString("FilterOptionHost", LocalResourceFile), Null.NullInteger.ToString()));
                        //FilterUsageList.Items.Insert(0, new ListItem(Localization.GetString("FilterOptionSelect", LocalResourceFile), "-2"));

                        FilterUsageList.InsertItem(0, Localization.GetString("FilterOptionHost", LocalResourceFile), Null.NullInteger.ToString());
                        FilterUsageList.InsertItem(0, Localization.GetString("FilterOptionSelect", LocalResourceFile), "-2");
                        FilterUsageList.Items[0].Selected = true;
                    }
                }
            }
        }

        private void BindUsageList(int selectedPortalID, string selectedPortalName)
        {
            if ((_IsListBound))
            {
                return;
            }
            _IsListBound = true;
            IDictionary<int, TabInfo> tabs = null;
            string portalName = string.Empty;

            if (PackageID != Null.NullInteger && Package != null)
            {
                if (IsSuperTab)
                {
                    if (selectedPortalID == -2)
                    {
                        portalName = string.Empty;
                    }
                    else
                    {
                        tabs = BuildData(selectedPortalID);
                        portalName = selectedPortalName;
                    }
                }
                else
                {
                    tabs = BuildData(PortalId);
                    portalName = string.Empty;
                }
            }
            if ((tabs != null && tabs.Count > 0))
            {
                UsageList.Visible = true;
                UsageList.DataSource = tabs.Values;
                UsageList.DataBind();

                UsageListMsg.Text = string.Format(Localization.GetString("Msg.InUseBy", LocalResourceFile), tabs.Count, portalName);
            }
            else if ((portalName != string.Empty))
            {
                UsageList.Visible = false;
                UsageListMsg.Text = string.Format(Localization.GetString("Msg.NotUsedBy", LocalResourceFile), portalName);
            }
            else
            {
                UsageList.Visible = false;
                UsageListMsg.Text = string.Empty;
            }
        }

        private IDictionary<int, TabInfo> BuildData(int portalID)
        {
            IDictionary<int, TabInfo> tabsWithModule = TabController.Instance.GetTabsByPackageID(portalID, PackageID, false);
            TabCollection allPortalTabs = TabController.Instance.GetTabsByPortal(portalID);
            IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();

			//must get each tab, they parent may not exist
            foreach (TabInfo tab in allPortalTabs.Values)
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }
            return tabsInOrder;
        }

        private void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
            if ((tabsWithModule.ContainsKey(currentTab.TabID) && !tabsInOrder.ContainsKey(currentTab.TabID)))
            {
				//add current tab
                tabsInOrder.Add(currentTab.TabID, currentTab);
				//add children of current tab
                foreach (TabInfo tab in allPortalTabs.WithParentId(currentTab.TabID))
                {
                    AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
                }
            }
        }
    }
}