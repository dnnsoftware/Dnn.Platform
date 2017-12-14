#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ModulePackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ModulePackageWriter : PackageWriterBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModulePackageWriter));
		#region "Constructors"

        public ModulePackageWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            DesktopModule = new DesktopModuleInfo();

            //Create a Package
            Package = new PackageInfo(installer);

            ReadLegacyManifest(manifestNav, true);

            Package.Name = DesktopModule.ModuleName;
            Package.FriendlyName = DesktopModule.FriendlyName;
            Package.Description = DesktopModule.Description;
            if (!string.IsNullOrEmpty(DesktopModule.Version))
            {
                Package.Version = new Version(DesktopModule.Version);
            }

            Package.PackageType = "Module";

            LegacyUtil.ParsePackageName(Package);

            Initialize(DesktopModule.FolderName);
        }

        public ModulePackageWriter(DesktopModuleInfo desktopModule, XPathNavigator manifestNav, PackageInfo package) : base(package)
        {
            DesktopModule = desktopModule;

            Initialize(desktopModule.FolderName);
            if (manifestNav != null)
            {
                ReadLegacyManifest(manifestNav.SelectSingleNode("folders/folder"), false);
            }
            string physicalFolderPath = Path.Combine(Globals.ApplicationMapPath, BasePath);
            ProcessModuleFolders(physicalFolderPath, physicalFolderPath);
        }

        public ModulePackageWriter(PackageInfo package) : base(package)
        {
            DesktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);
            Initialize(DesktopModule.FolderName);
        }

        public ModulePackageWriter(DesktopModuleInfo desktopModule, PackageInfo package) : base(package)
        {
            DesktopModule = desktopModule;
            Initialize(desktopModule.FolderName);
        }
		
		#endregion

		#region "Protected Properties"

        protected override Dictionary<string, string> Dependencies
        {
            get
            {
                var dependencies = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(DesktopModule.Dependencies))
                {
                    dependencies["type"] = DesktopModule.Dependencies;
                }
                if (!string.IsNullOrEmpty(DesktopModule.Permissions))
                {
                    dependencies["permission"] = DesktopModule.Permissions;
                }
                return dependencies;
            }
        }
		
		#endregion

		#region "Public Properties"


		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the associated Desktop Module
		/// </summary>
		/// <value>A DesktopModuleInfo object</value>
		/// -----------------------------------------------------------------------------
        public DesktopModuleInfo DesktopModule { get; set; }

		#endregion

		#region "Private Methods"

        private void Initialize(string folder)
        {
            BasePath = Path.Combine("DesktopModules", folder).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", folder).Replace("/", "\\");
            AssemblyPath = "bin";
        }

        private static void ProcessControls(XPathNavigator controlNav, string moduleFolder, ModuleDefinitionInfo definition)
        {
            var moduleControl = new ModuleControlInfo();

            moduleControl.ControlKey = Util.ReadElement(controlNav, "key");
            moduleControl.ControlTitle = Util.ReadElement(controlNav, "title");

            //Write controlSrc
            string controlSrc = Util.ReadElement(controlNav, "src");
            if (!(controlSrc.ToLower().StartsWith("desktopmodules") || !controlSrc.ToLower().EndsWith(".ascx")))
            {
				//this code allows a developer to reference an ASCX file in a different folder than the module folder ( good for ASCX files shared between modules where you want only a single copy )
                //or it allows the developer to use webcontrols rather than usercontrols

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
                    moduleControl.ControlType = (SecurityAccessLevel) TypeDescriptor.GetConverter(typeof (SecurityAccessLevel)).ConvertFromString(controlType);
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

        private void ProcessModuleFiles(string folder, string basePath)
        {
			//we are going to drill down through the folders to add the files
            foreach (string fileName in Directory.GetFiles(folder))
            {
                string name = fileName.Replace(basePath + "\\", "");
                AddFile(name, name);
            }
        }

        private void ProcessModuleFolders(string folder, string basePath)
        {
			//Process Folders recursively
            foreach (string directoryName in Directory.GetDirectories(folder))
            {
                ProcessModuleFolders(directoryName, basePath);
            }
			
            //process files
            ProcessModuleFiles(folder, basePath);
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
			
            //Process legacy controls Node
            foreach (XPathNavigator controlNav in moduleNav.Select("controls/control"))
            {
                ProcessControls(controlNav, moduleFolder, definition);
            }
            DesktopModule.ModuleDefinitions[definition.FriendlyName] = definition;
        }

        private void ReadLegacyManifest(XPathNavigator folderNav, bool processModule)
        {
            if (processModule)
            {
				//Version 2 of legacy manifest
                string name = Util.ReadElement(folderNav, "name");
                DesktopModule.FolderName = name;
                DesktopModule.ModuleName = name;
                DesktopModule.FriendlyName = name;
                string folderName = Util.ReadElement(folderNav, "foldername");
                if (!string.IsNullOrEmpty(folderName))
                {
                    DesktopModule.FolderName = folderName;
                }
                if (string.IsNullOrEmpty(DesktopModule.FolderName))
                {
                    DesktopModule.FolderName = "MyModule";
                }
                string friendlyname = Util.ReadElement(folderNav, "friendlyname");
                if (!string.IsNullOrEmpty(friendlyname))
                {
                    DesktopModule.FriendlyName = friendlyname;
                    DesktopModule.ModuleName = friendlyname;
                }
                string iconFile = Util.ReadElement(folderNav, "iconfile");
                if (!string.IsNullOrEmpty(iconFile))
                {
                    Package.IconFile = iconFile;
                }
                string modulename = Util.ReadElement(folderNav, "modulename");
                if (!string.IsNullOrEmpty(modulename))
                {
                    DesktopModule.ModuleName = modulename;
                }
                string permissions = Util.ReadElement(folderNav, "permissions");
                if (!string.IsNullOrEmpty(permissions))
                {
                    DesktopModule.Permissions = permissions;
                }
                string dependencies = Util.ReadElement(folderNav, "dependencies");
                if (!string.IsNullOrEmpty(dependencies))
                {
                    DesktopModule.Dependencies = dependencies;
                }
                DesktopModule.Version = Util.ReadElement(folderNav, "version", "01.00.00");
                DesktopModule.Description = Util.ReadElement(folderNav, "description");
                DesktopModule.BusinessControllerClass = Util.ReadElement(folderNav, "businesscontrollerclass");

                //Process legacy modules Node
                foreach (XPathNavigator moduleNav in folderNav.Select("modules/module"))
                {
                    ProcessModules(moduleNav, DesktopModule.FolderName);
                }
            }
			
            //Process legacy files Node
            foreach (XPathNavigator fileNav in folderNav.Select("files/file"))
            {
                string fileName = Util.ReadElement(fileNav, "name");
                string filePath = Util.ReadElement(fileNav, "path");

                //In Legacy Modules the file can be physically located in the Root of the zip folder - or in the path/file location
                //First test the folder
                string sourceFileName;
                if (filePath.Contains("[app_code]"))
                {
					//Special case for App_code - files can be in App_Code\ModuleName or root
                    sourceFileName = Path.Combine(filePath, fileName).Replace("[app_code]", "App_Code\\" + DesktopModule.FolderName);
                }
                else
                {
                    sourceFileName = Path.Combine(filePath, fileName);
                }
                string tempFolder = Package.InstallerInfo.TempInstallFolder;
                if (!File.Exists(Path.Combine(tempFolder, sourceFileName)))
                {
                    sourceFileName = fileName;
                }
				
				//In Legacy Modules the assembly is always in "bin" - ignore the path element
                if (fileName.ToLower().EndsWith(".dll"))
                {
                    AddFile("bin/" + fileName, sourceFileName);
                }
                else
                {
                    AddFile(Path.Combine(filePath, fileName), sourceFileName);
                }
            }
			
            //Process resource file Node
            if (!string.IsNullOrEmpty(Util.ReadElement(folderNav, "resourcefile")))
            {
                AddResourceFile(new InstallFile(Util.ReadElement(folderNav, "resourcefile"), Package.InstallerInfo));
            }
        }

        private void WriteEventMessage(XmlWriter writer)
        {
			//Start Start eventMessage
            writer.WriteStartElement("eventMessage");

            //Write Processor Type
            writer.WriteElementString("processorType", "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke");

            //Write Processor Type
            writer.WriteElementString("processorCommand", "UpgradeModule");

            //Write Event Message Attributes
            writer.WriteStartElement("attributes");

            //Write businessControllerClass Attribute
            writer.WriteElementString("businessControllerClass", DesktopModule.BusinessControllerClass);

            //Write businessControllerClass Attribute
            writer.WriteElementString("desktopModuleID", "[DESKTOPMODULEID]");

            //Write upgradeVersionsList Attribute
            string upgradeVersions = Null.NullString;
            Versions.Sort();
            foreach (string version in Versions)
            {
                upgradeVersions += version + ",";
            }
            if (upgradeVersions.Length > 1)
            {
                upgradeVersions = upgradeVersions.Remove(upgradeVersions.Length - 1, 1);
            }
            writer.WriteElementString("upgradeVersionsList", upgradeVersions);

            //End end of Event Message Attribues
            writer.WriteEndElement();

            //End component Element
            writer.WriteEndElement();
        }

        private void WriteModuleComponent(XmlWriter writer)
        {
			//Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "Module");

            //Write Module Manifest
            if (AppCodeFiles.Count > 0)
            {
                DesktopModule.CodeSubDirectory = DesktopModule.FolderName;
            }
            CBO.SerializeObject(DesktopModule, writer);

            //Write EventMessage
            if (!string.IsNullOrEmpty(DesktopModule.BusinessControllerClass))
            {
                WriteEventMessage(writer);
            }
			
            //End component Element
            writer.WriteEndElement();
        }
		
		#endregion

		#region "Protected Methods"

        protected override void WriteManifestComponent(XmlWriter writer)
        {
			//Write Module Component
            WriteModuleComponent(writer);
        }
		
		#endregion
    }
}