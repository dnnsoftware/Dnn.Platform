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
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DesktopModules.Admin.ModuleCreator
{

    public partial class CreateModule : PortalModuleBase
    {

        #region Private Methods

        private void LoadReadMe()
        {
            var readMePath = Server.MapPath(ModulePath) + "Templates\\" + optLanguage.SelectedValue + "\\" + cboTemplate.SelectedItem.Value + "\\readme.txt";
            if (File.Exists(readMePath))
            {
                var readMe = Null.NullString;
                TextReader tr = new StreamReader(readMePath);
                readMe = tr.ReadToEnd();
                tr.Close();
                lblDescription.Text = readMe.Replace("\n", "<br/>");
            }
            else
            {
                lblDescription.Text = "";
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
            optLanguage.SelectedIndex = 0;
        }

        private void LoadModuleTemplates()
        {
            cboTemplate.Items.Clear();
            var moduleTemplatePath = Server.MapPath(ModulePath) + "Templates\\" + optLanguage.SelectedValue;
            string[] folderList = Directory.GetDirectories(moduleTemplatePath);
            foreach (string folderPath in folderList)
            {
                if (Path.GetFileName(folderPath).ToLower().StartsWith("module"))
                {
                    cboTemplate.Items.Add(new ListItem(Path.GetFileName(folderPath)));
                }
            }
            cboTemplate.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", ""));
            if (cboTemplate.Items.FindByText("Module - User Control") != null)
            {
                cboTemplate.Items.FindByText("Module - User Control").Selected = true;
            }
            else
            {
                cboTemplate.SelectedIndex = 0;
            }
            LoadReadMe();
        }

        private void CreateModuleFolder()
        {
            var moduleFolderPath = Globals.ApplicationMapPath + "\\DesktopModules\\" + GetFolderName().Replace("/", "\\");

            if (!Directory.Exists(moduleFolderPath))
            {
                Directory.CreateDirectory(moduleFolderPath);
            }
        }

        private string CreateModuleControl()
        {
            var moduleTemplatePath = Server.MapPath(ModulePath) + "Templates\\" + optLanguage.SelectedValue + "\\" + cboTemplate.SelectedValue + "\\";

            EventLogController.Instance.AddLog("Processing Template Folder", moduleTemplatePath, PortalSettings, -1, EventLogController.EventLogType.HOST_ALERT);


            var controlName = Null.NullString;
            var fileName = Null.NullString;
            var modulePath = "";
            var sourceCode = Null.NullString;

            //iterate through files in template folder
            string[] fileList = Directory.GetFiles(moduleTemplatePath);
            foreach (string filePath in fileList)
            {
                modulePath = Server.MapPath("DesktopModules/" + GetFolderName() + "/");

                //open file
                TextReader tr = new StreamReader(filePath);
                sourceCode = tr.ReadToEnd();
                tr.Close();

                //replace tokens
                sourceCode = sourceCode.Replace("_OWNER_", GetOwner());
                sourceCode = sourceCode.Replace("_MODULE_", GetModule());
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

            return controlName;
        }

        private string GetOwner()
        {
            return txtOwner.Text.Replace(" ", "");
        }

        private string GetModule()
        {
            return txtModule.Text.Replace(" ", "");
        }

        private string GetControl()
        {
            return txtControl.Text.Replace(" ", "");
        }

        private string GetFolderName()
        {
            var strFolder = Null.NullString;
            strFolder += txtOwner.Text + "/" + txtModule.Text;
            //return folder and remove any spaces that might appear in folder structure
            return strFolder.Replace(" ", "");
        }

        private string GetClassName()
        {
            var strClass = Null.NullString;
            strClass += txtOwner.Text + "." + txtModule.Text;
            //return class and remove any spaces that might appear in class name
            return strClass.Replace(" ", "");
        }


        /// <summary>
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        private bool CreateModuleDefinition()
        {
            try
            {
                if (PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == GetClassName()) == null)
                {
                    var controlName = Null.NullString;

                    //Create module folder
                    CreateModuleFolder();

                    //Create module control
                    controlName = CreateModuleControl();
                    if (controlName != "")
                    {
                        //Create package
                        var objPackage = new PackageInfo();
                        objPackage.Name = GetClassName();
                        objPackage.FriendlyName = txtModule.Text;
                        objPackage.Description = txtDescription.Text;
                        objPackage.Version = new Version(1, 0, 0);
                        objPackage.PackageType = "Module";
                        objPackage.License = "";
                        objPackage.Owner = txtOwner.Text;
                        objPackage.Organization = txtOwner.Text;
                        objPackage.FolderName = "DesktopModules/" + GetFolderName();
                        objPackage.License = "The license for this package is not currently included within the installation file, please check with the vendor for full license details.";
                        objPackage.ReleaseNotes = "This package has no Release Notes.";
                        PackageController.Instance.SaveExtensionPackage(objPackage);

                        //Create desktopmodule
                        var objDesktopModules = new DesktopModuleController();
                        var objDesktopModule = new DesktopModuleInfo();
                        objDesktopModule.DesktopModuleID = Null.NullInteger;
                        objDesktopModule.ModuleName = GetClassName();
                        objDesktopModule.FolderName = GetFolderName();
                        objDesktopModule.FriendlyName = txtModule.Text;
                        objDesktopModule.Description = txtDescription.Text;
                        objDesktopModule.IsPremium = false;
                        objDesktopModule.IsAdmin = false;
                        objDesktopModule.Version = "01.00.00";
                        objDesktopModule.BusinessControllerClass = "";
                        objDesktopModule.CompatibleVersions = "";
                        objDesktopModule.Dependencies = "";
                        objDesktopModule.Permissions = "";
                        objDesktopModule.PackageID = objPackage.PackageID;
                        objDesktopModule.DesktopModuleID = objDesktopModules.AddDesktopModule(objDesktopModule);
                        objDesktopModule = objDesktopModules.GetDesktopModule(objDesktopModule.DesktopModuleID);

                        //Add OwnerName to the DesktopModule taxonomy and associate it with this module
                        var vocabularyId = -1;
                        var termId = -1;
                        var objTermController = DotNetNuke.Entities.Content.Common.Util.GetTermController();
                        var objTerms = objTermController.GetTermsByVocabulary("Module_Categories");
                        foreach (Term term in objTerms)
                        {
                            vocabularyId = term.VocabularyId;
                            if (term.Name == txtOwner.Text)
                            {
                                termId = term.TermId;
                            }
                        }
                        if (termId == -1)
                        {
                            termId = objTermController.AddTerm(new Term(vocabularyId) { Name = txtOwner.Text });
                        }
                        var objTerm = objTermController.GetTerm(termId);
                        var objContentController = DotNetNuke.Entities.Content.Common.Util.GetContentController();
                        var objContent = objContentController.GetContentItem(objDesktopModule.ContentItemId);
                        objTermController.AddTermToContent(objTerm, objContent);

                        //Add desktopmodule to all portals
                        DesktopModuleController.AddDesktopModuleToPortals(objDesktopModule.DesktopModuleID);

                        //Create module definition
                        var objModuleDefinition = new ModuleDefinitionInfo();
                        objModuleDefinition.ModuleDefID = Null.NullInteger;
                        objModuleDefinition.DesktopModuleID = objDesktopModule.DesktopModuleID;
                        // need core enhancement to have a unique DefinitionName  
                        objModuleDefinition.FriendlyName = GetClassName();
                        //objModuleDefinition.FriendlyName = txtModule.Text;
                        //objModuleDefinition.DefinitionName = GetClassName();
                        objModuleDefinition.DefaultCacheTime = 0;
                        objModuleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(objModuleDefinition, false, true);

                        //Create modulecontrol
                        var objModuleControl = new ModuleControlInfo();
                        objModuleControl.ModuleControlID = Null.NullInteger;
                        objModuleControl.ModuleDefID = objModuleDefinition.ModuleDefID;
                        objModuleControl.ControlKey = "";
                        objModuleControl.ControlSrc = "DesktopModules/" + GetFolderName() + "/" + controlName;
                        objModuleControl.ControlTitle = "";
                        objModuleControl.ControlType = SecurityAccessLevel.View;
                        objModuleControl.HelpURL = "";
                        objModuleControl.IconFile = "";
                        objModuleControl.ViewOrder = 0;
                        objModuleControl.SupportsPartialRendering = false;
                        objModuleControl.SupportsPopUps = false;
                        ModuleControlController.AddModuleControl(objModuleControl);

                        //Update current module to reference new moduledefinition
                        var objModule = ModuleController.Instance.GetModule(ModuleId, TabId, false);
                        objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                        objModule.ModuleTitle = txtModule.Text;

                        //HACK - need core enhancement to be able to update ModuleDefID
                        DotNetNuke.Data.DataProvider.Instance().ExecuteSQL("Update dbo.Modules set ModuleDefID = " + objModule.ModuleDefID.ToString() + " where ModuleID = " + ModuleId.ToString());

                        ModuleController.Instance.UpdateModule(objModule);

                        return true;
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TemplateProblem.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                        return false;
                    }
                }
                else
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AlreadyExists.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
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

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            optLanguage.SelectedIndexChanged += optLanguage_SelectedIndexChanged;
            cboTemplate.SelectedIndexChanged += cboTemplate_SelectedIndexChanged;
            cmdCreate.Click += cmdCreate_Click;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (UserInfo.IsSuperUser)
            {
                if (!Page.IsPostBack)
                {
                    Dictionary<string, string> HostSettings = HostController.Instance.GetSettingsDictionary();
                    if (HostSettings.ContainsKey("Owner"))
                    {
                        txtOwner.Text = HostSettings["Owner"];
                    }
                    LoadLanguages();
                    LoadModuleTemplates();
                    txtControl.Text = "View";
                }
            }
            else
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SuperUser.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                createForm.Visible = false;
            }
        }

        protected void optLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadModuleTemplates();
        }

        protected void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReadMe();
        }

        protected void cmdCreate_Click(object sender, EventArgs e)
        {
            if (UserInfo.IsSuperUser)
            {
                if (!String.IsNullOrEmpty(txtOwner.Text) && !String.IsNullOrEmpty(txtModule.Text) && cboTemplate.SelectedIndex > 0 && !String.IsNullOrEmpty(txtControl.Text))
                {
                    HostController.Instance.Update("Owner", txtOwner.Text, false);
                    if (CreateModuleDefinition())
                    {
                        Response.Redirect(Globals.NavigateURL(), true);
                    }
                }
                else
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InputValidation.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                }
            }
        }

        #endregion

    }
}