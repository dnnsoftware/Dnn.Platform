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
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Modules
{
    public partial class Import : PortalModuleBase
    {

        #region Private Members

        private new int ModuleId = -1;
        private ModuleInfo _module;

        private ModuleInfo Module
        {
            get
            {
                return _module ?? (_module = ModuleController.Instance.GetModule(ModuleId, TabId, false));
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(Request.Params["ReturnURL"]) ?? Globals.NavigateURL();
            }
        }

        #endregion

        #region Private Methods

        private string ImportModule()
        {
            var strMessage = "";
            if (Module != null)
            {
                if (!String.IsNullOrEmpty(Module.DesktopModule.BusinessControllerClass) && Module.DesktopModule.IsPortable)
                {
                    try
                    {
                        var objObject = Reflection.CreateObject(Module.DesktopModule.BusinessControllerClass, Module.DesktopModule.BusinessControllerClass);
                        if (objObject is IPortable)
                        {
                            var xmlDoc = new XmlDocument();
                            try
                            {
                                xmlDoc.LoadXml(txtContent.Text);
                            }
                            catch
                            {
                                strMessage = Localization.GetString("NotValidXml", LocalResourceFile);
                            }
                            if (String.IsNullOrEmpty(strMessage))
                            {
                                var strType = xmlDoc.DocumentElement.GetAttribute("type");
                                if (strType == Globals.CleanName(Module.DesktopModule.ModuleName) || strType == Globals.CleanName(Module.DesktopModule.FriendlyName))
                                {
                                    var strVersion = xmlDoc.DocumentElement.GetAttribute("version");
                                    // DNN26810 if rootnode = "content", import only content(the old way)
                                    if (xmlDoc.DocumentElement.Name.ToLower() == "content" )
                                    {
                                        ((IPortable)objObject).ImportModule(ModuleId, xmlDoc.DocumentElement.InnerXml, strVersion, UserInfo.UserID);
                                    }
                                    // otherwise (="module") import the new way
                                    else
                                    {
                                        ModuleController.DeserializeModule(xmlDoc.DocumentElement, Module, PortalId, TabId);
                                    }
                                    Response.Redirect(Globals.NavigateURL(), true);
                                }
                                else
                                {
                                    strMessage = Localization.GetString("NotCorrectType", LocalResourceFile);
                                }
                            }
                        }
                        else
                        {
                            strMessage = Localization.GetString("ImportNotSupported", LocalResourceFile);
                        }
                    }
                    catch
                    {
                        strMessage = Localization.GetString("Error", LocalResourceFile);
                    }
                }
                else
                {
                    strMessage = Localization.GetString("ImportNotSupported", LocalResourceFile);
                }
            }
            return strMessage;
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Request.QueryString["moduleid"] != null)
            {
                Int32.TryParse(Request.QueryString["moduleid"], out ModuleId);
            }

            //Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "IMPORT", Module))
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboFolders.SelectionChanged += OnFoldersIndexChanged;
            cboFiles.SelectedIndexChanged += OnFilesIndexChanged;
            cmdImport.Click += OnImportClick;

            try
            {
                if (!Page.IsPostBack)
                {
                    cmdCancel.NavigateUrl = ReturnURL;
                    cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    cboFolders.Services.Parameters.Add("permission", "ADD");
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnFoldersIndexChanged(object sender, EventArgs e)
        {
            cboFiles.Items.Clear();
            cboFiles.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "-");
            if (cboFolders.SelectedItem == null)
            {
                return;
            }
            if (Module == null)
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
            if (folder == null) return;

            var arrFiles = Globals.GetFileList(PortalId, "xml", false, folder.FolderPath);
            foreach (FileItem objFile in arrFiles)
            {
                if (objFile.Text.IndexOf("content." + Globals.CleanName(Module.DesktopModule.ModuleName) + ".", System.StringComparison.Ordinal) != -1)
                {
                    cboFiles.AddItem(objFile.Text.Replace("content." + Globals.CleanName(Module.DesktopModule.ModuleName) + ".", ""), objFile.Text);
                }

                //legacy support for files which used the FriendlyName
                if (Globals.CleanName(Module.DesktopModule.ModuleName) == Globals.CleanName(Module.DesktopModule.FriendlyName))
                {
                    continue;
                }

                if (objFile.Text.IndexOf("content." + Globals.CleanName(Module.DesktopModule.FriendlyName) + ".", System.StringComparison.Ordinal) != -1)
                {
                    cboFiles.AddItem(objFile.Text.Replace("content." + Globals.CleanName(Module.DesktopModule.FriendlyName) + ".", ""), objFile.Text);
                }
            }
        }

        protected void OnFilesIndexChanged(object sender, EventArgs e)
        {
            if (cboFolders.SelectedItem == null) return;
            var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
            if (folder == null) return;

            var objStreamReader = File.OpenText(PortalSettings.HomeDirectoryMapPath + folder.FolderPath + cboFiles.SelectedItem.Value);
            var content = objStreamReader.ReadToEnd();
            objStreamReader.Close();
            txtContent.Text = content.Replace("><", ">\r\n<");
        }

        protected void OnImportClick(object sender, EventArgs e)
        {
            try
            {
                if (Module != null)
                {
                    var strMessage = ImportModule();
                    if (String.IsNullOrEmpty(strMessage))
                    {
                        Response.Redirect(ReturnURL, true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
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