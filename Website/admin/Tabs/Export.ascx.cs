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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;

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

#endregion

namespace DotNetNuke.Modules.Admin.Tabs
{

    public partial class Export : PortalModuleBase
    {

        private TabInfo _tab;

        public TabInfo Tab
        {
            get
            {
                if (_tab == null)
                {
                    _tab = TabController.Instance.GetTab(TabId, PortalId, false);
                }
                return _tab;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes the Tab
        /// </summary>
        /// <param name="xmlTemplate">Reference to XmlDocument context</param>
        /// <param name="nodeTabs">Node to add the serialized objects</param>
        /// -----------------------------------------------------------------------------
        private void SerializeTab(XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
            var xmlTab = new XmlDocument();
            var nodeTab = TabController.SerializeTab(xmlTab, Tab, chkContent.Checked);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
        }

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!TabPermissionController.CanExportPage())
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdExport.Click += OnExportClick;

            try
            {
                if (Page.IsPostBack) return;
                cmdCancel.NavigateUrl = Globals.NavigateURL();
                var folderPath = "Templates/";
                var templateFolder = FolderManager.Instance.GetFolder(UserInfo.PortalID, folderPath);
                cboFolders.Services.Parameters.Add("permission", "ADD");
                
                if (templateFolder != null && IsAccessibleByUser(templateFolder))
                {
                    cboFolders.SelectedFolder = templateFolder;
                }
                
                if (Tab != null)
                {
                    txtFile.Text = Globals.CleanName(Tab.TabName);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private bool IsAccessibleByUser(IFolderInfo folder)
        {
            return FolderPermissionController.Instance.CanAddFolder(folder);
        }

        protected void OnExportClick(Object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid)
                {
                    return;
                }

                if (cboFolders.SelectedItem != null)
                {
                    var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                    if (folder != null)
                    {
                        var filename = folder.FolderPath + txtFile.Text + ".page.template";
                        filename = filename.Replace("/", "\\");

                        var xmlTemplate = new XmlDocument();
                        XmlNode nodePortal = xmlTemplate.AppendChild(xmlTemplate.CreateElement("portal"));
                        if (nodePortal.Attributes != null)
                        {
                            nodePortal.Attributes.Append(XmlUtils.CreateAttribute(xmlTemplate, "version", "3.0"));
                        }

                        //Add template description
                        XmlElement node = xmlTemplate.CreateElement("description");
                        node.InnerXml = Server.HtmlEncode(txtDescription.Text);
                        nodePortal.AppendChild(node);

                        //Serialize tabs
                        XmlNode nodeTabs = nodePortal.AppendChild(xmlTemplate.CreateElement("tabs"));
                        SerializeTab(xmlTemplate, nodeTabs);

                        UI.Skins.Skin.AddModuleMessage(this, "", string.Format(Localization.GetString("ExportedMessage", LocalResourceFile), filename), ModuleMessage.ModuleMessageType.BlueInfo);

                        //add file to Files table
						using (var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(xmlTemplate.OuterXml)))
						{
							Services.FileSystem.FileManager.Instance.AddFile(folder, txtFile.Text + ".page.template", fileContent, true, true, "application/octet-stream");
						}
						
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

        }

        #endregion

    }
}