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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Modules
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Export : PortalModuleBase
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

        private string ExportModule(int moduleID, string fileName, IFolderInfo folder)
        {
            var strMessage = "";
            if (Module != null)
            {
                if (!String.IsNullOrEmpty(Module.DesktopModule.BusinessControllerClass) && Module.DesktopModule.IsPortable)
                {
                    try
                    {
                        var objObject = Reflection.CreateObject(Module.DesktopModule.BusinessControllerClass, Module.DesktopModule.BusinessControllerClass);
                        
						//Double-check
						if (objObject is IPortable)
                        {
                            XmlDocument moduleXml = new XmlDocument();
                            XmlNode moduleNode = ModuleController.SerializeModule(moduleXml, Module, true);

                            //add attributes to XML document
                            XmlAttribute typeAttribute = moduleXml.CreateAttribute("type");
                            typeAttribute.Value = CleanName(Module.DesktopModule.ModuleName);
                            moduleNode.Attributes.Append(typeAttribute);

                            XmlAttribute versionAttribute = moduleXml.CreateAttribute("version");
                            versionAttribute.Value = Module.DesktopModule.Version;
                            moduleNode.Attributes.Append(versionAttribute);

                            // Create content from XmlNode
                            StringWriter sw = new StringWriter();
                            XmlTextWriter xw = new XmlTextWriter(sw);
                            moduleNode.WriteTo(xw);
                            var content = sw.ToString();
                            if (!String.IsNullOrEmpty(content))
                            {
								//remove invalid chars in content -> DNN 26810: Handled by ModuleController.SerializeModule
	                            //content = Regex.Replace(content, _invalidCharsRegex, string.Empty);
								//add attributes to XML document
                                //content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "<content type=\"" + CleanName(Module.DesktopModule.ModuleName) + "\" version=\"" +
                                //          Module.DesktopModule.Version + "\">" + content + "</content>";

                                //First check the Portal limits will not be exceeded (this is approximate)
                                var strFile = PortalSettings.HomeDirectoryMapPath + folder.FolderPath + fileName;
                                if (PortalController.Instance.HasSpaceAvailable(PortalId, content.Length))
                                {
                                    //add file to Files table
									using (var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(content)))
									{
										FileManager.Instance.AddFile(folder, fileName, fileContent, true, true, "application/octet-stream");
									}
                                }
                                else
                                {
                                    strMessage += "<br>" + string.Format(Localization.GetString("DiskSpaceExceeded"), strFile);
                                }
                            }
                            else
                            {
                                strMessage = Localization.GetString("NoContent", LocalResourceFile);
                            }
                        }
                        else
                        {
                            strMessage = Localization.GetString("ExportNotSupported", LocalResourceFile);
                        }
                    }
                    catch
                    {
                        strMessage = Localization.GetString("Error", LocalResourceFile);
                    }
                }
                else
                {
                    strMessage = Localization.GetString("ExportNotSupported", LocalResourceFile);
                }
            }
            return strMessage;
        }

        private static string CleanName(string name)
        {
            var strName = name;
            const string strBadChars = ". ~`!@#$%^&*()-_+={[}]|\\:;<,>?/\"'";

            int intCounter;
            for (intCounter = 0; intCounter <= strBadChars.Length - 1; intCounter++)
            {
                strName = strName.Replace(strBadChars.Substring(intCounter, 1), "");
            }
            return strName;
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
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EXPORT", Module))
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
                if (Request.QueryString["moduleid"] != null)
                {
                    Int32.TryParse(Request.QueryString["moduleid"], out ModuleId);
                }
                if (!Page.IsPostBack)
                {
                    cmdCancel.NavigateUrl = ReturnURL;

                    cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    cboFolders.Services.Parameters.Add("permission", "ADD");
                    if (Module != null)
                    {
                        txtFile.Text = CleanName(Module.ModuleTitle);
                    }
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
                if (cboFolders.SelectedItem != null && !String.IsNullOrEmpty(txtFile.Text))
                {
                    var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                    if (folder != null)
                    {
                        var strFile = "content." + CleanName(Module.DesktopModule.ModuleName) + "." + CleanName(txtFile.Text) + ".xml";
                        var strMessage = ExportModule(ModuleId, strFile, folder);
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
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Validation", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
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