#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

using ICSharpCode.SharpZipLib.Zip;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Globalization;

#endregion

namespace DotNetNuke.Modules.Admin.Portals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Template PortalModuleBase is used to export a Portal as a Template
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Template : PortalModuleBase
    {
        #region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes all Files
        /// </summary>
        /// <param name="objportal">Portal to serialize</param>
        /// <param name="folderPath">The folder containing the files</param>
        /// <remarks>
        /// The serialization uses the xml attributes defined in FileInfo class.
        /// </remarks>
        /// <history>
        /// 	[cnurse]	11/08/2004	Created
        ///     [cnurse]    05/20/2004  Extracted adding of file to zip to new FileSystemUtils method
        /// </history>
        /// -----------------------------------------------------------------------------
        private void SerializeFiles(XmlWriter writer, PortalInfo objportal, string folderPath, ref ZipOutputStream zipFile)
        {
            var folderManager = FolderManager.Instance;
            var objFolder = folderManager.GetFolder(objportal.PortalID, folderPath);

            writer.WriteStartElement("files");
            foreach (FileInfo objFile in folderManager.GetFiles(objFolder))
            {
                //verify that the file exists on the file system
                var filePath = objportal.HomeDirectoryMapPath + folderPath + objFile.FileName;
                if (File.Exists(filePath))
                {
                    writer.WriteStartElement("file");

                    writer.WriteElementString("contenttype", objFile.ContentType);
                    writer.WriteElementString("extension", objFile.Extension);
                    writer.WriteElementString("filename", objFile.FileName);
                    writer.WriteElementString("height", objFile.Height.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("size", objFile.Size.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("width", objFile.Width.ToString(CultureInfo.InvariantCulture));

                    writer.WriteEndElement();

                    FileSystemUtils.AddToZip(ref zipFile, filePath, objFile.FileName, folderPath);
                }
            }
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes all Folders including Permissions
        /// </summary>
        /// <param name="objportal">Portal to serialize</param>
        /// <remarks>
        /// The serialization uses the xml attributes defined in FolderInfo class.
        /// </remarks>
        /// <history>
        /// 	[cnurse]	11/08/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void SerializeFolders(XmlWriter writer, PortalInfo objportal, ref ZipOutputStream zipFile)
        {
            //Sync db and filesystem before exporting so all required files are found
            var folderManager = FolderManager.Instance;
            folderManager.Synchronize(objportal.PortalID);
            writer.WriteStartElement("folders");

            foreach (FolderInfo folder in folderManager.GetFolders(objportal.PortalID))
            {
                writer.WriteStartElement("folder");

                writer.WriteElementString("folderpath", folder.FolderPath);
                writer.WriteElementString("storagelocation", folder.StorageLocation.ToString());

                //Serialize Folder Permissions
                SerializeFolderPermissions(writer, objportal, folder.FolderPath);

                //Serialize files
                SerializeFiles(writer, objportal, folder.FolderPath, ref zipFile);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes all Folder Permissions
        /// </summary>
        /// <param name="objportal">Portal to serialize</param>
        /// <param name="folderPath">The folder containing the files</param>
        /// <remarks>
        /// The serialization uses the xml attributes defined in FolderInfo class.
        /// </remarks>
        /// <history>
        /// 	[cnurse]	11/08/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void SerializeFolderPermissions(XmlWriter writer, PortalInfo objportal, string folderPath)
        {
            FolderPermissionCollection permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(objportal.PortalID, folderPath);

            writer.WriteStartElement("folderpermissions");

            foreach (FolderPermissionInfo permission in permissions)
            {
                writer.WriteStartElement("permission");

                writer.WriteElementString("permissioncode", permission.PermissionCode);
                writer.WriteElementString("permissionkey", permission.PermissionKey);
                writer.WriteElementString("rolename", permission.RoleName);
                writer.WriteElementString("allowaccess", permission.AllowAccess.ToString().ToLowerInvariant());

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes all Profile Definitions
        /// </summary>
        /// <param name="objportal">Portal to serialize</param>
        /// <remarks>
        /// The serialization uses the xml attributes defined in ProfilePropertyDefinition class.
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        private void SerializeProfileDefinitions(XmlWriter writer, PortalInfo objportal)
        {
            var objListController = new ListController();

            writer.WriteStartElement("profiledefinitions");
            foreach (ProfilePropertyDefinition objProfileProperty in
                ProfileController.GetPropertyDefinitionsByPortal(objportal.PortalID, false, false))
            {
                writer.WriteStartElement("profiledefinition");

                writer.WriteElementString("propertycategory", objProfileProperty.PropertyCategory);
                writer.WriteElementString("propertyname", objProfileProperty.PropertyName);

                var objList = objListController.GetListEntryInfo("DataType", objProfileProperty.DataType);
                writer.WriteElementString("datatype", objList == null ? "Unknown" : objList.Value);
                writer.WriteElementString("length", objProfileProperty.Length.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("defaultvisibility", Convert.ToInt32(objProfileProperty.DefaultVisibility).ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void SerializeTabs(XmlWriter writer, PortalInfo portal, Hashtable tabs, TabCollection tabCollection)
        {
            foreach (TabInfo tab in tabCollection.Values)
            {
                //if not deleted
                if (!tab.IsDeleted)
                {
                    XmlNode tabNode = null;
                    if (string.IsNullOrEmpty(tab.CultureCode) || tab.CultureCode == portal.DefaultLanguage)
                    {
                        // page in default culture and checked or page doesn't exist in tree(which should always export).
                        var tabId = tab.TabID.ToString(CultureInfo.InvariantCulture);
                        if (ctlPages.FindNodeByValue(tabId) == null || ctlPages.CheckedNodes.Any(p => p.Value == tabId))
                        {
                            tabNode = TabController.SerializeTab(new XmlDocument(), tabs, tab, portal, chkContent.Checked);
                        }
                    }
                    else
                    {
                        // check if default culture page is selected or default page doesn't exist in tree(which should always export).
                        TabInfo defaultTab = tab.DefaultLanguageTab;
                        var tabId = defaultTab.TabID.ToString(CultureInfo.InvariantCulture);
                        if (defaultTab == null
                            || ctlPages.FindNodeByValue(tabId) == null
                            || ctlPages.CheckedNodes.Count(p => p.Value == defaultTab.TabID.ToString(CultureInfo.InvariantCulture)) > 0)
                        {
                            tabNode = TabController.SerializeTab(new XmlDocument(), tabs, tab, portal, chkContent.Checked);
                        }
                    }

                    if (tabNode != null)
                        tabNode.WriteTo(writer);
                }
            }

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes all portal Tabs
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="portal">Portal to serialize</param>
        /// <remarks>
        /// Only portal tabs will be exported to the template, Admin tabs are not exported.
        /// On each tab, all modules will also be exported.
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	23/09/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void SerializeTabs(XmlWriter writer, PortalInfo portal)
        {
            //supporting object to build the tab hierarchy
            var tabs = new Hashtable();

            writer.WriteStartElement("tabs");

            if (chkMultilanguage.Checked)
            {
                //Process Default Language first
                SerializeTabs(writer, portal, tabs, 
                    GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(portal.DefaultLanguage, true)));

                //Process other locales
                foreach (ListItem language in chkLanguages.Items)
                {
                    if (language.Selected && language.Value != portal.DefaultLanguage)
                    {
                        SerializeTabs(writer, portal, tabs,
                            GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(language.Value, false)));
                    }                        
                }
            }
            else
            {
                if (chkMultilanguage.Enabled)
                {
                    // only export 1 language
                    string language = languageComboBox.SelectedValue;
                    SerializeTabs(writer, portal, tabs, 
                        GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(language, true)));
                }
                else
                {
                    SerializeTabs(writer, portal, tabs, 
                        GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID)));
                }
            }

            writer.WriteEndElement();
        }

        private TabCollection GetExportableTabs(TabCollection tabs)
        {
            var exportableTabs = tabs.Where(kvp => !kvp.Value.IsSystem).Select(kvp => kvp.Value);
            return new TabCollection(exportableTabs);
        }

        private void SerializePortalSettings(XmlWriter writer, PortalInfo portal)
        {
            writer.WriteStartElement("settings");

            writer.WriteElementString("logofile", portal.LogoFile);
            writer.WriteElementString("footertext", portal.FooterText);
            writer.WriteElementString("userregistration", portal.UserRegistration.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("banneradvertising", portal.BannerAdvertising.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("defaultlanguage", portal.DefaultLanguage);

            Dictionary<string, string> settingsDictionary = PortalController.Instance.GetPortalSettings(portal.PortalID);

            string setting;
            settingsDictionary.TryGetValue("DefaultPortalSkin", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("skinsrc", setting);
            }
            settingsDictionary.TryGetValue("DefaultAdminSkin", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("skinsrcadmin", setting);
            }
            settingsDictionary.TryGetValue("DefaultPortalContainer", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("containersrc", setting);
            }
            settingsDictionary.TryGetValue("DefaultAdminContainer", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("containersrcadmin", setting);
            }
            settingsDictionary.TryGetValue("EnableSkinWidgets", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enableskinwidgets", setting);
            }
            settingsDictionary.TryGetValue("portalaliasmapping", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("portalaliasmapping", setting);
            }

            writer.WriteElementString("contentlocalizationenabled", chkMultilanguage.Checked.ToString());

            settingsDictionary.TryGetValue("TimeZone", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("timezone", setting);
            }

            settingsDictionary.TryGetValue("EnablePopUps", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enablepopups", setting);
            }

            settingsDictionary.TryGetValue("InlineEditorEnabled", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("inlineeditorenabled", setting);
            }

            settingsDictionary.TryGetValue("HideFoldersEnabled", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("hidefoldersenabled", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelMode", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelmode", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelSecurity", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelsecurity", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelVisibility", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelvisibility", setting);
            }

            writer.WriteElementString("hostspace", portal.HostSpace.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("userquota", portal.UserQuota.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("pagequota", portal.PageQuota.ToString(CultureInfo.InvariantCulture));

            settingsDictionary.TryGetValue("PageHeadText", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("pageheadtext", setting);
            }
            settingsDictionary.TryGetValue("InjectModuleHyperLink", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("injectmodulehyperlink", setting);
            }
            settingsDictionary.TryGetValue("AddCompatibleHttpHeader", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("addcompatiblehttpheader", setting);
            }

            //End Portal Settings
            writer.WriteEndElement();
        }

        private void SerializeEnabledLocales(XmlWriter writer, PortalInfo portal)
        {
            var enabledLocales = LocaleController.Instance.GetLocales(portal.PortalID);
            if (enabledLocales.Count > 1)
            {
                writer.WriteStartElement("locales");
                if (chkMultilanguage.Checked)
                {
                    foreach (ListItem item in this.chkLanguages.Items)
                    {
                        if (item.Selected)
                        {
                            writer.WriteElementString("locale", item.Value);
                        }
                    }
                }
                else
                {
                    foreach (var enabledLocale in enabledLocales)
                    {
                        writer.WriteElementString("locale", enabledLocale.Value.Code);
                    }
                }

                writer.WriteEndElement();
            }
        }

        private static void SerializeExtensionUrlProviders(XmlWriter writer, int portalId)
        {
            var providers = ExtensionUrlProviderController.GetModuleProviders(portalId);
            if (!providers.Any())
            {
                return;
            }

            writer.WriteStartElement("extensionUrlProviders");

            foreach (var provider in providers)
            {
                writer.WriteStartElement("extensionUrlProvider");
                writer.WriteElementString("name", provider.ProviderConfig.ProviderName);
                writer.WriteElementString("active", provider.ProviderConfig.IsActive.ToString());
                var settings = provider.ProviderConfig.Settings;
                if (settings.Any())
                {
                    writer.WriteStartElement("settings");
                    foreach (var setting in settings)
                    {
                        writer.WriteStartElement("setting");
                        writer.WriteAttributeString("name", setting.Key);
                        writer.WriteAttributeString("value", setting.Value);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                ////if (provider.ProviderConfig.TabIds.Any())
                ////{
                ////    writer.WriteStartElement("tabIds");
                ////    foreach (var tabId in provider.ProviderConfig.TabIds)
                ////    {
                ////        // TODO: translate to tab path?
                ////        writer.WriteElementString("tabId", tabId.ToString(CultureInfo.InvariantCulture));
                ////    }
                ////    writer.WriteEndElement();
                ////}

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void SetupSettings()
        {
            var portalInfo = PortalController.Instance.GetPortal(Convert.ToInt32(cboPortals.SelectedValue));

            Dictionary<string, string> settingsDictionary = PortalController.Instance.GetPortalSettings(portalInfo.PortalID);
            string setting;
            bool contentLocalizable = false;
            settingsDictionary.TryGetValue("ContentLocalizationEnabled", out setting);

            if (!String.IsNullOrEmpty(setting))
            {
                bool.TryParse(setting, out contentLocalizable);
            }
            if (contentLocalizable)
            {
                chkMultilanguage.Enabled = true;
                chkMultilanguage.Checked = true;
                rowLanguages.Visible = true;

                BindLocales(portalInfo);
            }
            else
            {
                chkMultilanguage.Enabled = false;
                chkMultilanguage.Checked = false;
                rowLanguages.Visible = false;
                rowMultiLanguage.Visible = false;
            }
            BindTree(portalInfo);
        }

        private void BindLocales(PortalInfo portalInfo)
        {
            var locales = LocaleController.Instance.GetLocales(portalInfo.PortalID).Values;
            MultiselectLanguages.Visible = false;
            SingleSelectLanguages.Visible = false;
            if (chkMultilanguage.Checked)
            {
                MultiselectLanguages.Visible = true;
                chkLanguages.DataTextField = "EnglishName";
                chkLanguages.DataValueField = "Code";
                chkLanguages.DataSource = locales;
                chkLanguages.DataBind();

                foreach (ListItem item in chkLanguages.Items)
                {
                    if (item.Value == portalInfo.DefaultLanguage)
                    {
                        item.Enabled = false;
                        item.Attributes.Add("title", string.Format(LocalizeString("DefaultLanguage"), item.Text));
                        lblNote.Text = string.Format(LocalizeString("lblNote"), item.Text);
                    }
                    item.Selected = true;
                }

            }
            else
            {
                languageComboBox.BindData(true);
                languageComboBox.SetLanguage(portalInfo.DefaultLanguage);

                SingleSelectLanguages.Visible = true;
                lblNoteSingleLanguage.Text = string.Format(LocalizeString("lblNoteSingleLanguage"), new CultureInfo(portalInfo.DefaultLanguage).EnglishName);

            }
        }

        #region Pages tree
        private bool IsAdminTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return perms.Cast<TabPermissionInfo>().All(perm => perm.RoleName == PortalSettings.AdministratorRoleName || !perm.AllowAccess);
        }
        private bool IsRegisteredUserTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return perms.Cast<TabPermissionInfo>().Any(perm => perm.RoleName == PortalSettings.RegisteredRoleName && perm.AllowAccess);
        }
        private static bool IsSecuredTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return perms.Cast<TabPermissionInfo>().All(perm => perm.RoleName != Globals.glbRoleAllUsersName || !perm.AllowAccess);
        }

        private string IconPortal
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Portal.png");
            }
        }
        private string GetNodeStatusIcon(TabInfo tab)
        {
            string s = "";
            if (tab.DisableLink)
            {
                s = s + string.Format("<img src=\"{0}\" alt=\"\" title=\"{1}\" class=\"statusicon\" />", IconPageDisabled, LocalizeString("lblDisabled"));
            }
            if (tab.IsVisible == false)
            {
                s = s + string.Format("<img src=\"{0}\" alt=\"\" title=\"{1}\" class=\"statusicon\" />", IconPageHidden, LocalizeString("lblHidden"));
            }
            if (tab.Url != "")
            {
                s = s + string.Format("<img src=\"{0}\" alt=\"\" title=\"{1}\" class=\"statusicon\" />", IconRedirect, LocalizeString("lblRedirect"));
            }
            return s;
        }
        private string IconPageDisabled
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Disabled.png");
            }
        }
        private string IconPageHidden
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Hidden.png");
            }
        }
        private string AllUsersIcon
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Everyone.png");
            }
        }
        private string AdminOnlyIcon
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserAdmin.png");
            }
        }
        private string IconHome
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Home.png");
            }
        }
        private string RegisteredUsersIcon
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_User.png");
            }
        }
        private string SecuredIcon
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserSecure.png");
            }
        }
        private string IconRedirect
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Redirect.png");
            }
        }
        private string GetNodeIcon(TabInfo tab, out string toolTip)
        {
            if (PortalSettings.HomeTabId == tab.TabID)
            {
                toolTip = LocalizeString("lblHome");
                return IconHome;
            }

            if (IsSecuredTab(tab))
            {
                if (IsAdminTab(tab))
                {
                    toolTip = LocalizeString("lblAdminOnly");
                    return AdminOnlyIcon;
                }

                if (IsRegisteredUserTab(tab))
                {
                    toolTip = LocalizeString("lblRegistered");
                    return RegisteredUsersIcon;
                }

                toolTip = LocalizeString("lblSecure");
                return SecuredIcon;
            }

            toolTip = LocalizeString("lblEveryone");
            return AllUsersIcon;
        }

        private void BindTree(PortalInfo portal)
        {
            ctlPages.Nodes.Clear();

            var rootNode = new RadTreeNode
                {
                    Text = PortalSettings.PortalName,
                    ImageUrl = IconPortal,
                    Value = Null.NullInteger.ToString(CultureInfo.InvariantCulture),
                    Expanded = true,
                    AllowEdit = false,
                    EnableContextMenu = true,
                    Checked = true
                };
            rootNode.Attributes.Add("isPortalRoot", "True");

            //var tabs = new TabCollection();
            var tabs = TabController.GetPortalTabs(chkMultilanguage.Checked ? TabController.GetTabsBySortOrder(portal.PortalID, portal.DefaultLanguage, true) : TabController.GetTabsBySortOrder(portal.PortalID, languageComboBox.SelectedValue, true), Null.NullInteger, false, "<" + Localization.GetString("None_Specified") + ">", true, false, true, false, false).Where(t => !t.IsSystem).ToList();

            foreach (var tab in tabs) 
            {
                if (tab.Level == 0 && tab.TabID != portal.AdminTabId)
                {
                    string tooltip;
                    var nodeIcon = GetNodeIcon(tab, out tooltip);
                    var node = new RadTreeNode
                    {
                        Text = string.Format("{0} {1}", tab.TabName, GetNodeStatusIcon(tab)),
                        Value = tab.TabID.ToString(CultureInfo.InvariantCulture),
                        AllowEdit = true,
                        ImageUrl = nodeIcon,
                        ToolTip = tooltip,
                        Checked = true
                    };

                    AddChildNodes(node, portal);
                    rootNode.Nodes.Add(node);
                }
            }

            ctlPages.Nodes.Add(rootNode);
        }
        private void AddChildNodes(RadTreeNode parentNode, PortalInfo portal)
        {
            parentNode.Nodes.Clear();

            var parentId = int.Parse(parentNode.Value);

            var tabs = GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(languageComboBox.SelectedValue, true)).WithParentId(parentId);


            foreach (var tab in tabs)
            {
                if (tab.ParentId == parentId)
                {
                    string tooltip;
                    var nodeIcon = GetNodeIcon(tab, out tooltip);
                    var node = new RadTreeNode
                    {
                        Text = string.Format("{0} {1}", tab.TabName, GetNodeStatusIcon(tab)),
                        Value = tab.TabID.ToString(CultureInfo.InvariantCulture),
                        AllowEdit = true,
                        ImageUrl = nodeIcon,
                        ToolTip = tooltip,
                        Checked = true
                    };
                    AddChildNodes(node, portal);
                    parentNode.Nodes.Add(node);
                }
            }
        }
        #endregion

        #endregion

        #region "EventHandlers"
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdExport.Click += cmdExport_Click;
            cboPortals.SelectedIndexChanged += cboPortals_SelectedIndexChanged;
            try
            {
                if (!Page.IsPostBack)
                {
                    cboPortals.DataTextField = "PortalName";
                    cboPortals.DataValueField = "PortalId";
                    cboPortals.DataSource = PortalController.Instance.GetPortals();
                    cboPortals.DataBind();
                    cboPortals.SelectedValue = PortalId.ToString(CultureInfo.InvariantCulture);
                    SetupSettings();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the Cancel Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	09/02/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Exports the selected portal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Template will be saved in Portals\_default folder.
        /// An extension of .template will be added to filename if not entered
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	23/09/2004	Created
        /// 	[cnurse]	11/08/2004	Addition of files to template
        ///  	[aprasad]	1/17/2011	New setting AutoAddPortalAlias
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdExport_Click(Object sender, EventArgs e)
        {
            try
            {
                // Validations
                bool isValid = true;

                // Verify all ancestor pages are selected
                foreach (RadTreeNode page in ctlPages.CheckedNodes)
                {
                    if (page.ParentNode != null && page.ParentNode.Value != "-1" && !page.ParentNode.Checked)
                        isValid = false;
                }
                if (!isValid)
                {
                    UI.Skins.Skin.AddModuleMessage(this, LocalizeString("ErrorAncestorPages"), ModuleMessage.ModuleMessageType.RedError);
                }

                if (ctlPages.CheckedNodes.Count == 0)
                {
                    isValid = false;
                    UI.Skins.Skin.AddModuleMessage(this, LocalizeString("ErrorPages"), ModuleMessage.ModuleMessageType.RedError);
                }

                if (!Page.IsValid || !isValid)
                {
                    return;
                }

                var settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;

                var filename = Globals.HostMapPath + txtTemplateName.Text;
                if (!filename.EndsWith(".template"))
                {
                    filename += ".template";
                }
                XmlWriter writer = XmlWriter.Create(filename, settings);

                writer.WriteStartElement("portal");
                writer.WriteAttributeString("version", "5.0");

                //Add template description
                writer.WriteElementString("description", Server.HtmlEncode(txtDescription.Text));

                //Serialize portal settings
                var portal = PortalController.Instance.GetPortal(Convert.ToInt32(cboPortals.SelectedValue));

                SerializePortalSettings(writer, portal);
                SerializeEnabledLocales(writer, portal);
                SerializeExtensionUrlProviders(writer, portal.PortalID);

                if (chkProfile.Checked)
                {
                    //Serialize Profile Definitions
                    SerializeProfileDefinitions(writer, portal);
                }

                if (chkModules.Checked)
                {
                    //Serialize Portal Desktop Modules
                    DesktopModuleController.SerializePortalDesktopModules(writer, portal.PortalID);
                }

                if (chkRoles.Checked)
                {
                    //Serialize Roles
                    RoleController.SerializeRoleGroups(writer, portal.PortalID);
                }

                //Serialize tabs
                SerializeTabs(writer, portal);

                if (chkFiles.Checked)
                {
                    //Create Zip File to hold files
                    var resourcesFile = new ZipOutputStream(File.Create(filename + ".resources"));
                    resourcesFile.SetLevel(6);

                    //Serialize folders (while adding files to zip file)
                    SerializeFolders(writer, portal, ref resourcesFile);

                    //Finish and Close Zip file
                    resourcesFile.Finish();
                    resourcesFile.Close();
                }
                writer.WriteEndElement();

                writer.Close();

                UI.Skins.Skin.AddModuleMessage(this, "", string.Format(Localization.GetString("ExportedMessage", LocalResourceFile), filename), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cboPortals_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupSettings();
        }

        protected void chkMultilanguage_OnCheckedChanged(object sender, EventArgs e)
        {
            var portalInfo = PortalController.Instance.GetPortal(Convert.ToInt32(cboPortals.SelectedValue));

            BindLocales(portalInfo);
            BindTree(portalInfo);
        }

        #endregion


        protected void languageComboBox_OnItemChanged(object sender, EventArgs e)
        {
            var portalInfo = PortalController.Instance.GetPortal(Convert.ToInt32(cboPortals.SelectedValue));
            BindTree(portalInfo);
        }
    }
}