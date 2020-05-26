// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Modules.SearchResults
{
    public partial class ResultsSettings : ModuleSettingsBase
    {
		#region "Base Method Implementations"

        public override void LoadSettings()
        {
            try
            {
                if ((Page.IsPostBack == false))
                {
                    if (!String.IsNullOrEmpty(Convert.ToString(Settings["LinkTarget"])))
                    {
                        comboBoxLinkTarget.SelectedValue = Convert.ToString(Settings["LinkTarget"]);
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(Settings["ScopeForPortals"])))
                    {
                        var list = Convert.ToString(Settings["ScopeForPortals"]).Split('|').ToList();
                        var portalList = LoadPortalsList().ToList();
                        if (portalList.Any())
                        {
                            foreach (var portal in portalList)
                            {
                                var item = new ListItem(portal[0], portal[1]) {Selected = list.Contains(portal[1])};
                                comboBoxPortals.Items.Add(item);
                            }
                        }
                        else
                        {
                            divPortalGroup.Visible = false;
                        }
                    }
                    else
                    {
                        var portalList = LoadPortalsList().ToList();
                        if (portalList.Any())
                        {
                            foreach (var portal in portalList)
                            {
                                var item = new ListItem(portal[0], portal[1]) { Selected = PortalId.ToString() == portal[1] };
                                comboBoxPortals.Items.Add(item);
                            }
                        }
                        else
                        {
                            divPortalGroup.Visible = false;
                        }
                    }

                    
                    if (!string.IsNullOrEmpty(Convert.ToString(Settings["ScopeForFilters"])))
                    {
                        var list = Convert.ToString(Settings["ScopeForFilters"]).Split('|').ToList();
                        var filterList = LoadSeachContentSourcesList();
                        foreach (var filter in filterList)
                        {
                            var item = new ListItem(filter, filter) {Selected = list.Contains(filter)};
                            comboBoxFilters.Items.Add(item);
                        }
                    }
                    else
                    {
                        var filterList = LoadSeachContentSourcesList();
                        foreach (var filter in filterList)
                        {
                            var item = new ListItem(filter, filter) {Selected = true};
                            comboBoxFilters.Items.Add(item);
                        }
                    }

                    var scopeForRoles = 
                        PortalController.GetPortalSetting("SearchResult_ScopeForRoles", PortalId, string.Empty)
                        .Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
                    var roles = RoleController.Instance.GetRoles(PortalId, r => !r.IsSystemRole || r.RoleName == "Registered Users");
                    roles.Insert(0, new RoleInfo(){RoleName = "Superusers" });

                    foreach (var role in roles)
                    {
                        var item = new ListItem(role.RoleName, role.RoleName) { Selected = scopeForRoles.Length == 0 || scopeForRoles.Contains(role.RoleName) };
                        comboBoxRoles.Items.Add(item);
                    }

                    chkEnableWildSearch.Checked = GetBooleanSetting("EnableWildSearch", true);
                    chkShowDescription.Checked = GetBooleanSetting("ShowDescription", true);
                    chkShowFriendlyTitle.Checked = GetBooleanSetting("ShowFriendlyTitle", true);
                    chkShowSnippet.Checked = GetBooleanSetting("ShowSnippet", true);
                    chkShowLastUpdated.Checked = GetBooleanSetting("ShowLastUpdated", true);
                    chkShowSource.Checked = GetBooleanSetting("ShowSource", true);
                    chkShowTags.Checked = GetBooleanSetting("ShowTags", true);

                    txtMaxDescriptionLength.Text = GetStringSetting("MaxDescriptionLength", "100");
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
                if (Page.IsValid)
                {
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "LinkTarget", comboBoxLinkTarget.SelectedValue);

                    var selectedPortals = comboBoxPortals.Value.Replace(",", "|");

                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ScopeForPortals", selectedPortals);

                    var selectedFilters = comboBoxFilters.Value.Replace(",", "|");

                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ScopeForFilters", selectedFilters.ToString());

                    var selectedRoles = comboBoxRoles.Value;
                    PortalController.UpdatePortalSetting(PortalId, "SearchResult_ScopeForRoles", selectedRoles);

                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "EnableWildSearch", chkEnableWildSearch.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowDescription", chkShowDescription.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowFriendlyTitle", chkShowFriendlyTitle.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowSnippet", chkShowSnippet.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowLastUpdated", chkShowLastUpdated.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowSource", chkShowSource.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "ShowTags", chkShowTags.Checked.ToString());

                    var maxDescriptionLength = txtMaxDescriptionLength.Text;
                    if (string.IsNullOrEmpty(maxDescriptionLength) || !Regex.IsMatch(maxDescriptionLength, "^\\d+$"))
                    {
                        maxDescriptionLength = "100";
                    }
                    ModuleController.Instance.UpdateModuleSetting(ModuleId, "MaxDescriptionLength", maxDescriptionLength);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

			DataCache.RemoveCache(string.Format("ModuleInfos{0}", PortalSettings.PortalId));
        }
		
		#endregion

        protected IEnumerable<string[]> LoadPortalsList()
        {
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();
            var mygroup = (from @group in groups
                           select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId)
                               into portals
                               where portals.Any(x => x.PortalID == PortalSettings.Current.PortalId)
                               select portals.ToArray()).FirstOrDefault();

            var result = new List<string[]>();
            if (mygroup != null && mygroup.Any())
            {
                result.AddRange(mygroup.Select(
                    pi => new[] {pi.PortalName, pi.PortalID.ToString(CultureInfo.InvariantCulture)}));
            }

            return result;
        }

        protected IEnumerable<string> LoadSeachContentSourcesList()
        {
            var portals = PortalController.Instance.GetPortals();

            var result = new List<string>();
            foreach (var portal in portals)
            {
                var pi = portal as PortalInfo;

                if (pi != null)
                {
                    var list = InternalSearchController.Instance.GetSearchContentSourceList(pi.PortalID);
                    foreach (var src in list)
                    {
                        if (!src.IsPrivate && !result.Contains(src.LocalizedName))
                        {
                            result.Add(src.LocalizedName);
                        }
                    }
                }
            }
            return result;
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(Settings[settingName])))
            {
                return Convert.ToBoolean(Settings[settingName]);
            }

            return defaultValue;
        }

        private string GetStringSetting(string settingName, string defaultValue)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(Settings[settingName])))
            {
                return Convert.ToString(Settings[settingName]);
            }

            return defaultValue;
        }
    }
}
