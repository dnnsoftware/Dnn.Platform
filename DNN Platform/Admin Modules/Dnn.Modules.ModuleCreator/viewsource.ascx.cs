// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Module.ModuleCreator
{
    using System;
    using System.IO;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    public partial class ViewSource : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;

        public ViewSource()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected int ModuleControlId
        {
            get
            {
                var moduleControlId = Null.NullInteger;
                if (this.Request.QueryString["ctlid"] != null)
                {
                    moduleControlId = int.Parse(this.Request.QueryString["ctlid"]);
                }

                return moduleControlId;
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this._navigationManager.NavigateURL();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.cboFile.SelectedIndexChanged += this.OnFileIndexChanged;
            this.optLanguage.SelectedIndexChanged += this.OnLanguageSelectedIndexChanged;
            this.cboTemplate.SelectedIndexChanged += this.cboTemplate_SelectedIndexChanged;
            this.cmdUpdate.Click += this.OnUpdateClick;
            this.cmdPackage.Click += this.OnPackageClick;
            this.cmdConfigure.Click += this.OnConfigureClick;
            this.cmdCreate.Click += this.OnCreateClick;

            if (this.Page.IsPostBack == false)
            {
                this.cmdCancel1.NavigateUrl = this.ReturnURL;
                this.cmdCancel2.NavigateUrl = this.ReturnURL;

                var objModuleControl = ModuleControlController.GetModuleControl(this.ModuleControlId);
                if (objModuleControl != null)
                {
                    this.BindFiles(objModuleControl.ControlSrc);
                    this.LoadFile();
                }

                if (this.Request.UrlReferrer != null)
                {
                    this.ViewState["UrlReferrer"] = Convert.ToString(this.Request.UrlReferrer);
                }
                else
                {
                    this.ViewState["UrlReferrer"] = string.Empty;
                }

                this.LoadLanguages();
                this.LoadModuleTemplates();
                if (this.cboTemplate.Items.FindByText("Module - User Control") != null)
                {
                    this.cboTemplate.Items.FindByText("Module - User Control").Selected = true;
                }

                this.LoadReadMe();
            }
        }

        protected void OnFileIndexChanged(object sender, EventArgs e)
        {
            this.LoadFile();
        }

        protected void OnLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadModuleTemplates();
        }

        protected void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadReadMe();
        }

        private void BindFiles(string controlSrc)
        {
            string[] fileList;
            this.cboFile.Items.Clear();

            var objModuleControl = ModuleControlController.GetModuleControl(this.ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, this.PortalId);

            var relativePath = $"DesktopModules/{(objModuleControl.ControlSrc.EndsWith(".mvc") ? "MVC/" : string.Empty)}{objDesktopModule.FolderName}/";
            var modulePath = this.Server.MapPath(relativePath);

            if (Directory.Exists(modulePath))
            {
                // iterate through files in desktopmodules folder
                fileList = Directory.GetFiles(modulePath, "*", SearchOption.AllDirectories);
                foreach (string filePath in fileList)
                {
                    switch (Path.GetExtension(filePath).ToLowerInvariant())
                    {
                        case ".ascx":
                            this.cboFile.Items.Add(new ListItem(filePath.Substring(modulePath.Length), filePath));
                            var resxPath = filePath.Replace(Path.GetFileName(filePath), "App_LocalResources\\" + Path.GetFileName(filePath)) + ".resx";
                            if (File.Exists(resxPath))
                            {
                                this.cboFile.Items.Add(new ListItem(filePath.Substring(modulePath.Length), resxPath));
                            }

                            break;
                        case ".vb":
                        case ".cs":
                        case ".vbhtml":
                        case ".cshtml":
                        case ".css":
                        case ".js":
                        case ".txt":
                        case ".html":
                        case ".xml":
                        case ".xslt":
                        case ".sql":
                        case ".sqldataprovider":
                            this.cboFile.Items.Add(new ListItem(filePath.Substring(modulePath.Length), filePath));
                            break;
                    }
                }
            }
            else
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("FolderNameInvalid", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }

            // iterate through files in app_code folder
            modulePath = Globals.ApplicationMapPath + "\\App_Code\\" + objDesktopModule.FolderName.Replace("/", "\\") + "\\";
            if (Directory.Exists(modulePath))
            {
                fileList = Directory.GetFiles(modulePath);
                foreach (string filePath in fileList)
                {
                    switch (Path.GetExtension(filePath).ToLowerInvariant())
                    {
                        case ".vb":
                            this.cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                        case ".cs":
                            this.cboFile.Items.Add(new ListItem(Path.GetFileName(filePath), filePath));
                            break;
                    }
                }
            }

            // select file
            if (this.cboFile.Items.FindByValue(Globals.ApplicationMapPath + "\\" + controlSrc.Replace("/", "\\")) != null)
            {
                this.cboFile.Items.FindByValue(Globals.ApplicationMapPath + "\\" + controlSrc.Replace("/", "\\")).Selected = true;
            }
        }

        private void LoadFile()
        {
            this.lblPath.Text = this.cboFile.SelectedValue;
            var objStreamReader = File.OpenText(this.lblPath.Text);
            this.txtSource.Text = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            this.SetFileType(this.lblPath.Text);
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

            DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(this.Page, "mimeType", mimeType, true);
        }

        private void SaveFile()
        {
            try
            {
                File.SetAttributes(this.cboFile.SelectedValue, FileAttributes.Normal);
                var objStream = File.CreateText(this.cboFile.SelectedValue);
                objStream.WriteLine(this.txtSource.Text);
                objStream.Close();
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ControlUpdated", this.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void LoadLanguages()
        {
            this.optLanguage.Items.Clear();
            var moduleTemplatePath = this.Server.MapPath(this.ControlPath) + "Templates";
            string[] folderList = Directory.GetDirectories(moduleTemplatePath);
            foreach (string folderPath in folderList)
            {
                this.optLanguage.Items.Add(new ListItem(Path.GetFileName(folderPath)));
            }

            var language = string.Empty;
            foreach (ListItem objFile in this.cboFile.Items)
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

            if (language == string.Empty)
            {
                this.optLanguage.SelectedIndex = 0;
            }
            else
            {
                this.optLanguage.Items.FindByValue(language).Selected = true;
            }
        }

        private void LoadModuleTemplates()
        {
            this.cboTemplate.Items.Clear();
            var moduleTemplatePath = this.Server.MapPath(this.ControlPath) + "Templates\\" + this.optLanguage.SelectedValue;
            string[] folderList = Directory.GetDirectories(moduleTemplatePath);
            foreach (string folderPath in folderList)
            {
                this.cboTemplate.Items.Add(new ListItem(Path.GetFileName(folderPath)));
            }

            this.cboTemplate.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", string.Empty));
        }

        private void LoadReadMe()
        {
            var templatePath = this.Server.MapPath(this.ControlPath) + "Templates\\" + this.optLanguage.SelectedValue + "\\" + this.cboTemplate.SelectedItem.Value;
            if (File.Exists(templatePath + "\\readme.txt"))
            {
                var readMe = Null.NullString;
                using (TextReader tr = new StreamReader(templatePath + "\\readme.txt"))
                {
                    readMe = tr.ReadToEnd();
                    tr.Close();
                }

                this.lblDescription.Text = readMe.Replace("\n", "<br/>");
            }
            else
            {
                this.lblDescription.Text = string.Empty;
            }

            // Determine if Control Name is required
            var controlNameRequired = false;
            var controlName = "<Not Required>";
            string[] fileList = Directory.GetFiles(templatePath);
            foreach (string filePath in fileList)
            {
                if (Path.GetFileName(filePath).ToLowerInvariant().IndexOf("template") > -1)
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

            this.txtControl.Text = controlName;
            this.txtControl.Enabled = controlNameRequired;
            if (this.txtControl.Enabled)
            {
                if (!this.cboTemplate.SelectedItem.Value.ToLowerInvariant().StartsWith("module"))
                {
                    var objModuleControl = ModuleControlController.GetModuleControl(this.ModuleControlId);
                    var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
                    var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, this.PortalId);
                    this.txtControl.Text = objDesktopModule.FriendlyName;
                }
            }
        }

        private string CreateModuleControl()
        {
            var objModuleControl = ModuleControlController.GetModuleControl(this.ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, this.PortalId);
            var objPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == objDesktopModule.PackageID);

            var moduleTemplatePath = this.Server.MapPath(this.ControlPath) + "Templates\\" + this.optLanguage.SelectedValue + "\\" + this.cboTemplate.SelectedValue + "\\";

            EventLogController.Instance.AddLog("Processing Template Folder", moduleTemplatePath, this.PortalSettings, -1, EventLogController.EventLogType.HOST_ALERT);

            var controlName = Null.NullString;
            var fileName = Null.NullString;
            var modulePath = Null.NullString;
            var sourceCode = Null.NullString;

            // iterate through files in template folder
            string[] fileList = Directory.GetFiles(moduleTemplatePath);
            foreach (string filePath in fileList)
            {
                modulePath = this.Server.MapPath("DesktopModules/" + objDesktopModule.FolderName + "/");

                // open file
                using (TextReader tr = new StreamReader(filePath))
                {
                    sourceCode = tr.ReadToEnd();
                    tr.Close();
                }

                // replace tokens
                var owner = objPackage.Owner.Replace(" ", string.Empty);
                if (string.IsNullOrEmpty(owner))
                {
                    owner = "DNN";
                }

                sourceCode = sourceCode.Replace("_OWNER_", owner);
                sourceCode = sourceCode.Replace("_MODULE_", objDesktopModule.FriendlyName.Replace(" ", string.Empty));
                sourceCode = sourceCode.Replace("_CONTROL_", this.GetControl());
                sourceCode = sourceCode.Replace("_YEAR_", DateTime.Now.Year.ToString());

                // get filename
                fileName = Path.GetFileName(filePath);
                fileName = fileName.Replace("template", this.GetControl());
                fileName = fileName.Replace("_OWNER_", objPackage.Owner.Replace(" ", string.Empty));
                fileName = fileName.Replace("_MODULE_", objDesktopModule.FriendlyName.Replace(" ", string.Empty));
                fileName = fileName.Replace("_CONTROL_", this.GetControl());

                switch (Path.GetExtension(filePath).ToLowerInvariant())
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
                        if (filePath.ToLowerInvariant().IndexOf(".ascx") == -1)
                        {
                            modulePath = modulePath.Replace("DesktopModules", "App_Code");
                        }

                        break;
                    case ".cs":
                        if (filePath.ToLowerInvariant().IndexOf(".ascx") == -1)
                        {
                            modulePath = modulePath.Replace("DesktopModules", "App_Code");
                        }

                        break;
                    case ".js":
                        modulePath = modulePath + "\\js\\";
                        break;
                }

                // check if folder exists
                if (!Directory.Exists(modulePath))
                {
                    Directory.CreateDirectory(modulePath);
                }

                // check if file already exists
                if (!File.Exists(modulePath + fileName))
                {
                    // create file
                    using (TextWriter tw = new StreamWriter(modulePath + fileName))
                    {
                        tw.WriteLine(sourceCode);
                        tw.Close();
                    }

                    EventLogController.Instance.AddLog("Created File", modulePath + fileName, this.PortalSettings, -1, EventLogController.EventLogType.HOST_ALERT);
                }
            }

            // Create module control
            if (controlName != Null.NullString)
            {
                try
                {
                    objModuleControl = new ModuleControlInfo();
                    objModuleControl.ModuleControlID = Null.NullInteger;
                    objModuleControl.ModuleDefID = objModuleDefinition.ModuleDefID;
                    objModuleControl.ControlKey = this.GetControl();
                    objModuleControl.ControlSrc = "DesktopModules/" + objDesktopModule.FolderName + "/" + controlName;
                    objModuleControl.ControlTitle = this.txtControl.Text;
                    objModuleControl.ControlType = (SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), this.cboType.SelectedItem.Value);
                    objModuleControl.HelpURL = string.Empty;
                    objModuleControl.IconFile = string.Empty;
                    objModuleControl.ViewOrder = 0;
                    objModuleControl.SupportsPartialRendering = true;
                    objModuleControl.SupportsPopUps = true;
                    ModuleControlController.AddModuleControl(objModuleControl);
                    controlName = objModuleControl.ControlSrc;
                }
                catch
                {
                    // Suppress error
                }
            }

            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ControlCreated", this.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);

            return controlName;
        }

        private string GetControl()
        {
            return this.txtControl.Text.Replace(" ", string.Empty);
        }

        private void OnUpdateClick(object sender, EventArgs e)
        {
            this.SaveFile();
        }

        private void OnPackageClick(object sender, EventArgs e)
        {
            var objModuleControl = ModuleControlController.GetModuleControl(this.ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, this.PortalId);
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(-1, "Extensions");
            this.Response.Redirect(this._navigationManager.NavigateURL(objModule.TabID, "PackageWriter", "rtab=" + this.TabId.ToString(), "packageId=" + objDesktopModule.PackageID.ToString(), "mid=" + objModule.ModuleID.ToString()) + "?popUp=true", true);
        }

        private void OnConfigureClick(object sender, EventArgs e)
        {
            var objModuleControl = ModuleControlController.GetModuleControl(this.ModuleControlId);
            var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
            var objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, this.PortalId);
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(-1, "Extensions");
            this.Response.Redirect(this._navigationManager.NavigateURL(objModule.TabID, "Edit", "mid=" + objModule.ModuleID.ToString(), "PackageID=" + objDesktopModule.PackageID.ToString()) + "?popUp=true", true);
        }

        private void OnCreateClick(object sender, EventArgs e)
        {
            if (this.cboTemplate.SelectedIndex > 0 && this.txtControl.Text != string.Empty)
            {
                var controlSrc = this.CreateModuleControl();
                this.BindFiles(controlSrc);
                this.LoadFile();
            }
            else
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AddControlError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }
    }
}
