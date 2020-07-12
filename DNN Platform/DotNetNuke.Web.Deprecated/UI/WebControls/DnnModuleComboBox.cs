// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
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

    public class DnnModuleComboBox : WebControl
    {
        private const string DefaultExtensionImage = "icon_extensions_32px.png";

        private DnnComboBox _moduleCombo;
        private string _originalValue;

        public event EventHandler ItemChanged;

        public int ItemCount
        {
            get
            {
                return this._moduleCombo.Items.Count;
            }
        }

        public string SelectedValue
        {
            get
            {
                return this._moduleCombo.SelectedValue;
            }
        }

        public string RadComboBoxClientId
        {
            get
            {
                return this._moduleCombo.ClientID;
            }
        }

        public Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> Filter { get; set; }

        public override bool Enabled
        {
            get
            {
                return this._moduleCombo.Enabled;
            }

            set
            {
                this._moduleCombo.Enabled = value;
            }
        }

        public void BindAllPortalDesktopModules()
        {
            this._moduleCombo.SelectedValue = null;
            this._moduleCombo.DataSource = this.GetPortalDesktopModules();
            this._moduleCombo.DataBind();
            this.BindPortalDesktopModuleImages();
        }

        public void BindTabModulesByTabID(int tabID)
        {
            this._moduleCombo.SelectedValue = null;
            this._moduleCombo.DataSource = GetTabModules(tabID);
            this._moduleCombo.DataBind();
            this.BindTabModuleImages(tabID);
        }

        public void SetModule(string code)
        {
            this._moduleCombo.SelectedIndex = this._moduleCombo.FindItemIndexByValue(code);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this._moduleCombo = new DnnComboBox();
            this._moduleCombo.DataValueField = "key";
            this._moduleCombo.DataTextField = "value";
            this.Controls.Add(this._moduleCombo);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this._originalValue = this.SelectedValue;
        }

        protected virtual void OnItemChanged()
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, new EventArgs());
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (this._moduleCombo.Items.FindItemByValue(this._originalValue) != null)
            {
                this._moduleCombo.Items.FindItemByValue(this._originalValue).Selected = true;
            }

            this._moduleCombo.Width = this.Width;
            base.OnPreRender(e);
        }

        private static Dictionary<int, string> GetTabModules(int tabID)
        {
            var tabModules = ModuleController.Instance.GetTabModules(tabID);

            // Is this tab from another site?
            var isRemote = TabController.Instance.GetTab(tabID, Null.NullInteger, false).PortalID != PortalSettings.Current.PortalId;

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

        private Dictionary<int, string> GetPortalDesktopModules()
        {
            IOrderedEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> portalModulesList;
            if (this.Filter == null)
            {
                portalModulesList = DesktopModuleController.GetPortalDesktopModules(PortalSettings.Current.PortalId)
                    .Where((kvp) => kvp.Value.DesktopModule.Category == "Uncategorised" || string.IsNullOrEmpty(kvp.Value.DesktopModule.Category))
                    .OrderBy(c => c.Key);
            }
            else
            {
                portalModulesList = DesktopModuleController.GetPortalDesktopModules(PortalSettings.Current.PortalId)
                    .Where(this.Filter)
                    .OrderBy(c => c.Key);
            }

            return portalModulesList.ToDictionary(
                portalModule => portalModule.Value.DesktopModuleID,
                portalModule => portalModule.Key);
        }

        private void BindPortalDesktopModuleImages()
        {
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            foreach (RadComboBoxItem item in this._moduleCombo.Items)
            {
                string imageUrl =
                    (from pkgs in packages
                     join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
                     where portMods.Value.DesktopModuleID.ToString() == item.Value
                     select pkgs.IconFile).FirstOrDefault();

                item.ImageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            }
        }

        private void BindTabModuleImages(int tabID)
        {
            var tabModules = ModuleController.Instance.GetTabModules(tabID);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var moduleDefnitions = ModuleDefinitionController.GetModuleDefinitions();
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            foreach (RadComboBoxItem item in this._moduleCombo.Items)
            {
                string imageUrl = (from pkgs in packages
                                   join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
                                   join modDefs in moduleDefnitions on portMods.Value.DesktopModuleID equals modDefs.Value.DesktopModuleID
                                   join tabMods in tabModules on modDefs.Value.DesktopModuleID equals tabMods.Value.DesktopModuleID
                                   where tabMods.Value.ModuleID.ToString() == item.Value
                                   select pkgs.IconFile).FirstOrDefault();

                item.ImageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            }
        }
    }
}
