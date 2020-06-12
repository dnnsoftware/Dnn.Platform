// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
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
    /// -----------------------------------------------------------------------------
    public partial class Export : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public Export()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region Private Members

        private new int ModuleId = -1;
        private ModuleInfo _module;

        private ModuleInfo Module
        {
            get
            {
                return this._module ?? (this._module = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false));
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this._navigationManager.NavigateURL();
            }
        }

        #endregion

        #region Private Methods

        private string ExportModule(int moduleID, string fileName, IFolderInfo folder)
        {
            var strMessage = "";
            if (this.Module != null)
            {
                if (!String.IsNullOrEmpty(this.Module.DesktopModule.BusinessControllerClass) && this.Module.DesktopModule.IsPortable)
                {
                    try
                    {
                        var objObject = Reflection.CreateObject(this.Module.DesktopModule.BusinessControllerClass, this.Module.DesktopModule.BusinessControllerClass);

                        // Double-check
                        if (objObject is IPortable)
                        {
                            XmlDocument moduleXml = new XmlDocument { XmlResolver = null };
                            XmlNode moduleNode = ModuleController.SerializeModule(moduleXml, this.Module, true);

                            // add attributes to XML document
                            XmlAttribute typeAttribute = moduleXml.CreateAttribute("type");
                            typeAttribute.Value = CleanName(this.Module.DesktopModule.ModuleName);
                            moduleNode.Attributes.Append(typeAttribute);

                            XmlAttribute versionAttribute = moduleXml.CreateAttribute("version");
                            versionAttribute.Value = this.Module.DesktopModule.Version;
                            moduleNode.Attributes.Append(versionAttribute);

                            // Create content from XmlNode
                            StringWriter sw = new StringWriter();
                            XmlTextWriter xw = new XmlTextWriter(sw);
                            moduleNode.WriteTo(xw);
                            var content = sw.ToString();
                            if (!String.IsNullOrEmpty(content))
                            {
                                // remove invalid chars in content -> DNN 26810: Handled by ModuleController.SerializeModule
                                // content = Regex.Replace(content, _invalidCharsRegex, string.Empty);
                                // add attributes to XML document
                                // content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "<content type=\"" + CleanName(Module.DesktopModule.ModuleName) + "\" version=\"" +
                                //          Module.DesktopModule.Version + "\">" + content + "</content>";

                                // First check the Portal limits will not be exceeded (this is approximate)
                                if (PortalController.Instance.HasSpaceAvailable(this.PortalId, content.Length))
                                {
                                    // add file to Files table
                                    using (var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                                    {
                                        Services.FileSystem.FileManager.Instance.AddFile(folder, fileName, fileContent, true, true, "application/octet-stream");
                                    }
                                }
                                else
                                {
                                    strMessage += "<br>" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
                                }
                            }
                            else
                            {
                                strMessage = Localization.GetString("NoContent", this.LocalResourceFile);
                            }
                        }
                        else
                        {
                            strMessage = Localization.GetString("ExportNotSupported", this.LocalResourceFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is InvalidFileExtensionException || ex is PermissionsNotMetException || ex is InvalidFilenameException)
                        {
                            strMessage = ex.Message;
                        }
                        else
                        {
                            strMessage = Localization.GetString("Error", this.LocalResourceFile);
                        }
                    }
                }
                else
                {
                    strMessage = Localization.GetString("ExportNotSupported", this.LocalResourceFile);
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

            if (this.Request.QueryString["moduleid"] != null)
            {
                Int32.TryParse(this.Request.QueryString["moduleid"], out this.ModuleId);
            }
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EXPORT", this.Module))
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
                if (this.Request.QueryString["moduleid"] != null)
                {
                    Int32.TryParse(this.Request.QueryString["moduleid"], out this.ModuleId);
                }
                if (!this.Page.IsPostBack)
                {
                    this.cmdCancel.NavigateUrl = this.ReturnURL;

                    this.cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    this.cboFolders.Services.Parameters.Add("permission", "ADD");
                    if (this.Module != null)
                    {
                        this.txtFile.Text = CleanName(this.Module.ModuleTitle);
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
                IFolderInfo folder = null;
                if (this.cboFolders.SelectedItem != null && !String.IsNullOrEmpty(this.txtFile.Text))
                {
                    folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
                }

                if (folder != null)
                {
                    var strFile = "content." + CleanName(this.Module.DesktopModule.ModuleName) + "." + CleanName(this.txtFile.Text) + ".export";
                    var strMessage = this.ExportModule(this.ModuleId, strFile, folder);
                    if (String.IsNullOrEmpty(strMessage))
                    {
                        this.Response.Redirect(this.ReturnURL, true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Validation", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
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
