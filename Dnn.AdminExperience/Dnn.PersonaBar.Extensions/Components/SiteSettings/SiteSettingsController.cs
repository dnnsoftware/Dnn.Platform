﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Localization;

    using Constants = Dnn.PersonaBar.Library.Constants;

    public class SiteSettingsController
    {
        private Dictionary<string, InstallFile> _Files;
        private string _Manifest = Null.NullString;

        protected string BasePath
        {
            get
            {
                return HttpContext.Current.Server.MapPath("~/Install/Language");
            }
        }

        public void SaveLocalizedKeys(int portalId, string propertyName, string propertyCategory, string cultureCode, string propertyNameString,
            string propertyHelpString, string propertyRequiredString, string propertyValidationString, string categoryNameString)
        {
            var portalResources = new XmlDocument { XmlResolver = null };
            var defaultResources = new XmlDocument { XmlResolver = null };
            XmlNode parent;

            defaultResources.Load(this.GetResourceFile("", Localization.SystemLocale, portalId));
            string filename = this.GetResourceFile("Portal", cultureCode, portalId);

            if (File.Exists(filename))
            {
                portalResources.Load(filename);
            }
            else
            {
                portalResources.Load(this.GetResourceFile("", Localization.SystemLocale, portalId));
            }
            this.UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Text", propertyNameString);
            this.UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Help", propertyHelpString);
            this.UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Required", propertyRequiredString);
            this.UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Validation", propertyValidationString);
            this.UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyCategory + ".Header", categoryNameString);

            //remove unmodified keys
            foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
            {
                XmlNode defaultNode = defaultResources.SelectSingleNode("//root/data[@name='" + node.Attributes["name"].Value + "']");
                if (defaultNode != null && defaultNode.InnerXml == node.InnerXml)
                {
                    parent = node.ParentNode;
                    parent.RemoveChild(node);
                }
            }

            //remove duplicate keys
            foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
            {
                if (portalResources.SelectNodes("//root/data[@name='" + node.Attributes["name"].Value + "']").Count > 1)
                {
                    parent = node.ParentNode;
                    parent.RemoveChild(node);
                }
            }
            if (portalResources.SelectNodes("//root/data").Count > 0)
            {
                //there's something to save
                portalResources.Save(filename);
            }
            else
            {
                //nothing to be saved, if file exists delete
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
        }

        public IList<string> GetAvailableAnalyzers()
        {
            var analyzers = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    analyzers.AddRange(from t in assembly.GetTypes() where this.IsAnalyzerType(t) && this.IsAllowType(t) select string.Format("{0}, {1}", t.FullName, assembly.GetName().Name));
                }
                catch (Exception)
                {
                    //do nothing but just ignore the error.
                }
            }
            return analyzers;
        }

        public bool CreateAuthSystemPackage(string cultureCode, PackageInfo authPackage, bool createZip)
        {
            var package = new PackageInfo
            {
                Name = authPackage.Name,
                FriendlyName = authPackage.FriendlyName,
                Version = authPackage.Version,
                License = Util.PACKAGE_NoLicense
            };
            var fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
            var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(authPackage.PackageID);
            var authPath = authSystem.LoginControlSrc.Substring(0, authSystem.LoginControlSrc.LastIndexOf("/", StringComparison.Ordinal));
            return this.CreatePackage(cultureCode, package, authPackage.PackageID, authPath.Replace("/", "\\"), fileName, createZip);
        }

        public bool CreateCorePackage(string cultureCode, string fileName, bool createZip)
        {
            var package = new PackageInfo { Name = Globals.CleanFileName(fileName) };
            package.FriendlyName = package.Name;
            package.Version = DotNetNukeContext.Current.Application.Version;
            package.License = Util.PACKAGE_NoLicense;

            fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);

            return this.CreatePackage(cultureCode, package, -2, "", fileName, createZip);
        }

        public void CreateFullPackage(string cultureCode, string fileName)
        {
            Locale language = LocaleController.Instance.GetLocale(cultureCode);
            var package = new PackageInfo
            {
                Name = Globals.CleanFileName(fileName),
                Version = DotNetNukeContext.Current.Application.Version,
                License = Util.PACKAGE_NoLicense,
                PackageType = "CoreLanguagePack"
            };

            this._Files = new Dictionary<string, InstallFile>();
            this.CreateCorePackage(cultureCode, fileName, false);
            foreach (var desktopModule in DesktopModuleController.GetDesktopModules(Null.NullInteger).Values.Where(desktopModule => !desktopModule.FolderName.StartsWith("Admin/")))
            {
                this.CreateModulePackage(cultureCode, desktopModule, false);
            }
            foreach (var provider in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Provider"))
            {
                this.CreateProviderPackage(cultureCode, provider, false);
            }
            foreach (var authSystem in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Auth_System"))
            {
                this.CreateAuthSystemPackage(cultureCode, authSystem, false);
            }
            foreach (var library in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Library" || p.PackageType == "EvoqConnector"))
            {
                //only generate if a folder name is known for the library
                if (library.FolderName != null)
                {
                    this.CreateLibraryPackage(cultureCode, library, false);
                }
            }

            fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
            fileName = fileName + "." + package.Version.ToString(3) + "." + language.Code + ".zip";

            var packageWriter = PackageWriterFactory.GetWriter(package) as LanguagePackWriter;
            packageWriter.Language = language;
            packageWriter.BasePath = "";
            foreach (KeyValuePair<string, InstallFile> kvp in this._Files)
            {
                packageWriter.Files.Add(kvp.Key, kvp.Value);
            }
            packageWriter.CreatePackage(fileName, package.Name + " " + language.Text + ".dnn", this._Manifest, true);
        }

        public bool CreateLibraryPackage(string cultureCode, PackageInfo library, bool createZip)
        {
            var package = new PackageInfo
            {
                Name = library.Name,
                FriendlyName = library.FriendlyName,
                Version = library.Version,
                License = Util.PACKAGE_NoLicense
            };

            var fileName = Path.Combine(this.BasePath, "ResourcePack" + package.Name);
            return this.CreatePackage(cultureCode, package, library.PackageID, library.FolderName, fileName, createZip);
        }

        public bool CreateModulePackage(string cultureCode, DesktopModuleInfo desktopModule, bool createZip)
        {
            var modulePackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID);

            var package = new PackageInfo
            {
                Name = modulePackage.Name,
                FriendlyName = modulePackage.FriendlyName,
                Version = modulePackage.Version,
                License = Util.PACKAGE_NoLicense
            };

            var fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
            return this.CreatePackage(cultureCode, package, modulePackage.PackageID, Path.Combine("DesktopModules\\", desktopModule.FolderName), fileName, createZip);
        }

        public bool CreateProviderPackage(string cultureCode, PackageInfo providerPackage, bool createZip)
        {
            var package = new PackageInfo
            {
                Name = providerPackage.Name,
                FriendlyName = providerPackage.FriendlyName,
                Version = providerPackage.Version,
                License = Util.PACKAGE_NoLicense
            };

            var fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);

            //Get the provider "path"
            XmlDocument configDoc = Config.Load();
            string providerName = package.Name;
            if (providerName.IndexOf(".", StringComparison.Ordinal) > Null.NullInteger)
            {
                providerName = providerName.Substring(providerName.IndexOf(".", StringComparison.Ordinal) + 1);
            }
            switch (providerName)
            {
                case "SchedulingProvider":
                    providerName = "DNNScheduler";
                    break;
                case "SearchIndexProvider":
                    providerName = "ModuleIndexProvider";
                    break;
                case "SearchProvider":
                    providerName = "SearchDataStoreProvider";
                    break;
            }
            var providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" + providerName + "']") ??
                                    configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" + package.Name + "']");

            if (providerNavigator != null)
            {
                string providerPath = providerNavigator.GetAttribute("providerPath", "");
                return this.CreatePackage(cultureCode, package, providerPackage.PackageID,
                    providerPath.Substring(2, providerPath.Length - 2).Replace("/", "\\"), fileName, createZip);
            }
            else
            {
                return false;
            }
        }

        public string GetResourceFile(string type, string language, int portalId)
        {
            string resourcefilename = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
            if (language != Localization.SystemLocale)
            {
                resourcefilename = resourcefilename + "." + language;
            }
            if (type == "Portal")
            {
                resourcefilename = resourcefilename + "." + "Portal-" + portalId;
            }
            else if (type == "Host")
            {
                resourcefilename = resourcefilename + "." + "Host";
            }
            return HttpContext.Current.Server.MapPath(resourcefilename + ".resx");
        }

        private bool CreatePackage(string cultureCode, PackageInfo package, int dependentPackageId, string basePath, string fileName, bool createZip)
        {
            var language = LocaleController.Instance.GetLocale(cultureCode);
            var languagePack = new LanguagePackInfo
            {
                LanguageID = language.LanguageId,
                DependentPackageID = dependentPackageId
            };

            if (dependentPackageId == -2)
            {
                package.PackageType = "CoreLanguagePack";
            }
            else
            {
                package.PackageType = "ExtensionLanguagePack";
            }
            package.Name += " " + language.Text;
            package.FriendlyName += " " + language.Text;

            var packageWriter = PackageWriterFactory.GetWriter(package) as LanguagePackWriter;
            packageWriter.Language = language;
            packageWriter.LanguagePack = languagePack;
            packageWriter.BasePath = basePath;
            packageWriter.GetFiles(false);

            if (packageWriter.Files.Count > 0)
            {
                if (createZip)
                {
                    var manifest = packageWriter.WriteManifest(true);
                    fileName = fileName + "." + package.Version.ToString(3) + "." + language.Code + ".zip";
                    packageWriter.CreatePackage(fileName, package.Name + ".dnn", manifest, true);
                }
                else if (!this._Manifest.Contains($@"package name=""{package.Name}"""))
                {
                    packageWriter.BasePath = "";
                    this._Manifest += packageWriter.WriteManifest(true);
                    foreach (var kvp in packageWriter.Files)
                    {
                        this._Files[kvp.Key] = kvp.Value;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsAnalyzerType(Type type)
        {
            return type != null && type.FullName != null && (type.FullName.Contains("Lucene.Net.Analysis.Analyzer") || this.IsAnalyzerType(type.BaseType));
        }

        private bool IsAllowType(Type type)
        {
            return !type.FullName.Contains("Lucene.Net.Analysis.Analyzer") && !type.FullName.Contains("DotNetNuke");
        }

        private void UpdateResourceFileNode(XmlDocument xmlDoc, string key, string text)
        {
            XmlNode node;
            XmlNode nodeData;
            XmlAttribute attr;
            node = xmlDoc.SelectSingleNode("//root/data[@name='" + key + "']/value");
            if (node == null)
            {
                //missing entry
                nodeData = xmlDoc.CreateElement("data");
                attr = xmlDoc.CreateAttribute("name");
                attr.Value = key;
                nodeData.Attributes.Append(attr);
                xmlDoc.SelectSingleNode("//root").AppendChild(nodeData);
                node = nodeData.AppendChild(xmlDoc.CreateElement("value"));
            }
            node.InnerXml = HttpUtility.HtmlEncode(text);
        }
    }
}
