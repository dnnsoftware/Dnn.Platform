// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the logic for the CreateModule view.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class CreateModule : ModuleUserControlBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CreateModule));
        private readonly INavigationManager navigationManager;

        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

        /// <summary>Initializes a new instance of the <see cref="CreateModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public CreateModule()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CreateModule"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public CreateModule(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager ?? Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>Gets the module control file name without it's extension.</summary>
        protected string ModuleControl
        {
            get
            {
                return Path.GetFileNameWithoutExtension(this.scriptList.SelectedValue).TrimStart('_') + ".ascx";
            }
        }

        /// <summary>Gets the razor script path.</summary>
        protected string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = Null.NullString;
                var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
                if (!string.IsNullOrEmpty(scriptFileSetting))
                {
                    m_RazorScriptFile = string.Format(this.razorScriptFileFormatString, scriptFileSetting);
                }

                return m_RazorScriptFile;
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cmdCancel.Click += this.CmdCancel_Click;
            this.cmdCreate.Click += this.CmdCreate_Click;
            this.scriptList.SelectedIndexChanged += this.ScriptList_SelectedIndexChanged;
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                this.Response.Redirect(this.navigationManager.NavigateURL("Access Denied"), true);
            }

            if (!this.Page.IsPostBack)
            {
                this.LoadScripts();
                this.DisplayFile();
            }
        }

        private void Create()
        {
            // Create new Folder
            string folderMapPath = this.Server.MapPath(string.Format("~/DesktopModules/RazorModules/{0}", this.txtFolder.Text));
            if (Directory.Exists(folderMapPath))
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("FolderExists", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }
            else
            {
                // Create folder
                Directory.CreateDirectory(folderMapPath);
            }

            // Create new Module Control
            string moduleControlMapPath = folderMapPath + "/" + this.ModuleControl;
            try
            {
                using (var moduleControlWriter = new StreamWriter(moduleControlMapPath))
                {
                    moduleControlWriter.Write(Localization.GetString("ModuleControlText.Text", this.LocalResourceFile));
                    moduleControlWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ModuleControlCreationError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // Copy Script to new Folder
            string scriptSourceFile = this.Server.MapPath(string.Format(this.razorScriptFileFormatString, this.scriptList.SelectedValue));
            string scriptTargetFile = folderMapPath + "/" + this.scriptList.SelectedValue;
            try
            {
                File.Copy(scriptSourceFile, scriptTargetFile);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ScriptCopyError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // Create new Manifest in target folder
            string manifestMapPath = folderMapPath + "/" + this.ModuleControl.Replace(".ascx", ".dnn");
            try
            {
                using (var manifestWriter = new StreamWriter(manifestMapPath))
                {
                    string manifestTemplate = Localization.GetString("ManifestText.Text", this.LocalResourceFile);
                    string manifest = string.Format(manifestTemplate, this.txtName.Text, this.txtDescription.Text, this.txtFolder.Text, this.ModuleControl, this.scriptList.SelectedValue);
                    manifestWriter.Write(manifest);
                    manifestWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ManifestCreationError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // Register Module
            ModuleDefinitionInfo moduleDefinition = this.ImportManifest(manifestMapPath);

            // remove the manifest file
            try
            {
                FileWrapper.Instance.Delete(manifestMapPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            // Optionally goto new Page
            if (this.chkAddPage.Checked)
            {
                string tabName = "Test " + this.txtName.Text + " Page";
                string tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
                int tabID = TabController.GetTabByTabPath(this.ModuleContext.PortalId, tabPath, this.ModuleContext.PortalSettings.CultureCode);

                if (tabID == Null.NullInteger)
                {
                    // Create a new page
                    var newTab = new TabInfo();
                    newTab.TabName = "Test " + this.txtName.Text + " Page";
                    newTab.ParentId = Null.NullInteger;
                    newTab.PortalID = this.ModuleContext.PortalId;
                    newTab.IsVisible = true;
                    newTab.TabID = TabController.Instance.AddTabBefore(newTab, this.ModuleContext.PortalSettings.AdminTabId);

                    var objModule = new ModuleInfo();
                    objModule.Initialize(this.ModuleContext.PortalId);

                    objModule.PortalID = this.ModuleContext.PortalId;
                    objModule.TabID = newTab.TabID;
                    objModule.ModuleOrder = Null.NullInteger;
                    objModule.ModuleTitle = moduleDefinition.FriendlyName;
                    objModule.PaneName = Globals.glbDefaultPane;
                    objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                    objModule.InheritViewPermissions = true;
                    objModule.AllTabs = false;
                    ModuleController.Instance.AddModule(objModule);

                    this.Response.Redirect(this.navigationManager.NavigateURL(newTab.TabID), true);
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabExists", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                // Redirect to main extensions page
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
        }

        private ModuleDefinitionInfo ImportManifest(string manifest)
        {
            ModuleDefinitionInfo moduleDefinition = null;
            try
            {
                var installer = new Installer(manifest, this.Request.MapPath("."), true);

                if (installer.IsValid)
                {
                    // Reset Log
                    installer.InstallerInfo.Log.Logs.Clear();

                    // Install
                    installer.Install();

                    if (installer.IsValid)
                    {
                        DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(installer.InstallerInfo.PackageID);
                        if (desktopModule != null && desktopModule.ModuleDefinitions.Count > 0)
                        {
                            foreach (KeyValuePair<string, ModuleDefinitionInfo> kvp in desktopModule.ModuleDefinitions)
                            {
                                moduleDefinition = kvp.Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InstallError.Text", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        this.phInstallLogs.Controls.Add(installer.InstallerInfo.Log.GetLogsTable());
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InstallError.Text", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    this.phInstallLogs.Controls.Add(installer.InstallerInfo.Log.GetLogsTable());
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ImportControl.ErrorMessage", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }

            return moduleDefinition;
        }

        private void LoadScripts()
        {
            string basePath = this.Server.MapPath(this.razorScriptFolder);
            var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;

            foreach (string script in Directory.GetFiles(this.Server.MapPath(this.razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, string.Empty);
                var item = new ListItem(scriptPath, scriptPath);
                if (!string.IsNullOrEmpty(scriptFileSetting) && string.Equals(scriptPath, scriptFileSetting, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                }

                this.scriptList.Items.Add(item);
            }
        }

        private void DisplayFile()
        {
            string scriptFile = string.Format(this.razorScriptFileFormatString, this.scriptList.SelectedValue);

            this.lblSourceFile.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("SourceFile", this.LocalResourceFile), scriptFile);
            this.lblModuleControl.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("SourceControl", this.LocalResourceFile), this.ModuleControl);
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void CmdCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    this.Response.Redirect(this.navigationManager.NavigateURL("Access Denied"), true);
                }

                if (this.Page.IsValid)
                {
                    this.Create();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ScriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DisplayFile();
        }
    }
}
