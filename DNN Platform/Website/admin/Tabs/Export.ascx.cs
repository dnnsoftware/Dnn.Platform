// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Tabs
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    public partial class Export : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;
        private TabInfo _tab;

        public Export()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public TabInfo Tab
        {
            get
            {
                if (this._tab == null)
                {
                    this._tab = TabController.Instance.GetTab(this.TabId, this.PortalId, false);
                }

                return this._tab;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!TabPermissionController.CanExportPage())
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdExport.Click += this.OnExportClick;

            try
            {
                if (this.Page.IsPostBack)
                {
                    return;
                }

                this.cmdCancel.NavigateUrl = this._navigationManager.NavigateURL();
                var folderPath = "Templates/";
                var templateFolder = FolderManager.Instance.GetFolder(this.UserInfo.PortalID, folderPath);
                this.cboFolders.Services.Parameters.Add("permission", "ADD");

                if (templateFolder != null && this.IsAccessibleByUser(templateFolder))
                {
                    this.cboFolders.SelectedFolder = templateFolder;
                }

                if (this.Tab != null)
                {
                    this.txtFile.Text = Globals.CleanName(this.Tab.TabName);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnExportClick(object sender, EventArgs e)
        {
            try
            {
                if (!this.Page.IsValid)
                {
                    return;
                }

                if (this.cboFolders.SelectedItem != null)
                {
                    var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
                    if (folder != null)
                    {
                        var filename = folder.FolderPath + this.txtFile.Text + ".page.template";
                        filename = filename.Replace("/", "\\");

                        var xmlTemplate = new XmlDocument { XmlResolver = null };
                        XmlNode nodePortal = xmlTemplate.AppendChild(xmlTemplate.CreateElement("portal"));
                        if (nodePortal.Attributes != null)
                        {
                            nodePortal.Attributes.Append(XmlUtils.CreateAttribute(xmlTemplate, "version", "3.0"));
                        }

                        // Add template description
                        XmlElement node = xmlTemplate.CreateElement("description");
                        node.InnerXml = this.Server.HtmlEncode(this.txtDescription.Text);
                        nodePortal.AppendChild(node);

                        // Serialize tabs
                        XmlNode nodeTabs = nodePortal.AppendChild(xmlTemplate.CreateElement("tabs"));
                        this.SerializeTab(xmlTemplate, nodeTabs);

                        UI.Skins.Skin.AddModuleMessage(this, string.Empty, string.Format(Localization.GetString("ExportedMessage", this.LocalResourceFile), filename), ModuleMessage.ModuleMessageType.BlueInfo);

                        // add file to Files table
                        using (var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(xmlTemplate.OuterXml)))
                        {
                            Services.FileSystem.FileManager.Instance.AddFile(folder, this.txtFile.Text + ".page.template", fileContent, true, true, "application/octet-stream");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes the Tab.
        /// </summary>
        /// <param name="xmlTemplate">Reference to XmlDocument context.</param>
        /// <param name="nodeTabs">Node to add the serialized objects.</param>
        /// -----------------------------------------------------------------------------
        private void SerializeTab(XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
            var xmlTab = new XmlDocument { XmlResolver = null };
            var nodeTab = TabController.SerializeTab(xmlTab, this.Tab, this.chkContent.Checked);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
        }

        private bool IsAccessibleByUser(IFolderInfo folder)
        {
            return FolderPermissionController.Instance.CanAddFolder(folder);
        }
    }
}
