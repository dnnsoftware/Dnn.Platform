#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2012
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Security;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Installer.Writers;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Modules.Admin.Modules
{
    public partial class ViewSource : PortalModuleBase
    {

        #region Private Members

        protected int ModuleControlId
        {
            get
            {
                var moduleControlId = Null.NullInteger;
                if ((Request.QueryString["ctlid"] != null))
                {
                    moduleControlId = Int32.Parse(Request.QueryString["ctlid"]);
                }
                return moduleControlId;
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

        private void BindFiles(string controlSrc)
        {
            string[] fileList;
            cboFile.Items.Clear();

            var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, PortalId);

            var modulePath = Server.MapPath("DesktopModules/" + objDesktopModule.FolderName + "/");

            if (Directory.Exists(modulePath))
            {
                //iterate through files in desktopmodules folder
                fileList = Directory.GetFiles(modulePath);
                foreach (string filePath in fileList)
                {
                    switch (Path.GetExtension(filePath).ToLower())
                    {
                        case ".ascx":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            var resxPath = filePath.Replace(Path.GetFileName(filePath), "App_LocalResources\\" + Path.GetFileName(filePath)) + ".resx";
                            if (File.Exists(resxPath))
                            {
                                cboFile.Items.Add(new ListItem(Path.GetFileName(resxPath), resxPath));
                            }
                            break;
                        case ".vb":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".cs":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".vbhtml":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".cshtml":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".css":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".js":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".txt":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".html":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".xml":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".xslt":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".sql":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".sqldataprovider":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                    }
                }
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("FolderNameInvalid", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }


            //iterate through files in app_code folder
            modulePath = Globals.ApplicationMapPath + "\\App_Code\\" + objDesktopModule.FolderName.Replace("/", "\\") + "\\";
            if (Directory.Exists(modulePath))
            {
                fileList = Directory.GetFiles(modulePath);
                foreach (string filePath in fileList)
                {
                    switch (Path.GetExtension(filePath).ToLower())
                    {
                        case ".vb":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".cs":
                            cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                    }
                }
            }

            // select file
            if (cboFile.Items.FindByValue(Globals.ApplicationMapPath + "\\" + controlSrc.Replace("/", "\\")) != null)
            {
                cboFile.Items.FindByValue(Globals.ApplicationMapPath + "\\" + controlSrc.Replace("/", "\\")).Selected = true;
            }

        }

        private void LoadFile()
        {
            lblPath.Text = cboFile.SelectedValue;
            var objStreamReader = File.OpenText(lblPath.Text);
            txtSource.Text = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            SetFileType(lblPath.Text);
        }

        private void SetFileType(string filePath)
        {
            string mimeType;
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".vb":
                    mimeType = "text/x-vb";
                    break;
                case ".cs":
                    mimeType = "text/x-csharp";
                    break;
                case ".css":
                    mimeType = "text/css";
                    break;
                case ".js":
                    mimeType = "text/javascript";
                    break;
                case ".xml":
                case ".xslt":
                    mimeType = "application/xml";
                    break;
                case ".sql":
                case ".sqldataprovider":
                    mimeType = "text/x-sql";
                    break;
                default:
                    mimeType = "text/html";
                    break;
            }
            DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(Page, "mimeType", mimeType, true);
        }

        private void SaveFile()
        {
            try
            {
                File.SetAttributes(cboFile.SelectedValue, FileAttributes.Normal);
                var objStream = File.CreateText(cboFile.SelectedValue);
                objStream.WriteLine(txtSource.Text);
                objStream.Close();
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ControlUpdated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void LoadLanguages()
        {
            optLanguage.Items.Clear();
            var moduleTemplatePath = Server.MapPath(ModulePath) + "Templates";
            string[] folderList = Directory.GetDirectories(moduleTemplatePath);
            foreach (string folderPath in folderList)
            {
                optLanguage.Items.Add(new ListItem(Path.GetFileName(folderPath)));
            }

            var language = "";
            foreach (ListItem objFile in cboFile.Items)
            {
                if (objFile.Text.EndsWith(".vb"))
                {
                    language = "VB";
                }
                if (objFile.Text.EndsWith(".cs"))
                {
                    language = "C#";
                }
            }
            if (language == "")
            {
                optLanguage.SelectedIndex = 0;
            }
            else
            {
                optLanguage.Items.FindByValue(language).Selected = true;
            }
        }

        private void LoadModuleTemplates()
        {
            cboTemplate.Items.Clear();
            var moduleTemplatePath = Server.MapPath(ModulePath) + "Templates\\" + optLanguage.SelectedValue;
            string[] folderList = Directory.GetDirectories(moduleTemplatePath);
            foreach (string folderPath in folderList)
            {
                cboTemplate.Items.Add(new ListItem(Path.GetFileName(folderPath)));
            }
            cboTemplate.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", ""));
        }

        private void LoadReadMe()
        {
            var templatePath = Server.MapPath(ModulePath) + "Templates\\" + optLanguage.SelectedValue + "\\" + cboTemplate.SelectedItem.Value;
            if (File.Exists(templatePath + "\\readme.txt"))
            {
                var readMe = Null.NullString;
                TextReader tr = new StreamReader(templatePath + "\\readme.txt");
                readMe = tr.ReadToEnd();
                tr.Close();
                lblDescription.Text = readMe.Replace("\n", "<br/>");
            }
            else
            {
                lblDescription.Text = "";
            }

            //Determine if Control Name is required
            var controlNameRequired = false;
            var controlName = "<Not Required>";
            string[] fileList = Directory.GetFiles(templatePath);
            foreach (string filePath in fileList)
            {
                if (Path.GetFileName(filePath).ToLower().IndexOf("template") > -1)
                {
                    controlNameRequired = true;
                    controlName = "Edit";
                }
                else
                {
                    if (Path.GetFileName(filePath).EndsWith(".ascx"))
                    {
                        controlName = Path.GetFileNameWithoutExtension(filePath);
                    }
                }
            }
            txtControl.Text = controlName;
            txtControl.Enabled = controlNameRequired;
            if (txtControl.Enabled)
            {
                if (!cboTemplate.SelectedItem.Value.ToLower().StartsWith("module"))
                {
                    var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
                    var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
                    var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, PortalId);
                    txtControl.Text = objDesktopModule.FriendlyName;
                }
            }
        }

        private string CreateModuleControl()
        {
            var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, PortalId);
            var objPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID ==objDesktopModule.PackageID);

            var moduleTemplatePath = Server.MapPath(ModulePath) + "Templates\\" + optLanguage.SelectedValue + "\\" + cboTemplate.SelectedValue + "\\";


            EventLogController.Instance.AddLog("Processing Template Folder", moduleTemplatePath, PortalSettings, -1, EventLogController.EventLogType.HOST_ALERT);


            var controlName = Null.NullString;
            var fileName = Null.NullString;
            var modulePath = Null.NullString;
            var sourceCode = Null.NullString;

            //iterate through files in template folder
            string[] fileList = Directory.GetFiles(moduleTemplatePath);
            foreach (string filePath in fileList)
            {
                modulePath = Server.MapPath("DesktopModules/" + objDesktopModule.FolderName + "/");

                //open file
                TextReader tr = new StreamReader(filePath);
                sourceCode = tr.ReadToEnd();
                tr.Close();

                //replace tokens
                var owner = objPackage.Owner.Replace(" ", "");
                if (string.IsNullOrEmpty(owner))
                {
                    owner = "DNN";
                }
                sourceCode = sourceCode.Replace("_OWNER_", owner);
                sourceCode = sourceCode.Replace("_MODULE_", objDesktopModule.FriendlyName.Replace(" ", ""));
                sourceCode = sourceCode.Replace("_CONTROL_", GetControl());
                sourceCode = sourceCode.Replace("_YEAR_", DateTime.Now.Year.ToString());

                //get filename 
                fileName = Path.GetFileName(filePath);
                fileName = fileName.Replace("template", GetControl());
                fileName = fileName.Replace("_CONTROL_", GetControl());

                switch (Path.GetExtension(filePath).ToLower())
                {
                    case ".ascx":
                        controlName = fileName;
                        break;
                    case ".vbhtml":
                        controlName = fileName;
                        break;
                    case ".cshtml":
                        controlName = fileName;
                        break;
                    case ".resx":
                        modulePath = modulePath + "\\App_LocalResources\\";
                        break;
                    case ".vb":
                        if (filePath.ToLower().IndexOf(".ascx") == -1)
                        {
                            modulePath = modulePath.Replace("DesktopModules", "App_Code");
                        }
                        break;
                    case ".cs":
                        if (filePath.ToLower().IndexOf(".ascx") == -1)
                        {
                            modulePath = modulePath.Replace("DesktopModules", "App_Code");
                        }
                        break;
                    case ".js":
                        modulePath = modulePath + "\\js\\";
                        break;
                }

                //check if folder exists
                if (!Directory.Exists(modulePath))
                {
                    Directory.CreateDirectory(modulePath);
                }

                //check if file already exists
                if (!File.Exists(modulePath + fileName))
                {
                    //create file
                    TextWriter tw = new StreamWriter(modulePath + fileName);
                    tw.WriteLine(sourceCode);
                    tw.Close();

                    EventLogController.Instance.AddLog("Created File", modulePath + fileName, PortalSettings, -1, EventLogController.EventLogType.HOST_ALERT);

                }
            }

            //Create module control
            if (controlName != Null.NullString)
            {
                try
                {
                    objModuleControl = new ModuleControlInfo();
                    objModuleControl.ModuleControlID = Null.NullInteger;
                    objModuleControl.ModuleDefID = objModuleDefinition.ModuleDefID;
                    objModuleControl.ControlKey = GetControl();
                    objModuleControl.ControlSrc = "DesktopModules/" + objDesktopModule.FolderName + "/" + controlName;
                    objModuleControl.ControlTitle = txtControl.Text;
                    objModuleControl.ControlType = (SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), cboType.SelectedItem.Value);
                    objModuleControl.HelpURL = "";
                    objModuleControl.IconFile = "";
                    objModuleControl.ViewOrder = 0;
                    objModuleControl.SupportsPartialRendering = true;
                    objModuleControl.SupportsPopUps = true;
                    ModuleControlController.AddModuleControl(objModuleControl);
                    controlName = objModuleControl.ControlSrc;
                }
                catch
                {
                    //Suppress error
                }
            }

            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ControlCreated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);

            return controlName;
        }

        private string GetControl()
        {
            return txtControl.Text.Replace(" ", "");
        }


        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            cboFile.SelectedIndexChanged += OnFileIndexChanged;
            optLanguage.SelectedIndexChanged += OnLanguageSelectedIndexChanged;
            cboTemplate.SelectedIndexChanged += cboTemplate_SelectedIndexChanged;
            cmdUpdate.Click += OnUpdateClick;
            cmdPackage.Click += OnPackageClick;
            cmdConfigure.Click += OnConfigureClick;
            cmdCreate.Click += OnCreateClick;

            if (Page.IsPostBack == false)
            {
                cmdCancel1.NavigateUrl = ReturnURL;
                cmdCancel2.NavigateUrl = ReturnURL;

                var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
                if (objModuleControl != null)
                {
                    BindFiles(objModuleControl.ControlSrc);
                    LoadFile();
                }

                if (Request.UrlReferrer != null)
                {
                    ViewState["UrlReferrer"] = Convert.ToString(Request.UrlReferrer);
                }
                else
                {
                    ViewState["UrlReferrer"] = "";
                }

                LoadLanguages();
                LoadModuleTemplates();
                if (cboTemplate.Items.FindByText("Module - User Control") != null)
                {
                    cboTemplate.Items.FindByText("Module - User Control").Selected = true;
                }
                LoadReadMe();
            }

        }

        protected void OnFileIndexChanged(object sender, EventArgs e)
        {
            LoadFile();
        }

        protected void OnLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadModuleTemplates();
        }

        protected void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReadMe();
        }

        private void OnUpdateClick(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void OnPackageClick(object sender, EventArgs e)
        {
            var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, PortalId);
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(-1, "Extensions");
            Response.Redirect(Globals.NavigateURL(objModule.TabID, "PackageWriter", "rtab=" + TabId.ToString(), "packageId=" + objDesktopModule.PackageID.ToString(), "mid=" + objModule.ModuleID.ToString()) + "?popUp=true", true);
        }

        private void OnConfigureClick(object sender, EventArgs e)
        {
            var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, PortalId);
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(-1, "Extensions");
            Response.Redirect(Globals.NavigateURL(objModule.TabID, "Edit", "mid=" + objModule.ModuleID.ToString(), "PackageID=" + objDesktopModule.PackageID.ToString()) + "?popUp=true", true);
        }

        private void OnCreateClick(object sender, EventArgs e)
        {
            if (cboTemplate.SelectedIndex > 0 && txtControl.Text != "")
            {
                var controlSrc = CreateModuleControl();
                BindFiles(controlSrc);
                LoadFile();
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AddControlError", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        #endregion

    }
}