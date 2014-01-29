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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.ModuleDefinitions
{

	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// </history>
	public partial class CreateModuleDefinition : ModuleUserControlBase
	{

		#region Private Members

		private PackageInfo _Package;

		#endregion

		#region Protected Properties

		protected bool IsAddMode
		{
			get
			{
				return (PackageID == Null.NullInteger);
			}
		}

		protected PackageInfo Package
		{
			get
			{
				return _Package ?? (_Package = PackageID == Null.NullInteger ? new PackageInfo() : PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID));
			}
		}

		public int PackageID
		{
			get
			{
				var packageID = Null.NullInteger;
				if ((Request.QueryString["PackageID"] != null))
				{
					packageID = Int32.Parse(Request.QueryString["PackageID"]);
				}
				return packageID;
			}
		}

		#endregion

		#region Private Methods

		private static void AddFolder(string parentFolder, string newFolder)
		{
			var parentFolderPath = Globals.ApplicationMapPath + "\\DesktopModules";
			if (!string.IsNullOrEmpty(parentFolder))
			{
				parentFolderPath += "\\" + parentFolder;
			}
			var dinfo = new DirectoryInfo(parentFolderPath);
			var dinfoNew = new DirectoryInfo(parentFolderPath + "\\" + newFolder);
			if (!dinfoNew.Exists)
			{
				dinfo.CreateSubdirectory(newFolder);
			}
		}

		private static bool InvalidFilename(string fileName)
		{
			var invalidFilenameChars = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
			return invalidFilenameChars.IsMatch(fileName);
		}

		private string CreateControl(string controlSrc)
		{
			var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder());
			var className = GetClassName();
			var moduleControlPath = Server.MapPath("DesktopModules/" + folder + "/" + controlSrc);
			var message = Null.NullString;

			var source = string.Format(Localization.GetString("ModuleControlTemplate", LocalResourceFile), rblLanguage.SelectedValue, className);

			//reset attributes
			if (File.Exists(moduleControlPath))
			{
				message = Localization.GetString("FileExists", LocalResourceFile);
			}
			else
			{
				StreamWriter objStream;
				objStream = File.CreateText(moduleControlPath);
				objStream.WriteLine(source);
				objStream.Close();
			}
			return message;
		}

		private string GetClassName()
		{
			var strClass = Null.NullString;
			if (!String.IsNullOrEmpty(cboOwner.SelectedValue))
			{
				strClass += cboOwner.SelectedValue + ".";
			}
			if (!String.IsNullOrEmpty(cboModule.SelectedValue))
			{
				strClass += cboModule.SelectedValue;
			}
			//return class and remove any spaces that might appear in folder structure
			return strClass.Replace(" ", "");
		}

		private string GetSourceFolder()
		{
			var strFolder = Null.NullString;
			if (!String.IsNullOrEmpty(cboOwner.SelectedValue))
			{
				strFolder += cboOwner.SelectedValue + "/";
			}
			if (!String.IsNullOrEmpty(cboModule.SelectedValue))
			{
				strFolder += cboModule.SelectedValue + "/";
			}
			return strFolder;
		}

		/// <summary>
		/// </summary>
		/// <remarks>
		/// Loads the cboSource control list with locations of controls.
		/// </remarks>
		/// <history>
		/// </history>
		private ModuleDefinitionInfo ImportControl(string controlSrc)
		{
			ModuleDefinitionInfo moduleDefinition = null;
			try
			{
				string folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder());
				string friendlyName = txtName.Text;
				string name = GetClassName();
				string moduleControl = "DesktopModules/" + folder + "/" + controlSrc;

                var packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == name || p.FriendlyName == friendlyName);
                if (packageInfo != null)
                {
                    UI.Skins.Skin.AddModuleMessage(this, String.Format(Localization.GetString("NonuniqueNameModule", LocalResourceFile), packageInfo.FriendlyName), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    var package = new PackageInfo
                        {
                            Name = name,
                            FriendlyName = friendlyName,
                            Description = txtDescription.Text,
                            Version = new Version(1, 0, 0),
                            PackageType = "Module",
                            License = Util.PACKAGE_NoLicense
                        };

                    //Save Package
                    PackageController.Instance.SaveExtensionPackage(package);

                    var objDesktopModule = new DesktopModuleInfo
                        {
                            DesktopModuleID = Null.NullInteger,
                            ModuleName = name,
                            FolderName = folder,
                            FriendlyName = friendlyName,
                            Description = txtDescription.Text,
                            IsPremium = false,
                            IsAdmin = false,
                            Version = "01.00.00",
                            BusinessControllerClass = "",
                            CompatibleVersions = "",
                            Dependencies = "",
                            Permissions = "",
                            PackageID = package.PackageID
                        };

                    objDesktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(objDesktopModule, false, true);

                    //Add module to all portals
                    DesktopModuleController.AddDesktopModuleToPortals(objDesktopModule.DesktopModuleID);

                    //Save module definition
                    moduleDefinition = new ModuleDefinitionInfo();

                    moduleDefinition.ModuleDefID = Null.NullInteger;
                    moduleDefinition.DesktopModuleID = objDesktopModule.DesktopModuleID;
                    moduleDefinition.FriendlyName = friendlyName;
                    moduleDefinition.DefaultCacheTime = 0;

                    moduleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);

                    //Save module control
                    var objModuleControl = new ModuleControlInfo();

                    objModuleControl.ModuleControlID = Null.NullInteger;
                    objModuleControl.ModuleDefID = moduleDefinition.ModuleDefID;
                    objModuleControl.ControlKey = "";
                    objModuleControl.ControlSrc = moduleControl;
                    objModuleControl.ControlTitle = "";
                    objModuleControl.ControlType = SecurityAccessLevel.View;
                    objModuleControl.HelpURL = "";
                    objModuleControl.IconFile = "";
                    objModuleControl.ViewOrder = 0;
                    objModuleControl.SupportsPartialRendering = false;

                    ModuleControlController.AddModuleControl(objModuleControl);
                }
			}
			catch (Exception exc)
			{
				Exceptions.LogException(exc);
				UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ImportControl.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			return moduleDefinition;
		}

		private ModuleDefinitionInfo ImportManifest()
		{
			ModuleDefinitionInfo moduleDefinition = null;
			try
			{
				var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder());
				var manifest = Server.MapPath("~/DesktopModules/" + folder + "/" + cboFile.SelectedValue);
				var installer = new Installer(manifest, Request.MapPath("."), true);

				if (installer.IsValid)
				{
					installer.InstallerInfo.Log.Logs.Clear();
					installer.Install();

					if (installer.IsValid)
					{
						var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(installer.InstallerInfo.PackageID);
						if (desktopModule != null && desktopModule.ModuleDefinitions.Count > 0)
						{
							foreach (var kvp in desktopModule.ModuleDefinitions)
							{
								moduleDefinition = kvp.Value;
								break; // TODO: might not be correct. Was : Exit For
							}
						}
					}
					else
					{
						UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InstallError.Text", LocalResourceFile), UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
						phInstallLogs.Controls.Add(installer.InstallerInfo.Log.GetLogsTable());
					}
				}
				else
				{
					UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InstallError.Text", LocalResourceFile), UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
					phInstallLogs.Controls.Add(installer.InstallerInfo.Log.GetLogsTable());
				}
			}
			catch (Exception exc)
			{
				Exceptions.LogException(exc);
				UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ImportControl.ErrorMessage", LocalResourceFile), UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
			}

			return moduleDefinition;
		}

		private void LoadFiles(string extensions)
		{
			LoadFiles(extensions, String.Empty);
		}

		private void LoadFiles(string extensions, string folder)
		{
			if (String.IsNullOrEmpty(folder))
			{
				folder = Server.MapPath("~/DesktopModules/" + GetSourceFolder());
			}
			var files = Directory.GetFiles(folder);
			foreach (var file in files)
			{
				string extension = Path.GetExtension(file);
				if (extension != null && extensions.Contains(extension))
				{
					//cboFile.Items.Add(Path.GetFileName(file));
                    var fileName = Path.GetFileName(file);
                    cboFile.AddItem(fileName, fileName); 
				}
			}
		}

		private void LoadModuleFolders(string selectedValue)
		{
			cboModule.Items.Clear();
			var arrFolders = Directory.GetDirectories(Globals.ApplicationMapPath + "\\DesktopModules\\" + cboOwner.SelectedValue);
			foreach (var strFolder in arrFolders)
			{
				var path = strFolder.Replace(Path.GetDirectoryName(strFolder) + "\\", "");
				var item = new DnnComboBoxItem(path, path);
				if (item.Value == selectedValue)
				{
					item.Selected = true;
				}
				cboModule.Items.Add(item);
			}
			//cboModule.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", ""));
            cboModule.InsertItem(0, "<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", "");
		}

		private void LoadOwnerFolders(string selectedValue)
		{
			cboOwner.Items.Clear();
			var arrFolders = Directory.GetDirectories(Globals.ApplicationMapPath + "\\DesktopModules\\");
			foreach (var strFolder in arrFolders)
			{
				var files = Directory.GetFiles(strFolder, "*.ascx");
				//exclude module folders
				if (files.Length == 0 || strFolder.ToLower() == "admin")
				{
					var path = strFolder.Replace(Path.GetDirectoryName(strFolder) + "\\", "");
					var item = new DnnComboBoxItem(path, path);
					if (item.Value == selectedValue)
					{
						item.Selected = true;
					}
					cboOwner.Items.Add(item);
				}
			}
			//cboOwner.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", ""));
            cboOwner.InsertItem(0, "<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", "");
		}

		private void SetupModuleFolders()
		{
			cboFile.Items.Clear();

			switch (cboCreate.SelectedValue)
			{
				case "Control":
					LoadFiles(".ascx");
					LoadFiles(".cshtml");
					LoadFiles(".vbhtml");
					break;
				case "Template":
					LoadFiles(".module.template", Globals.HostMapPath + "Templates\\");
					break;
				case "Manifest":
					LoadFiles(".dnn,.dnn5");
					break;
			}
		}

		private void SetupOwnerFolders()
		{
			LoadModuleFolders(Null.NullString);
			SetupModuleFolders();
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Page_Init runs when the control is initialised
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[cnurse]	03/01/2006
		/// </history>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			cboCreate.SelectedIndexChanged += cboCreate_SelectedIndexChanged;
			cboModule.SelectedIndexChanged += cboModule_SelectedIndexChanged;
			cboOwner.SelectedIndexChanged += cboOwner_SelectedIndexChanged;
			cmdAddModule.Click += cmdAddModule_Click;
			cmdAddOwner.Click += cmdAddOwner_Click;
			cmdCancel.Click += cmdCancel_Click;
			cmdCancelModule.Click += cmdCancelModule_Click;
			cmdCancelOwner.Click += cmdCancelOwner_Click;
			cmdCreate.Click += cmdCreate_Click;
			cmdSaveModule.Click += cmdSaveModule_Click;
			cmdSaveOwner.Click += cmdSaveOwner_Click;

			ModuleContext.Configuration.ModuleTitle = Localization.GetString(IsAddMode ? "Add.Title" : "Edit.Title", LocalResourceFile);
		}

		/// <summary>
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// </history>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				if (!Page.IsPostBack)
				{
					//cboCreate.Items.Insert(0, new ListItem("<" + Localization.GetString("None_Specified") + ">", ""));
                    cboCreate.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "");
					LoadOwnerFolders(Null.NullString);
					LoadModuleFolders(Null.NullString);
				}
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void cboCreate_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadOwnerFolders(Null.NullString);
			LoadModuleFolders(Null.NullString);
			cboFile.Items.Clear();
			txtModule.Text = "";
			txtDescription.Text = "";

			switch (cboCreate.SelectedValue)
			{
				case "":
					rowOwner1.Visible = false;
					cmdAddOwner.Visible = false;
					rowModule1.Visible = false;
					cmdAddModule.Visible = false;
					rowFile1.Visible = false;
					rowFile2.Visible = false;
					rowLang.Visible = false;
					rowName.Visible = false;
					rowDescription.Visible = false;
					rowSource.Visible = false;
					rowAddPage.Visible = false;
					cmdCreate.Visible = false;
					break;
				case "New":
					rowOwner1.Visible = true;
					cmdAddOwner.Visible = true;
					rowModule1.Visible = true;
					cmdAddModule.Visible = true;
					rowFile1.Visible = false;
					rowFile2.Visible = true;
					rowLang.Visible = true;
					rowName.Visible = true;
					rowDescription.Visible = true;
					rowSource.Visible = false;
					rowAddPage.Visible = true;
					cmdCreate.Visible = true;
					break;
				case "Control":
					rowOwner1.Visible = true;
					cmdAddOwner.Visible = false;
					rowModule1.Visible = true;
					cmdAddModule.Visible = false;
					rowFile1.Visible = true;
					rowFile2.Visible = false;
					rowLang.Visible = false;
					rowName.Visible = true;
					rowDescription.Visible = true;
					rowSource.Visible = false;
					rowAddPage.Visible = true;
					cmdCreate.Visible = true;
					break;
				case "Template":
					rowOwner1.Visible = true;
					cmdAddOwner.Visible = true;
					rowModule1.Visible = true;
					cmdAddModule.Visible = true;
					rowFile1.Visible = true;
					rowFile2.Visible = false;
					rowLang.Visible = false;
					rowName.Visible = true;
					rowDescription.Visible = true;
					rowSource.Visible = false;
					rowAddPage.Visible = false;
					cmdCreate.Visible = true;
					break;
				case "Manifest":
					rowOwner1.Visible = true;
					cmdAddOwner.Visible = false;
					rowModule1.Visible = true;
					cmdAddModule.Visible = false;
					rowFile1.Visible = true;
					rowFile2.Visible = false;
					rowLang.Visible = false;
					rowName.Visible = false;
					rowDescription.Visible = false;
					rowSource.Visible = false;
					rowAddPage.Visible = true;
					cmdCreate.Visible = true;
					break;
			}
		}

		protected void cboModule_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupModuleFolders();
		}

		protected void cboOwner_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupOwnerFolders();
		}

		protected void cmdAddModule_Click(object sender, EventArgs e)
		{
			rowModule1.Visible = false;
			rowModule2.Visible = true;
		}

		protected void cmdAddOwner_Click(object sender, EventArgs e)
		{
			rowOwner1.Visible = false;
			rowOwner2.Visible = true;
		}

		protected void cmdCancel_Click(object sender, EventArgs e)
		{
			try
			{
				Response.Redirect(Globals.NavigateURL(), true);
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void cmdCancelModule_Click(object sender, EventArgs e)
		{
			rowModule1.Visible = true;
			rowModule2.Visible = false;
		}

		protected void cmdCancelOwner_Click(object sender, EventArgs e)
		{
			rowOwner1.Visible = true;
			rowOwner2.Visible = false;
		}

		protected void cmdCreate_Click(object sender, EventArgs e)
		{
			try
			{
				ModuleDefinitionInfo moduleDefinition = null;
				string strMessage = Null.NullString;
				switch (cboCreate.SelectedValue)
				{
					case "":
						break;
					case "New":
						if (String.IsNullOrEmpty(cboModule.SelectedValue))
						{
							strMessage = Localization.GetString("ModuleFolder", LocalResourceFile);
							break;
						}

						if (String.IsNullOrEmpty(rblLanguage.SelectedValue))
						{
							strMessage = Localization.GetString("LanguageError", LocalResourceFile);
							break;
						}

						//remove spaces so file is created correctly
						var controlSrc = txtFile.Text.Replace(" ", "");
						if (InvalidFilename(controlSrc))
						{
							strMessage = Localization.GetString("InvalidFilename", LocalResourceFile);
							break;
						}

						if (String.IsNullOrEmpty(controlSrc))
						{
							strMessage = Localization.GetString("MissingControl", LocalResourceFile);
							break;
						}
						if (String.IsNullOrEmpty(txtName.Text))
						{
							strMessage = Localization.GetString("MissingFriendlyname", LocalResourceFile);
							break;
						}
						if (!controlSrc.EndsWith(".ascx"))
						{
							controlSrc += ".ascx";
						}

						var uniqueName = true;
						var packages = new List<PackageInfo>();
						foreach (var package in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
						{
							if (package.Name == txtName.Text || package.FriendlyName == txtName.Text)
							{
								uniqueName = false;
								break;
							}
						}

						if (!uniqueName)
						{
							strMessage = Localization.GetString("NonuniqueName", LocalResourceFile);
							break;
						}
						//First create the control
						strMessage = CreateControl(controlSrc);
						if (String.IsNullOrEmpty(strMessage))
						{
							//Next import the control
							moduleDefinition = ImportControl(controlSrc);
						}
						break;
					case "Control":
						if (!String.IsNullOrEmpty(cboFile.SelectedValue))
						{
							moduleDefinition = ImportControl(cboFile.SelectedValue);
						}
						else
						{
							strMessage = Localization.GetString("NoControl", LocalResourceFile);
						}
						break;
					case "Template":
						break;
					case "Manifest":
						if (String.IsNullOrEmpty(cboFile.SelectedValue))
						{
							strMessage = Localization.GetString("MissingManifest", LocalResourceFile);
						}
						else
						{
							moduleDefinition = ImportManifest();
						}
						break;
				}
				if (moduleDefinition == null)
				{
					UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
				}
				else
				{
					if (!string.IsNullOrEmpty(cboCreate.SelectedValue) && chkAddPage.Checked)
					{
						var tabName = "Test " + txtName.Text + " Page";
						var tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
						var tabID = TabController.GetTabByTabPath(ModuleContext.PortalId, tabPath, Null.NullString);
						if (tabID == Null.NullInteger)
						{
							//Create a new page
							var newTab = new TabInfo();
							newTab.TabName = "Test " + txtName.Text + " Page";
							newTab.ParentId = Null.NullInteger;
							newTab.PortalID = ModuleContext.PortalId;
							newTab.IsVisible = true;
							newTab.TabID = new TabController().AddTabBefore(newTab, ModuleContext.PortalSettings.AdminTabId);
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
							var moduleCtl = new ModuleController();
							moduleCtl.AddModule(objModule);
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
			}
			catch (Exception ex)
			{
				Exceptions.ProcessModuleLoadException(this, ex);
			}
		}

		protected void cmdSaveModule_Click(object sender, EventArgs e)
		{
			AddFolder(cboOwner.SelectedValue, txtModule.Text);
			LoadModuleFolders(txtModule.Text);
			SetupModuleFolders();
			rowModule1.Visible = true;
			rowModule2.Visible = false;
		}

		protected void cmdSaveOwner_Click(object sender, EventArgs e)
		{
			AddFolder("", txtOwner.Text);
			LoadOwnerFolders(txtOwner.Text);
			SetupOwnerFolders();
			rowOwner1.Visible = true;
			rowOwner2.Visible = false;
		}
		
		#endregion

	}
}