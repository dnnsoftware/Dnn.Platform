#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class CreateModule : ModuleUserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CreateModule));

        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected string ModuleControl
        {
            get
            {
                return Path.GetFileNameWithoutExtension(scriptList.SelectedValue).TrimStart('_') + ".ascx";
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = Null.NullString;
                var scriptFileSetting = ModuleContext.Settings["ScriptFile"] as string;
                if (! (string.IsNullOrEmpty(scriptFileSetting)))
                {
                    m_RazorScriptFile = string.Format(razorScriptFileFormatString, scriptFileSetting);
                }
                return m_RazorScriptFile;
            }
        }

        private void Create()
        {
            //Create new Folder
            string folderMapPath = Server.MapPath(string.Format("~/DesktopModules/RazorModules/{0}", txtFolder.Text));
            if (Directory.Exists(folderMapPath))
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("FolderExists", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }
            else
            {
                //Create folder
                Directory.CreateDirectory(folderMapPath);
            }

            //Create new Module Control
            string moduleControlMapPath = folderMapPath + "/" + ModuleControl;
            try
            {
                using (var moduleControlWriter = new StreamWriter(moduleControlMapPath))
                {
                    moduleControlWriter.Write(Localization.GetString("ModuleControlText.Text", LocalResourceFile));
                    moduleControlWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ModuleControlCreationError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            //Copy Script to new Folder
            string scriptSourceFile = Server.MapPath(string.Format(razorScriptFileFormatString, scriptList.SelectedValue));
            string scriptTargetFile = folderMapPath + "/" + scriptList.SelectedValue;
            try
            {
                File.Copy(scriptSourceFile, scriptTargetFile);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ScriptCopyError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            //Create new Manifest in target folder
			string manifestMapPath = folderMapPath + "/" + ModuleControl.Replace(".ascx", ".dnn");
			try
			{
				using (var manifestWriter = new StreamWriter(manifestMapPath))
				{
					string manifestTemplate = Localization.GetString("ManifestText.Text", LocalResourceFile);
					string manifest = string.Format(manifestTemplate, txtName.Text, txtDescription.Text, txtFolder.Text, ModuleControl, scriptList.SelectedValue);
					manifestWriter.Write(manifest);
					manifestWriter.Flush();
				}
			}
			catch (Exception ex)
			{
				Exceptions.LogException(ex);
				UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ManifestCreationError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				return;
			}

            //Register Module
            ModuleDefinitionInfo moduleDefinition = ImportManifest(manifestMapPath);

			//remove the manifest file
	        try
	        {
				FileWrapper.Instance.Delete(manifestMapPath);
	        }
	        catch (Exception ex)
	        {
				Logger.Error(ex);
	        }
			

            //Optionally goto new Page
            if (chkAddPage.Checked)
            {
                string tabName = "Test " + txtName.Text + " Page";
                string tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
                int tabID = TabController.GetTabByTabPath(ModuleContext.PortalId, tabPath, ModuleContext.PortalSettings.CultureCode);

                if (tabID == Null.NullInteger)
                {
                    //Create a new page
                    var newTab = new TabInfo();
                    newTab.TabName = "Test " + txtName.Text + " Page";
                    newTab.ParentId = Null.NullInteger;
                    newTab.PortalID = ModuleContext.PortalId;
                    newTab.IsVisible = true;
                    newTab.TabID = TabController.Instance.AddTabBefore(newTab, ModuleContext.PortalSettings.AdminTabId);

                    var objModule = new ModuleInfo();
                    objModule.Initialize(ModuleContext.PortalId);

                    objModule.PortalID = ModuleContext.PortalId;
                    objModule.TabID = newTab.TabID;
                    objModule.ModuleOrder = Null.NullInteger;
                    objModule.ModuleTitle = moduleDefinition.FriendlyName;
                    objModule.PaneName = Globals.glbDefaultPane;
                    objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                    objModule.InheritViewPermissions = true;
                    objModule.AllTabs = false;
                    ModuleController.Instance.AddModule(objModule);

                    Response.Redirect(Globals.NavigateURL(newTab.TabID), true);
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabExists", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                //Redirect to main extensions page
                Response.Redirect(Globals.NavigateURL(), true);
            }
        }

        private ModuleDefinitionInfo ImportManifest(string manifest)
        {
            ModuleDefinitionInfo moduleDefinition = null;
            try
            {
                var _Installer = new Installer(manifest, Request.MapPath("."), true);

                if (_Installer.IsValid)
                {
                    //Reset Log
                    _Installer.InstallerInfo.Log.Logs.Clear();

                    //Install
                    _Installer.Install();

                    if (_Installer.IsValid)
                    {
                        DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(_Installer.InstallerInfo.PackageID);
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
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InstallError.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        phInstallLogs.Controls.Add(_Installer.InstallerInfo.Log.GetLogsTable());
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InstallError.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    phInstallLogs.Controls.Add(_Installer.InstallerInfo.Log.GetLogsTable());
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ImportControl.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }

            return moduleDefinition;
        }

        private void LoadScripts()
        {
            string basePath = Server.MapPath(razorScriptFolder);
            var scriptFileSetting = ModuleContext.Settings["ScriptFile"] as string;

            foreach (string script in Directory.GetFiles(Server.MapPath(razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, "");
                var item = new ListItem(scriptPath, scriptPath);
                if (! (string.IsNullOrEmpty(scriptFileSetting)) && scriptPath.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant())
                {
                    item.Selected = true;
                }
                scriptList.Items.Add(item);
            }
        }

        private void DisplayFile()
        {
            string scriptFile = string.Format(razorScriptFileFormatString, scriptList.SelectedValue);

            lblSourceFile.Text = string.Format(Localization.GetString("SourceFile", LocalResourceFile), scriptFile);
            lblModuleControl.Text = string.Format(Localization.GetString("SourceControl", LocalResourceFile), ModuleControl);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdCreate.Click += cmdCreate_Click;
            scriptList.SelectedIndexChanged += scriptList_SelectedIndexChanged;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (! ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }

            if (! Page.IsPostBack)
            {
                LoadScripts();
                DisplayFile();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (! ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    Response.Redirect(Globals.NavigateURL("Access Denied"), true);
                }

                if (Page.IsValid)
                {
                    Create();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void scriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayFile();
        }
    }
}