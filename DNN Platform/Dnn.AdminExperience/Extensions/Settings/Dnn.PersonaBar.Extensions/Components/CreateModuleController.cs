using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components
{
    public class CreateModuleController : ServiceLocator<ICreateModuleController, CreateModuleController>, ICreateModuleController
    {
        protected override Func<ICreateModuleController> GetFactory()
        {
            return () => new CreateModuleController();
        }

        /// <summary>
        /// create new module.
        /// </summary>
        /// <param name="createModuleDto"></param>
        /// <param name="newPageUrl"></param>
        /// <param name="errorMessage"></param>
        /// <returns>return the new package id.</returns>
        public int CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
            errorMessage = string.Empty;
            newPageUrl = string.Empty;
            var packageId = Null.NullInteger;
            switch (createModuleDto.Type)
            {
                case CreateModuleType.New:
                    packageId = CreateNewModule(createModuleDto, out newPageUrl, out errorMessage);
                    break;
                case CreateModuleType.Control:
                    packageId = CreateModuleFromControl(createModuleDto, out newPageUrl, out errorMessage);
                    break;
                case CreateModuleType.Manifest:
                    packageId = CreateModuleFromManifest(createModuleDto, out newPageUrl, out errorMessage);
                    break;
            }

            return packageId;
        }

        private int CreateNewModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                errorMessage = "NoModuleFolder";
                return Null.NullInteger;
            }

            if (string.IsNullOrEmpty(createModuleDto.Language))
            {
                errorMessage = "LanguageError";
                return Null.NullInteger;
            }

            //remove spaces so file is created correctly
            var controlSrc = createModuleDto.FileName.Replace(" ", "");
            if (InvalidFilename(controlSrc))
            {
                errorMessage = "InvalidFilename";
                return Null.NullInteger;
            }

            if (String.IsNullOrEmpty(controlSrc))
            {
                errorMessage = "MissingControl";
                return Null.NullInteger;
            }
            if (String.IsNullOrEmpty(createModuleDto.ModuleName))
            {
                errorMessage = "MissingFriendlyname";
                return Null.NullInteger;
            }
            if (!controlSrc.EndsWith(".ascx"))
            {
                controlSrc += ".ascx";
            }

            var uniqueName = true;
            foreach (var package in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
            {
                if (package.Name.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase) 
                    || package.FriendlyName.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    uniqueName = false;
                    break;
                }
            }

            if (!uniqueName)
            {
                errorMessage = "NonuniqueName";
                return Null.NullInteger;
            }
            //First create the control
            createModuleDto.FileName = controlSrc;
            var message = CreateControl(createModuleDto);
            if (string.IsNullOrEmpty(message))
            {
                //Next import the control
                return CreateModuleFromControl(createModuleDto, out newPageUrl, out errorMessage);
            }

            return Null.NullInteger;
        }

        private int CreateModuleFromControl(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.FileName))
            {
                errorMessage = "NoControl";
                return Null.NullInteger;
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
                var friendlyName = createModuleDto.ModuleName;
                var name = createModuleDto.ModuleName;
                var moduleControl = "DesktopModules/" + folder + "/" + createModuleDto.FileName;

                var packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                                    p.Name.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase)
                                     || p.FriendlyName.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase));
                if (packageInfo != null)
                {
                    errorMessage = "NonuniqueName";
                    return Null.NullInteger;
                }

                var package = new PackageInfo
                {
                    Name = name,
                    FriendlyName = friendlyName,
                    Description = createModuleDto.Description,
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
                    Description = createModuleDto.Description,
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
                var moduleDefinition = new ModuleDefinitionInfo();

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

                if (createModuleDto.AddPage)
                {
                    newPageUrl = CreateNewPage(moduleDefinition);
                }

                return package.PackageID;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                errorMessage = "CreateModuleFailed";
                return Null.NullInteger;
            }
        }

        private int CreateModuleFromManifest(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.Manifest))
            {
                errorMessage = "MissingManifest";
                return Null.NullInteger;
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
                var manifest = Path.Combine(Globals.ApplicationMapPath, "~/DesktopModules/" + folder + "/" + createModuleDto.Manifest);
                var installer = new Installer(manifest, Globals.ApplicationMapPath, true);

                if (installer.IsValid)
                {
                    installer.InstallerInfo.Log.Logs.Clear();
                    installer.Install();

                    if (installer.IsValid)
                    {
                        if (createModuleDto.AddPage)
                        {
                            var desktopModule =
                                DesktopModuleController.GetDesktopModuleByPackageID(installer.InstallerInfo.PackageID);
                            if (desktopModule != null && desktopModule.ModuleDefinitions.Count > 0)
                            {
                                foreach (var kvp in desktopModule.ModuleDefinitions)
                                {
                                    var moduleDefinition = kvp.Value;

                                    newPageUrl = CreateNewPage(moduleDefinition);
                                    break;
                                }
                            }
                        }

                        return installer.InstallerInfo.PackageID;
                    }
                    else
                    {
                        errorMessage = "InstallError";
                        return Null.NullInteger;
                    }
                }
                else
                {
                    errorMessage = "InstallError";
                    return Null.NullInteger;
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                errorMessage = "CreateModuleFailed";
                return Null.NullInteger;
            }
        }

        private string CreateNewPage(ModuleDefinitionInfo moduleDefinition)
        {
            if (PortalSettings.Current == null)
            {
                return string.Empty;
            }

            var portalId = PortalSettings.Current.PortalId;
            var tabName = "Test " + moduleDefinition.FriendlyName + " Page";
            var tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
            var tabId = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
            if (tabId == Null.NullInteger)
            {
                //Create a new page
                var newTab = new TabInfo();
                newTab.TabName = tabName;
                newTab.ParentId = Null.NullInteger;
                newTab.PortalID = portalId;
                newTab.IsVisible = true;
                newTab.TabID = TabController.Instance.AddTabBefore(newTab, PortalSettings.Current.AdminTabId);
                var objModule = new ModuleInfo();
                objModule.Initialize(portalId);
                objModule.PortalID = portalId;
                objModule.TabID = newTab.TabID;
                objModule.ModuleOrder = Null.NullInteger;
                objModule.ModuleTitle = moduleDefinition.FriendlyName;
                objModule.PaneName = Globals.glbDefaultPane;
                objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                objModule.InheritViewPermissions = true;
                objModule.AllTabs = false;
                ModuleController.Instance.AddModule(objModule);

                return Globals.NavigateURL(newTab.TabID);
            }

            return string.Empty;
        }

        private static bool InvalidFilename(string fileName)
        {
            var invalidFilenameChars = RegexUtils.GetCachedRegex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
            return invalidFilenameChars.IsMatch(fileName);
        }

        private string CreateControl(CreateModuleDto createModuleDto)
        {
            var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
            var className = GetClassName(createModuleDto);
            var moduleControlPath = Path.Combine(Globals.ApplicationMapPath, "DesktopModules/" + folder + "/" + createModuleDto.FileName);
            var message = Null.NullString;

            var source = string.Format(LoadControlTemplate(), createModuleDto.Language, className);

            //reset attributes
            if (File.Exists(moduleControlPath))
            {
                message = "FileExists";
            }
            else
            {
                using (var stream = File.CreateText(moduleControlPath))
                {
                    stream.WriteLine(source);
                }
            }
            return message;
        }

        private string LoadControlTemplate()
        {
            var personaBarFolder = Library.Constants.PersonaBarRelativePath.Replace("~/", "");
            var filePath = Path.Combine(Globals.ApplicationMapPath, personaBarFolder, "Modules/Dnn.Extensions/data/ModuleControlTemplate.resources");
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        private string GetSourceFolder(CreateModuleDto createModuleDto)
        {
            var folder = Null.NullString;
            if (!string.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                folder += createModuleDto.OwnerFolder + "/";
            }
            if (!string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                folder += createModuleDto.ModuleFolder + "/";
            }
            return folder;
        }

        private string GetClassName(CreateModuleDto createModuleDto)
        {
            var className = Null.NullString;
            if (!String.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                className += createModuleDto.OwnerFolder + ".";
            }
            if (!String.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                className += createModuleDto.ModuleFolder;
            }
            //return class and remove any spaces that might appear in folder structure
            return className.Replace(" ", "");
        }
    }
}