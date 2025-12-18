// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    public class DnnModuleComboBox : DnnComboBox
    {
        private const string DefaultExtensionImage = "icon_extensions_32px.png";

        private DnnComboBox moduleCombo;
        private string originalValue;

        public event EventHandler ItemChanged;

        public int ItemCount
        {
            get
            {
                return this.moduleCombo.Items.Count;
            }
        }

        public string RadComboBoxClientId
        {
            get
            {
                return this.moduleCombo.ClientID;
            }
        }

        public Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> Filter { get; set; }

        /// <inheritdoc/>
        public override string SelectedValue
        {
            get
            {
                return this.moduleCombo.SelectedValue;
            }
        }

        /// <inheritdoc/>
        public override bool Enabled
        {
            get
            {
                return this.moduleCombo.Enabled;
            }

            set
            {
                this.moduleCombo.Enabled = value;
            }
        }

        public void BindAllPortalDesktopModules()
        {
            this.moduleCombo.SelectedValue = null;
            this.moduleCombo.DataSource = this.GetPortalDesktopModules();
            this.moduleCombo.DataBind();
            this.BindPortalDesktopModuleImages();
        }

        public void BindTabModulesByTabID(int tabID)
        {
            this.moduleCombo.SelectedValue = null;
            this.moduleCombo.DataSource = GetTabModules(tabID);
            this.moduleCombo.DataBind();
            this.BindTabModuleImages(tabID);
        }

        public void SetModule(string code)
        {
            this.moduleCombo.SelectedIndex = this.moduleCombo.FindItemIndexByValue(code);
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.moduleCombo = new DnnComboBox();
            this.moduleCombo.DataValueField = "key";
            this.moduleCombo.DataTextField = "value";
            this.Controls.Add(this.moduleCombo);
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.originalValue = this.SelectedValue;
        }

        protected virtual void OnItemChanged()
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, new EventArgs());
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            if (this.moduleCombo.FindItemByValue(this.originalValue) != null)
            {
                this.moduleCombo.FindItemByValue(this.originalValue).Selected = true;
            }

            this.moduleCombo.Width = this.Width;
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

            // foreach (var item in _moduleCombo.Items)
            // {
            //    string imageUrl =
            //        (from pkgs in packages
            //         join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
            //         where portMods.Value.DesktopModuleID.ToString() == item.Value
            //         select pkgs.IconFile).FirstOrDefault();

            // item.ImageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            // }
        }

        private void BindTabModuleImages(int tabID)
        {
            var tabModules = ModuleController.Instance.GetTabModules(tabID);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var moduleDefnitions = ModuleDefinitionController.GetModuleDefinitions();
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            // foreach (RadComboBoxItem item in _moduleCombo.Items)
            // {
            //    string imageUrl = (from pkgs in packages
            //                       join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
            //                       join modDefs in moduleDefnitions on portMods.Value.DesktopModuleID equals modDefs.Value.DesktopModuleID
            //                       join tabMods in tabModules on modDefs.Value.DesktopModuleID equals tabMods.Value.DesktopModuleID
            //                       where tabMods.Value.ModuleID.ToString() == item.Value
            //                       select pkgs.IconFile).FirstOrDefault();

            // item.ImageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            // }
        }
    }
}
