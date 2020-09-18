// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Module.ModuleCreator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    public partial class CreateModule : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;

        public CreateModule()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.optLanguage.SelectedIndexChanged += this.optLanguage_SelectedIndexChanged;
            this.cboTemplate.SelectedIndexChanged += this.cboTemplate_SelectedIndexChanged;
            this.cmdCreate.Click += this.cmdCreate_Click;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.UserInfo.IsSuperUser)
            {
                if (!this.Page.IsPostBack)
                {
                    Dictionary<string, string> HostSettings = HostController.Instance.GetSettingsDictionary();
                    if (HostSettings.ContainsKey("Owner"))
                    {
                        this.txtOwner.Text = HostSettings["Owner"];
                    }

                    this.LoadLanguages();
                    this.LoadModuleTemplates();
                    this.txtControl.Text = "View";
                }
            }
            else
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SuperUser.ErrorMessage", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                this.createForm.Visible = false;
            }
        }

        protected void optLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadModuleTemplates();
        }

        protected void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadReadMe();
        }

        protected void cmdCreate_Click(object sender, EventArgs e)
        {
            if (this.UserInfo.IsSuperUser)
            {
                if (!string.IsNullOrEmpty(this.txtOwner.Text) && !string.IsNullOrEmpty(this.txtModule.Text) && this.cboTemplate.SelectedIndex > 0 && !string.IsNullOrEmpty(this.txtControl.Text))
                {
                    HostController.Instance.Update("Owner", this.txtOwner.Text, false);
                    if (this.CreateModuleDefinition())
                    {
                        this.Response.Redirect(this._navigationManager.NavigateURL(), true);
                    }
                }
                else
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InputValidation.ErrorMessage", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                }
            }
        }

        private void LoadReadMe()
        {
            var readMePath = this.Server.MapPath(this.ControlPath) + "Templates\\" + this.optLanguage.SelectedValue + "\\" + this.cboTemplate.SelectedItem.Value + "\\readme.txt";
            if (File.Exists(readMePath))
            {
                var readMe = Null.NullString;
                using (TextReader tr = new StreamReader(readMePath))
                {
                    readMe = tr.ReadToEnd();
                    tr.Close();
                    this.lblDescription.Text = readMe.Replace("\n", "<br/>");
                }
            }
            else
            {
                this.lblDescription.Text = string.Empty;
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

            this.optLanguage.SelectedIndex = 0;
        }

        private void LoadModuleTemplates()
        {
            this.cboTemplate.Items.Clear();
            var moduleTemplatePath = this.Server.MapPath(this.ControlPath) + "Templates\\" + this.optLanguage.SelectedValue;
            string[] folderList = Directory.GetDirectories(moduleTemplatePath);
            foreach (string folderPath in folderList)
            {
                if (Path.GetFileName(folderPath).ToLowerInvariant().StartsWith("module"))
                {
                    this.cboTemplate.Items.Add(new ListItem(Path.GetFileName(folderPath)));
                }
            }

            this.cboTemplate.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", string.Empty));
            if (this.cboTemplate.Items.FindByText("Module - User Control") != null)
            {
                this.cboTemplate.Items.FindByText("Module - User Control").Selected = true;
            }
            else
            {
                this.cboTemplate.SelectedIndex = 0;
            }

            this.LoadReadMe();
        }

        private void CreateModuleFolder()
        {
            var moduleFolderPath = Globals.ApplicationMapPath + "\\DesktopModules\\" + this.GetFolderName().Replace("/", "\\");

            if (!Directory.Exists(moduleFolderPath))
            {
                Directory.CreateDirectory(moduleFolderPath);
            }
        }

        private string CreateModuleControl()
        {
            var moduleTemplatePath = this.Server.MapPath(this.ControlPath) + "Templates\\" + this.optLanguage.SelectedValue + "\\" + this.cboTemplate.SelectedValue + "\\";

            EventLogController.Instance.AddLog("Processing Template Folder", moduleTemplatePath, this.PortalSettings, -1, EventLogController.EventLogType.HOST_ALERT);

            var controlName = Null.NullString;
            var fileName = Null.NullString;
            var modulePath = string.Empty;
            var sourceCode = Null.NullString;

            // iterate through files in template folder
            string[] fileList = Directory.GetFiles(moduleTemplatePath);
            foreach (string filePath in fileList)
            {
                modulePath = this.Server.MapPath("DesktopModules/" + this.GetFolderName() + "/");

                // open file
                using (TextReader tr = new StreamReader(filePath))
                {
                    sourceCode = tr.ReadToEnd();
                    tr.Close();
                }

                // replace tokens
                sourceCode = sourceCode.Replace("_OWNER_", this.GetOwner());
                sourceCode = sourceCode.Replace("_MODULE_", this.GetModule());
                sourceCode = sourceCode.Replace("_CONTROL_", this.GetControl());
                sourceCode = sourceCode.Replace("_YEAR_", DateTime.Now.Year.ToString());

                // get filename
                fileName = Path.GetFileName(filePath);
                fileName = fileName.Replace("template", this.GetControl());
                fileName = fileName.Replace("_OWNER_", this.GetOwner());
                fileName = fileName.Replace("_MODULE_", this.GetModule());
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

            return controlName;
        }

        private string GetOwner()
        {
            return this.txtOwner.Text.Replace(" ", string.Empty);
        }

        private string GetModule()
        {
            return this.txtModule.Text.Replace(" ", string.Empty);
        }

        private string GetControl()
        {
            return this.txtControl.Text.Replace(" ", string.Empty);
        }

        private string GetFolderName()
        {
            var strFolder = Null.NullString;
            strFolder += this.txtOwner.Text + "/" + this.txtModule.Text;

            // return folder and remove any spaces that might appear in folder structure
            return strFolder.Replace(" ", string.Empty);
        }

        private string GetClassName()
        {
            var strClass = Null.NullString;
            strClass += this.txtOwner.Text + "." + this.txtModule.Text;

            // return class and remove any spaces that might appear in class name
            return strClass.Replace(" ", string.Empty);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// </remarks>
        private bool CreateModuleDefinition()
        {
            try
            {
                if (PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == this.GetClassName()) == null)
                {
                    var controlName = Null.NullString;

                    // Create module folder
                    this.CreateModuleFolder();

                    // Create module control
                    controlName = this.CreateModuleControl();
                    if (controlName != string.Empty)
                    {
                        // Create package
                        var objPackage = new PackageInfo();
                        objPackage.Name = this.GetClassName();
                        objPackage.FriendlyName = this.txtModule.Text;
                        objPackage.Description = this.txtDescription.Text;
                        objPackage.Version = new Version(1, 0, 0);
                        objPackage.PackageType = "Module";
                        objPackage.License = string.Empty;
                        objPackage.Owner = this.txtOwner.Text;
                        objPackage.Organization = this.txtOwner.Text;
                        objPackage.FolderName = "DesktopModules/" + this.GetFolderName();
                        objPackage.License = "The license for this package is not currently included within the installation file, please check with the vendor for full license details.";
                        objPackage.ReleaseNotes = "This package has no Release Notes.";
                        PackageController.Instance.SaveExtensionPackage(objPackage);

                        // Create desktopmodule
                        var objDesktopModule = new DesktopModuleInfo();
                        objDesktopModule.DesktopModuleID = Null.NullInteger;
                        objDesktopModule.ModuleName = this.GetClassName();
                        objDesktopModule.FolderName = this.GetFolderName();
                        objDesktopModule.FriendlyName = this.txtModule.Text;
                        objDesktopModule.Description = this.txtDescription.Text;
                        objDesktopModule.IsPremium = false;
                        objDesktopModule.IsAdmin = false;
                        objDesktopModule.Version = "01.00.00";
                        objDesktopModule.BusinessControllerClass = string.Empty;
                        objDesktopModule.CompatibleVersions = string.Empty;
                        objDesktopModule.AdminPage = string.Empty;
                        objDesktopModule.HostPage = string.Empty;
                        objDesktopModule.Dependencies = string.Empty;
                        objDesktopModule.Permissions = string.Empty;
                        objDesktopModule.PackageID = objPackage.PackageID;
                        objDesktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(objDesktopModule, false, true);
                        objDesktopModule = DesktopModuleController.GetDesktopModule(objDesktopModule.DesktopModuleID, Null.NullInteger);

                        // Add OwnerName to the DesktopModule taxonomy and associate it with this module
                        var vocabularyId = -1;
                        var termId = -1;
                        var objTermController = DotNetNuke.Entities.Content.Common.Util.GetTermController();
                        var objTerms = objTermController.GetTermsByVocabulary("Module_Categories");
                        foreach (Term term in objTerms)
                        {
                            vocabularyId = term.VocabularyId;
                            if (term.Name == this.txtOwner.Text)
                            {
                                termId = term.TermId;
                            }
                        }

                        if (termId == -1)
                        {
                            termId = objTermController.AddTerm(new Term(vocabularyId) { Name = this.txtOwner.Text });
                        }

                        var objTerm = objTermController.GetTerm(termId);
                        var objContentController = DotNetNuke.Entities.Content.Common.Util.GetContentController();
                        var objContent = objContentController.GetContentItem(objDesktopModule.ContentItemId);
                        objTermController.AddTermToContent(objTerm, objContent);

                        // Add desktopmodule to all portals
                        DesktopModuleController.AddDesktopModuleToPortals(objDesktopModule.DesktopModuleID);

                        // Create module definition
                        var objModuleDefinition = new ModuleDefinitionInfo();
                        objModuleDefinition.ModuleDefID = Null.NullInteger;
                        objModuleDefinition.DesktopModuleID = objDesktopModule.DesktopModuleID;

                        // need core enhancement to have a unique DefinitionName
                        objModuleDefinition.FriendlyName = this.GetClassName();

                        // objModuleDefinition.FriendlyName = txtModule.Text;
                        // objModuleDefinition.DefinitionName = GetClassName();
                        objModuleDefinition.DefaultCacheTime = 0;
                        objModuleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(objModuleDefinition, false, true);

                        // Create modulecontrol
                        var objModuleControl = new ModuleControlInfo();
                        objModuleControl.ModuleControlID = Null.NullInteger;
                        objModuleControl.ModuleDefID = objModuleDefinition.ModuleDefID;
                        objModuleControl.ControlKey = string.Empty;
                        objModuleControl.ControlSrc = "DesktopModules/" + this.GetFolderName() + "/" + controlName;
                        objModuleControl.ControlTitle = string.Empty;
                        objModuleControl.ControlType = SecurityAccessLevel.View;
                        objModuleControl.HelpURL = string.Empty;
                        objModuleControl.IconFile = string.Empty;
                        objModuleControl.ViewOrder = 0;
                        objModuleControl.SupportsPartialRendering = false;
                        objModuleControl.SupportsPopUps = false;
                        ModuleControlController.AddModuleControl(objModuleControl);

                        // Update current module to reference new moduledefinition
                        var objModule = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
                        objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                        objModule.ModuleTitle = this.txtModule.Text;

                        // HACK - need core enhancement to be able to update ModuleDefID
                        using (DotNetNuke.Data.DataProvider.Instance().ExecuteSQL(
                            "UPDATE {databaseOwner}{objectQualifier}Modules SET ModuleDefID=" + objModule.ModuleDefID + " WHERE ModuleID=" + this.ModuleId))
                        {
                        }

                        ModuleController.Instance.UpdateModule(objModule);

                        return true;
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TemplateProblem.ErrorMessage", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                        return false;
                    }
                }
                else
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AlreadyExists.ErrorMessage", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    return false;
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, exc.ToString(), ModuleMessage.ModuleMessageType.RedError);
                return false;
            }
        }
    }
}
