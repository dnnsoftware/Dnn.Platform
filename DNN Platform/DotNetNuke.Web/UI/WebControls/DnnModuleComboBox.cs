#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Installer.Packages;

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnModuleComboBox : WebControl
    {
        private const string DefaultExtensionImage = "icon_extensions_32px.png";

        #region Public Events

        public event EventHandler ItemChanged;

        #endregion

        #region Public Properties

        public Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> Filter { get; set; }

        public int ItemCount
        {
            get
            {
                return _moduleCombo.Items.Count;
            }
        }

        public string SelectedValue
        {
            get
            {
                return _moduleCombo.SelectedValue;
            }
        }

        public string RadComboBoxClientId
        {
            get
            {
                return _moduleCombo.ClientID;
            }
        }

        #endregion

        #region Private Methods

        private Dictionary<int, string> GetPortalDesktopModules()
        {
            IOrderedEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> portalModulesList;
            if (Filter == null)
            {
                portalModulesList = DesktopModuleController.GetPortalDesktopModules(PortalSettings.Current.PortalId)
                    .Where((kvp) => kvp.Value.DesktopModule.Category == "Uncategorised" || String.IsNullOrEmpty(kvp.Value.DesktopModule.Category))
                    .OrderBy(c => c.Key);
            }
            else
            {
                portalModulesList = DesktopModuleController.GetPortalDesktopModules(PortalSettings.Current.PortalId)
                    .Where(Filter)
                    .OrderBy(c => c.Key);
            }

            return portalModulesList.ToDictionary(portalModule => portalModule.Value.DesktopModuleID, 
                                                    portalModule => portalModule.Key);
        }

        private static Dictionary<int, string> GetTabModules(int tabID)
        {
            var tabCtrl = new TabController();
            var moduleCtrl = new ModuleController();
            var tabModules = moduleCtrl.GetTabModules(tabID);

            // Is this tab from another site?
            var isRemote = tabCtrl.GetTab(tabID, Null.NullInteger, false).PortalID != PortalSettings.Current.PortalId;

            var pageModules = tabModules.Values.Where(m => !isRemote || ModuleSuportsSharing(m)).Where(m => ModulePermissionController.CanAdminModule(m) && m.IsDeleted == false).ToList();

            return pageModules.ToDictionary(module => module.ModuleID, module => module.ModuleTitle);
        }

        private static bool ModuleSuportsSharing(ModuleInfo moduleInfo)
        {
            switch (moduleInfo.DesktopModule.Shareable)
            {
                case ModuleSharing.Supported:
                case ModuleSharing.Unknown:
                    return moduleInfo.IsShareable;
                default:
                    return false;
            }
        }

        private void BindPortalDesktopModuleImages()
        {
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            foreach (RadComboBoxItem item in _moduleCombo.Items)
            {
                string imageUrl =
                    (from pkgs in packages
                     join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
                     where portMods.Value.DesktopModuleID.ToString() == item.Value
                     select pkgs.IconFile).FirstOrDefault();

                item.ImageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            }
        }

        private void BindTabModuleImages(int tabID)
        {
            var tabModules = new ModuleController().GetTabModules(tabID);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var moduleDefnitions = ModuleDefinitionController.GetModuleDefinitions();
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            foreach (RadComboBoxItem item in _moduleCombo.Items)
            {
                string imageUrl = (from pkgs in packages
                                   join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
                                   join modDefs in moduleDefnitions on portMods.Value.DesktopModuleID equals modDefs.Value.DesktopModuleID
                                   join tabMods in tabModules on modDefs.Value.DesktopModuleID equals tabMods.Value.DesktopModuleID
                                   where tabMods.Value.ModuleID.ToString() == item.Value
                                   select pkgs.IconFile).FirstOrDefault();

                item.ImageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _moduleCombo = new DnnComboBox();
            _moduleCombo.DataValueField = "key";
            _moduleCombo.DataTextField = "value";
            Controls.Add(_moduleCombo);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _originalValue = SelectedValue;
        }

        protected virtual void OnItemChanged()
        {
            if (ItemChanged != null)
            {
                ItemChanged(this, new EventArgs());
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (_moduleCombo.Items.FindItemByValue(_originalValue) != null)
            {
                _moduleCombo.Items.FindItemByValue(_originalValue).Selected = true;
            }

            _moduleCombo.Width = Width;
            base.OnPreRender(e);
        }

        #endregion

        #region Public Methods

        public void BindAllPortalDesktopModules()
        {
            _moduleCombo.DataSource = GetPortalDesktopModules();
            _moduleCombo.DataBind();
            BindPortalDesktopModuleImages();
        }

        public void BindTabModulesByTabID(int tabID)
        {
            _moduleCombo.DataSource = GetTabModules(tabID);
            _moduleCombo.DataBind();
            BindTabModuleImages(tabID);
        }

        public void SetModule(string code)
        {
            _moduleCombo.SelectedIndex = _moduleCombo.FindItemIndexByValue(code);
        }

        #endregion

        private DnnComboBox _moduleCombo;
        private string _originalValue;
    }
}