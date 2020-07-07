// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.SearchResults
{
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

    public partial class ResultsSettings : ModuleSettingsBase
    {
        public override void LoadSettings()
        {
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["LinkTarget"])))
                    {
                        this.comboBoxLinkTarget.SelectedValue = Convert.ToString(this.Settings["LinkTarget"]);
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["ScopeForPortals"])))
                    {
                        var list = Convert.ToString(this.Settings["ScopeForPortals"]).Split('|').ToList();
                        var portalList = this.LoadPortalsList().ToList();
                        if (portalList.Any())
                        {
                            foreach (var portal in portalList)
                            {
                                var item = new ListItem(portal[0], portal[1]) { Selected = list.Contains(portal[1]) };
                                this.comboBoxPortals.Items.Add(item);
                            }
                        }
                        else
                        {
                            this.divPortalGroup.Visible = false;
                        }
                    }
                    else
                    {
                        var portalList = this.LoadPortalsList().ToList();
                        if (portalList.Any())
                        {
                            foreach (var portal in portalList)
                            {
                                var item = new ListItem(portal[0], portal[1]) { Selected = this.PortalId.ToString() == portal[1] };
                                this.comboBoxPortals.Items.Add(item);
                            }
                        }
                        else
                        {
                            this.divPortalGroup.Visible = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["ScopeForFilters"])))
                    {
                        var list = Convert.ToString(this.Settings["ScopeForFilters"]).Split('|').ToList();
                        var filterList = this.LoadSeachContentSourcesList();
                        foreach (var filter in filterList)
                        {
                            var item = new ListItem(filter, filter) { Selected = list.Contains(filter) };
                            this.comboBoxFilters.Items.Add(item);
                        }
                    }
                    else
                    {
                        var filterList = this.LoadSeachContentSourcesList();
                        foreach (var filter in filterList)
                        {
                            var item = new ListItem(filter, filter) { Selected = true };
                            this.comboBoxFilters.Items.Add(item);
                        }
                    }

                    var scopeForRoles =
                        PortalController.GetPortalSetting("SearchResult_ScopeForRoles", this.PortalId, string.Empty)
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var roles = RoleController.Instance.GetRoles(this.PortalId, r => !r.IsSystemRole || r.RoleName == "Registered Users");
                    roles.Insert(0, new RoleInfo() { RoleName = "Superusers" });

                    foreach (var role in roles)
                    {
                        var item = new ListItem(role.RoleName, role.RoleName) { Selected = scopeForRoles.Length == 0 || scopeForRoles.Contains(role.RoleName) };
                        this.comboBoxRoles.Items.Add(item);
                    }

                    this.chkEnableWildSearch.Checked = this.GetBooleanSetting("EnableWildSearch", true);
                    this.chkShowDescription.Checked = this.GetBooleanSetting("ShowDescription", true);
                    this.chkShowFriendlyTitle.Checked = this.GetBooleanSetting("ShowFriendlyTitle", true);
                    this.chkShowSnippet.Checked = this.GetBooleanSetting("ShowSnippet", true);
                    this.chkShowLastUpdated.Checked = this.GetBooleanSetting("ShowLastUpdated", true);
                    this.chkShowSource.Checked = this.GetBooleanSetting("ShowSource", true);
                    this.chkShowTags.Checked = this.GetBooleanSetting("ShowTags", true);

                    this.txtMaxDescriptionLength.Text = this.GetStringSetting("MaxDescriptionLength", "100");
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
                if (this.Page.IsValid)
                {
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "LinkTarget", this.comboBoxLinkTarget.SelectedValue);

                    var selectedPortals = this.comboBoxPortals.Value.Replace(",", "|");

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ScopeForPortals", selectedPortals);

                    var selectedFilters = this.comboBoxFilters.Value.Replace(",", "|");

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ScopeForFilters", selectedFilters.ToString());

                    var selectedRoles = this.comboBoxRoles.Value;
                    PortalController.UpdatePortalSetting(this.PortalId, "SearchResult_ScopeForRoles", selectedRoles);

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "EnableWildSearch", this.chkEnableWildSearch.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowDescription", this.chkShowDescription.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowFriendlyTitle", this.chkShowFriendlyTitle.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowSnippet", this.chkShowSnippet.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowLastUpdated", this.chkShowLastUpdated.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowSource", this.chkShowSource.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowTags", this.chkShowTags.Checked.ToString());

                    var maxDescriptionLength = this.txtMaxDescriptionLength.Text;
                    if (string.IsNullOrEmpty(maxDescriptionLength) || !Regex.IsMatch(maxDescriptionLength, "^\\d+$"))
                    {
                        maxDescriptionLength = "100";
                    }

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "MaxDescriptionLength", maxDescriptionLength);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            DataCache.RemoveCache(string.Format("ModuleInfos{0}", this.PortalSettings.PortalId));
        }

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
                    pi => new[] { pi.PortalName, pi.PortalID.ToString(CultureInfo.InvariantCulture) }));
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
            if (!string.IsNullOrEmpty(Convert.ToString(this.Settings[settingName])))
            {
                return Convert.ToBoolean(this.Settings[settingName]);
            }

            return defaultValue;
        }

        private string GetStringSetting(string settingName, string defaultValue)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(this.Settings[settingName])))
            {
                return Convert.ToString(this.Settings[settingName]);
            }

            return defaultValue;
        }
    }
}
