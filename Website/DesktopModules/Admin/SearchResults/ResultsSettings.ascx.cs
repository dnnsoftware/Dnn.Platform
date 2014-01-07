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
using System.Text;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Web.UI.WebControls;

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
                                var item = new DnnComboBoxItem(portal[0], portal[1]) {Checked = list.Contains(portal[1])};
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
                                var item = new DnnComboBoxItem(portal[0], portal[1]) { Checked = PortalId.ToString() == portal[1] };
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
                            var item = new DnnComboBoxItem(filter, filter) {Checked = list.Contains(filter)};
                            comboBoxFilters.Items.Add(item);
                        }
                    }
                    else
                    {
                        var filterList = LoadSeachContentSourcesList();
                        foreach (var filter in filterList)
                        {
                            var item = new DnnComboBoxItem(filter, filter) {Checked = true};
                            comboBoxFilters.Items.Add(item);
                        }
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(Settings["EnableWildSearch"])))
                    {
                        var enableWildSearch = Convert.ToBoolean(Settings["EnableWildSearch"]);
                        chkEnableWildSearch.Checked = enableWildSearch;
                    }
                    else
                    {
                        chkEnableWildSearch.Checked = true;
                    }
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
                    var objModules = new ModuleController();
                    
                    objModules.UpdateModuleSetting(ModuleId, "LinkTarget", comboBoxLinkTarget.SelectedValue);

                    var selectedPortals = new StringBuilder();
                    foreach (var p in comboBoxPortals.CheckedItems)
                    {
                        if (selectedPortals.Length > 0)
                        {
                            selectedPortals.AppendFormat("|{0}", p.Value);
                        }
                        else
                        {
                            selectedPortals.Append(p.Value);
                        }
                    }

                    objModules.UpdateModuleSetting(ModuleId, "ScopeForPortals", selectedPortals.ToString());

                    var selectedFilters = new StringBuilder();
                    foreach (var p in comboBoxFilters.CheckedItems)
                    {
                        if (selectedFilters.Length > 0)
                        {
                            selectedFilters.AppendFormat("|{0}", p.Value);
                        }
                        else
                        {
                            selectedFilters.Append(p.Value);
                        }
                    }

                    objModules.UpdateModuleSetting(ModuleId, "ScopeForFilters", selectedFilters.ToString());

                    objModules.UpdateModuleSetting(ModuleId, "EnableWildSearch", chkEnableWildSearch.Checked.ToString());
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
            var portalCtrl = new PortalController();
            var portals = portalCtrl.GetPortals();

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
    }
}