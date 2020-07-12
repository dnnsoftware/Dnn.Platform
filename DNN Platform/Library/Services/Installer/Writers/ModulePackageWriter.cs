// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ModulePackageWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ModulePackageWriter : PackageWriterBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePackageWriter));

        public ModulePackageWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            this.DesktopModule = new DesktopModuleInfo();

            // Create a Package
            this.Package = new PackageInfo(installer);

            this.ReadLegacyManifest(manifestNav, true);

            this.Package.Name = this.DesktopModule.ModuleName;
            this.Package.FriendlyName = this.DesktopModule.FriendlyName;
            this.Package.Description = this.DesktopModule.Description;
            if (!string.IsNullOrEmpty(this.DesktopModule.Version))
            {
                this.Package.Version = new Version(this.DesktopModule.Version);
            }

            this.Package.PackageType = "Module";

            LegacyUtil.ParsePackageName(this.Package);

            this.Initialize(this.DesktopModule.FolderName);
        }

        public ModulePackageWriter(DesktopModuleInfo desktopModule, XPathNavigator manifestNav, PackageInfo package)
            : base(package)
        {
            this.DesktopModule = desktopModule;

            this.Initialize(desktopModule.FolderName);
            if (manifestNav != null)
            {
                this.ReadLegacyManifest(manifestNav.SelectSingleNode("folders/folder"), false);
            }

            string physicalFolderPath = Path.Combine(Globals.ApplicationMapPath, this.BasePath);
            this.ProcessModuleFolders(physicalFolderPath, physicalFolderPath);
        }

        public ModulePackageWriter(PackageInfo package)
            : base(package)
        {
            this.DesktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);
            this.Initialize(this.DesktopModule.FolderName);
        }

        public ModulePackageWriter(DesktopModuleInfo desktopModule, PackageInfo package)
            : base(package)
        {
            this.DesktopModule = desktopModule;
            this.Initialize(desktopModule.FolderName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the associated Desktop Module.
        /// </summary>
        /// <value>A DesktopModuleInfo object.</value>
        /// -----------------------------------------------------------------------------
        public DesktopModuleInfo DesktopModule { get; set; }

        protected override Dictionary<string, string> Dependencies
        {
            get
            {
                var dependencies = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(this.DesktopModule.Dependencies))
                {
                    dependencies["type"] = this.DesktopModule.Dependencies;
                }

                if (!string.IsNullOrEmpty(this.DesktopModule.Permissions))
                {
                    dependencies["permission"] = this.DesktopModule.Permissions;
                }

                return dependencies;
            }
        }

        protected override void WriteManifestComponent(XmlWriter writer)
        {
            // Write Module Component
            this.WriteModuleComponent(writer);
        }

        private static void ProcessControls(XPathNavigator controlNav, string moduleFolder, ModuleDefinitionInfo definition)
        {
            var moduleControl = new ModuleControlInfo();

            moduleControl.ControlKey = Util.ReadElement(controlNav, "key");
            moduleControl.ControlTitle = Util.ReadElement(controlNav, "title");

            // Write controlSrc
            string controlSrc = Util.ReadElement(controlNav, "src");
            if (!(controlSrc.StartsWith("desktopmodules", StringComparison.InvariantCultureIgnoreCase) || !controlSrc.EndsWith(".ascx", StringComparison.InvariantCultureIgnoreCase)))
            {
                // this code allows a developer to reference an ASCX file in a different folder than the module folder ( good for ASCX files shared between modules where you want only a single copy )
                // or it allows the developer to use webcontrols rather than usercontrols
                controlSrc = Path.Combine("DesktopModules", Path.Combine(moduleFolder, controlSrc));
            }

            controlSrc = controlSrc.Replace('\\', '/');
            moduleControl.ControlSrc = controlSrc;

            moduleControl.IconFile = Util.ReadElement(controlNav, "iconfile");

            string controlType = Util.ReadElement(controlNav, "type");
            if (!string.IsNullOrEmpty(controlType))
            {
                try
                {
                    moduleControl.ControlType = (SecurityAccessLevel)TypeDescriptor.GetConverter(typeof(SecurityAccessLevel)).ConvertFromString(controlType);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    throw new Exception(Util.EXCEPTION_Type);
                }
            }

            string viewOrder = Util.ReadElement(controlNav, "vieworder");
            if (!string.IsNullOrEmpty(viewOrder))
            {
                moduleControl.ViewOrder = int.Parse(viewOrder);
            }

            moduleControl.HelpURL = Util.ReadElement(controlNav, "helpurl");
            string supportsPartialRendering = Util.ReadElement(controlNav, "supportspartialrendering");
            if (!string.IsNullOrEmpty(supportsPartialRendering))
            {
                moduleControl.SupportsPartialRendering = bool.Parse(supportsPartialRendering);
            }

            string supportsPopUps = Util.ReadElement(controlNav, "supportspopups");
            if (!string.IsNullOrEmpty(supportsPopUps))
            {
                moduleControl.SupportsPartialRendering = bool.Parse(supportsPopUps);
            }

            definition.ModuleControls[moduleControl.ControlKey] = moduleControl;
        }

        private void Initialize(string folder)
        {
            this.BasePath = Path.Combine("DesktopModules", folder).Replace("/", "\\");
            this.AppCodePath = Path.Combine("App_Code", folder).Replace("/", "\\");
            this.AssemblyPath = "bin";
        }

        private void ProcessModuleFiles(string folder, string basePath)
        {
            // we are going to drill down through the folders to add the files
            foreach (string fileName in Directory.GetFiles(folder))
            {
                string name = fileName.Replace(basePath + "\\", string.Empty);
                this.AddFile(name, name);
            }
        }

        private void ProcessModuleFolders(string folder, string basePath)
        {
            // Process Folders recursively
            foreach (string directoryName in Directory.GetDirectories(folder))
            {
                this.ProcessModuleFolders(directoryName, basePath);
            }

            // process files
            this.ProcessModuleFiles(folder, basePath);
        }

        private void ProcessModules(XPathNavigator moduleNav, string moduleFolder)
        {
            var definition = new ModuleDefinitionInfo();

            definition.FriendlyName = Util.ReadElement(moduleNav, "friendlyname");
            string cacheTime = Util.ReadElement(moduleNav, "cachetime");
            if (!string.IsNullOrEmpty(cacheTime))
            {
                definition.DefaultCacheTime = int.Parse(cacheTime);
            }

            // Process legacy controls Node
            foreach (XPathNavigator controlNav in moduleNav.Select("controls/control"))
            {
                ProcessControls(controlNav, moduleFolder, definition);
            }

            this.DesktopModule.ModuleDefinitions[definition.FriendlyName] = definition;
        }

        private void ReadLegacyManifest(XPathNavigator folderNav, bool processModule)
        {
            if (processModule)
            {
                // Version 2 of legacy manifest
                string name = Util.ReadElement(folderNav, "name");
                this.DesktopModule.FolderName = name;
                this.DesktopModule.ModuleName = name;
                this.DesktopModule.FriendlyName = name;
                string folderName = Util.ReadElement(folderNav, "foldername");
                if (!string.IsNullOrEmpty(folderName))
                {
                    this.DesktopModule.FolderName = folderName;
                }

                if (string.IsNullOrEmpty(this.DesktopModule.FolderName))
                {
                    this.DesktopModule.FolderName = "MyModule";
                }

                string friendlyname = Util.ReadElement(folderNav, "friendlyname");
                if (!string.IsNullOrEmpty(friendlyname))
                {
                    this.DesktopModule.FriendlyName = friendlyname;
                    this.DesktopModule.ModuleName = friendlyname;
                }

                string iconFile = Util.ReadElement(folderNav, "iconfile");
                if (!string.IsNullOrEmpty(iconFile))
                {
                    this.Package.IconFile = iconFile;
                }

                string modulename = Util.ReadElement(folderNav, "modulename");
                if (!string.IsNullOrEmpty(modulename))
                {
                    this.DesktopModule.ModuleName = modulename;
                }

                string permissions = Util.ReadElement(folderNav, "permissions");
                if (!string.IsNullOrEmpty(permissions))
                {
                    this.DesktopModule.Permissions = permissions;
                }

                string dependencies = Util.ReadElement(folderNav, "dependencies");
                if (!string.IsNullOrEmpty(dependencies))
                {
                    this.DesktopModule.Dependencies = dependencies;
                }

                this.DesktopModule.Version = Util.ReadElement(folderNav, "version", "01.00.00");
                this.DesktopModule.Description = Util.ReadElement(folderNav, "description");
                this.DesktopModule.BusinessControllerClass = Util.ReadElement(folderNav, "businesscontrollerclass");

                // Process legacy modules Node
                foreach (XPathNavigator moduleNav in folderNav.Select("modules/module"))
                {
                    this.ProcessModules(moduleNav, this.DesktopModule.FolderName);
                }
            }

            // Process legacy files Node
            foreach (XPathNavigator fileNav in folderNav.Select("files/file"))
            {
                string fileName = Util.ReadElement(fileNav, "name");
                string filePath = Util.ReadElement(fileNav, "path");

                // In Legacy Modules the file can be physically located in the Root of the zip folder - or in the path/file location
                // First test the folder
                string sourceFileName;
                if (filePath.Contains("[app_code]"))
                {
                    // Special case for App_code - files can be in App_Code\ModuleName or root
                    sourceFileName = Path.Combine(filePath, fileName).Replace("[app_code]", "App_Code\\" + this.DesktopModule.FolderName);
                }
                else
                {
                    sourceFileName = Path.Combine(filePath, fileName);
                }

                string tempFolder = this.Package.InstallerInfo.TempInstallFolder;
                if (!File.Exists(Path.Combine(tempFolder, sourceFileName)))
                {
                    sourceFileName = fileName;
                }

                // In Legacy Modules the assembly is always in "bin" - ignore the path element
                if (fileName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.AddFile("bin/" + fileName, sourceFileName);
                }
                else
                {
                    this.AddFile(Path.Combine(filePath, fileName), sourceFileName);
                }
            }

            // Process resource file Node
            if (!string.IsNullOrEmpty(Util.ReadElement(folderNav, "resourcefile")))
            {
                this.AddResourceFile(new InstallFile(Util.ReadElement(folderNav, "resourcefile"), this.Package.InstallerInfo));
            }
        }

        private void WriteEventMessage(XmlWriter writer)
        {
            // Start Start eventMessage
            writer.WriteStartElement("eventMessage");

            // Write Processor Type
            writer.WriteElementString("processorType", "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke");

            // Write Processor Type
            writer.WriteElementString("processorCommand", "UpgradeModule");

            // Write Event Message Attributes
            writer.WriteStartElement("attributes");

            // Write businessControllerClass Attribute
            writer.WriteElementString("businessControllerClass", this.DesktopModule.BusinessControllerClass);

            // Write businessControllerClass Attribute
            writer.WriteElementString("desktopModuleID", "[DESKTOPMODULEID]");

            // Write upgradeVersionsList Attribute
            string upgradeVersions = Null.NullString;
            this.Versions.Sort();
            foreach (string version in this.Versions)
            {
                upgradeVersions += version + ",";
            }

            if (upgradeVersions.Length > 1)
            {
                upgradeVersions = upgradeVersions.Remove(upgradeVersions.Length - 1, 1);
            }

            writer.WriteElementString("upgradeVersionsList", upgradeVersions);

            // End end of Event Message Attribues
            writer.WriteEndElement();

            // End component Element
            writer.WriteEndElement();
        }

        private void WriteModuleComponent(XmlWriter writer)
        {
            // Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "Module");

            // Write Module Manifest
            if (this.AppCodeFiles.Count > 0)
            {
                this.DesktopModule.CodeSubDirectory = this.DesktopModule.FolderName;
            }

            CBO.SerializeObject(this.DesktopModule, writer);

            // Write EventMessage
            if (!string.IsNullOrEmpty(this.DesktopModule.BusinessControllerClass))
            {
                this.WriteEventMessage(writer);
            }

            // End component Element
            writer.WriteEndElement();
        }
    }
}
