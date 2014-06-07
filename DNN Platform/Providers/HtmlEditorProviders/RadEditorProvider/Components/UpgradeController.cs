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

using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

using System;

using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Upgrade;

namespace DotNetNuke.Providers.RadEditorProvider
{

	public class UpgradeController : IUpgradeable
	{
		private const string ModuleFolder = "~/DesktopModules/Admin/RadEditorProvider";
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Version"></param>
		/// <returns></returns>
		/// <remarks>This is not localizing Page Name or description.</remarks>
		public string UpgradeModule(string Version)
		{
			try
			{
				switch (Version)
				{
					case "06.00.00":
						string resourceFile = ModuleFolder + "/App_LocalResources/ProviderConfig.ascx.resx";
						string pageName = Localization.GetString("HTMLEditorPageName", resourceFile);
						string pageDescription = Localization.GetString("HTMLEditorPageDescription", resourceFile);

						//Create Rad Editor Config Page (or get existing one)
						TabInfo newPage = Upgrade.AddHostPage(pageName, pageDescription, ModuleFolder + "/images/radeditor_config_small.png",ModuleFolder + "/images/radeditor_config_large.png", true);

						//Add Module To Page
						int moduleDefId = GetModuleDefinitionID();
						Upgrade.AddModuleToPage(newPage, moduleDefId, pageName, ModuleFolder + "/images/radeditor_config_large.png", true);

						foreach (var item in DesktopModuleController.GetDesktopModules(Null.NullInteger))
						{
							DesktopModuleInfo moduleInfo = item.Value;

							if (moduleInfo.ModuleName == "DotNetNuke.RadEditorProvider")
							{
								moduleInfo.Category = "Host";
								DesktopModuleController.SaveDesktopModule(moduleInfo, false, false);
							}
						}
						break;
					case "07.00.06":
						UpdateConfigOfLinksType();
						break;
                    case "07.03.00":
				        UpdateConfigFilesName();
                        UpdateToolsFilesName();
                        break;
				}
			}
			catch (Exception ex)
			{
				ExceptionLogController xlc = new ExceptionLogController();
				xlc.AddLog(ex);

				return "Failed";
			}

			return "Success";
		}

		private int GetModuleDefinitionID()
		{
			// get desktop module
			DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DotNetNuke.RadEditorProvider", Null.NullInteger);
			if (desktopModule == null)
			{
				return -1;
			}

			//get module definition
			ModuleDefinitionInfo moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("RadEditor Manager", desktopModule.DesktopModuleID);
			if (moduleDefinition == null)
			{
				return -1;
			}

			return moduleDefinition.ModuleDefID;
		}

		private void UpdateConfigOfLinksType()
		{
			foreach (string file in Directory.GetFiles(HttpContext.Current.Server.MapPath(ModuleFolder + "/ConfigFile")))
			{
				var filename = Path.GetFileName(file).ToLowerInvariant();
				if (filename.StartsWith("configfile") && filename.EndsWith(".xml"))
				{
					UpdateConfigOfLinksTypeInFile(file);
				}
			}
		}

		private void UpdateConfigOfLinksTypeInFile(string file)
		{
			try
			{
				var config = new XmlDocument();
				config.Load(file);
				var node = config.SelectSingleNode("/configuration/property[@name='LinksUseTabNames']");
				if (node != null)
				{
					var value = bool.Parse(node.InnerText);
					config.DocumentElement.RemoveChild(node);
					var newNode = config.CreateElement("property");
					newNode.SetAttribute("name", "LinksType");
					newNode.InnerText = value ? "UseTabName" : "Normal";
					config.DocumentElement.AppendChild(newNode);
					config.Save(file);
				}
			}
			catch
			{
				//ignore error here.
			}
		}

	    private void UpdateConfigFilesName()
	    {
            foreach (string file in Directory.GetFiles(HttpContext.Current.Server.MapPath(ModuleFolder + "/ConfigFile")))
            {
                var filename = Path.GetFileName(file).ToLowerInvariant();
                if (filename.StartsWith("configfile") && filename.EndsWith(".xml"))
                {
                    UpdateFileNameWithRoleId(file);
                }
            }
	    }

        private void UpdateToolsFilesName()
        {
            foreach (string file in Directory.GetFiles(HttpContext.Current.Server.MapPath(ModuleFolder + "/ToolsFile")))
            {
                var filename = Path.GetFileName(file).ToLowerInvariant();
                if (filename.StartsWith("toolsfile") && filename.EndsWith(".xml"))
                {
                    UpdateFileNameWithRoleId(file);
                }
            }
        }

	    private void UpdateFileNameWithRoleId(string file)
	    {
	        var newPath = file;
            if(file.ToLowerInvariant().Contains(".host"))
            {
                var rolePart = ".RoleId." + Globals.glbRoleSuperUser;
                newPath = Regex.Replace(file, "\\.host", rolePart, RegexOptions.IgnoreCase);
            }
            else if (file.ToLowerInvariant().Contains(".admin"))
            {
                var portalSettings = new PortalSettings(Host.HostPortalID);
                var rolePart = ".RoleId." + portalSettings.AdministratorRoleId;
                newPath = Regex.Replace(file, "\\.admin", rolePart, RegexOptions.IgnoreCase);
            }
            else if (file.ToLowerInvariant().Contains(".registered"))
            {
                var portalSettings = new PortalSettings(Host.HostPortalID);
                var rolePart = ".RoleId." + portalSettings.RegisteredRoleId;
                newPath = Regex.Replace(file, "\\.registered", rolePart, RegexOptions.IgnoreCase);
            }

	        if (newPath != file)
	        {
	            File.Move(file, newPath);
	        }
	    }
	}

}