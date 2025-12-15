// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The PackageWriter class.</summary>
    public class PackageWriterBase
    {
        private static readonly Regex FileVersionMatchRegex = new Regex(Util.REGEX_Version, RegexOptions.Compiled);

        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly Dictionary<string, InstallFile> appCodeFiles = new Dictionary<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> assemblies = new Dictionary<string, InstallFile>();
        private readonly SortedList<string, InstallFile> cleanUpFiles = new SortedList<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> files = new Dictionary<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> resources = new Dictionary<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> scripts = new Dictionary<string, InstallFile>();
        private readonly List<string> versions = new List<string>();
        private string basePath = Null.NullString;
        private PackageInfo package;

        /// <summary>Initializes a new instance of the <see cref="PackageWriterBase"/> class.</summary>
        /// <param name="package">The PackageInfo to use to initialize the writer.</param>
        public PackageWriterBase(PackageInfo package)
        {
            this.package = package;
            this.package.AttachInstallerInfo(new InstallerInfo());
            this.applicationStatusInfo = Common.Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        }

        /// <summary>Initializes a new instance of the <see cref="PackageWriterBase"/> class.</summary>
        protected PackageWriterBase()
        {
            this.applicationStatusInfo = Common.Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        }

        /// <summary>Gets a Dictionary of AppCodeFiles that should be included in the Package.</summary>
        public Dictionary<string, InstallFile> AppCodeFiles
        {
            get
            {
                return this.appCodeFiles;
            }
        }

        /// <summary>Gets a Dictionary of Assemblies that should be included in the Package.</summary>
        public Dictionary<string, InstallFile> Assemblies
        {
            get
            {
                return this.assemblies;
            }
        }

        /// <summary>Gets a Dictionary of CleanUpFiles that should be included in the Package.</summary>
        public SortedList<string, InstallFile> CleanUpFiles
        {
            get
            {
                return this.cleanUpFiles;
            }
        }

        /// <summary>Gets a Dictionary of Files that should be included in the Package.</summary>
        public Dictionary<string, InstallFile> Files
        {
            get
            {
                return this.files;
            }
        }

        /// <summary>Gets a value indicating whether to include Assemblies.</summary>
        public virtual bool IncludeAssemblies
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets the Logger.</summary>
        public Logger Log
        {
            get
            {
                return this.Package.Log;
            }
        }

        /// <summary>Gets a Dictionary of Resources that should be included in the Package.</summary>
        public Dictionary<string, InstallFile> Resources
        {
            get
            {
                return this.resources;
            }
        }

        /// <summary>Gets a Dictionary of Scripts that should be included in the Package.</summary>
        public Dictionary<string, InstallFile> Scripts
        {
            get
            {
                return this.scripts;
            }
        }

        /// <summary>Gets a List of Versions that should be included in the Package.</summary>
        public List<string> Versions
        {
            get
            {
                return this.versions;
            }
        }

        /// <summary>Gets or sets the Path for the Package's app code files.</summary>
        public string AppCodePath { get; set; }

        /// <summary>Gets or sets the Path for the Package's assemblies.</summary>
        public string AssemblyPath { get; set; }

        /// <summary>Gets or sets the Base Path for the Package.</summary>
        public string BasePath
        {
            get
            {
                return this.basePath;
            }

            set
            {
                this.basePath = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether a project file is found in the folder.</summary>
        public bool HasProjectFile { get; set; }

        /// <summary>Gets or sets whether there are any errors in parsing legacy packages.</summary>
        /// <value>And sets whether there are any errors in parsing legacy packages.</value>
        public string LegacyError { get; set; }

        /// <summary>Gets or sets the associated Package.</summary>
        public PackageInfo Package
        {
            get
            {
                return this.package;
            }

            set
            {
                this.package = value;
            }
        }

        /// <summary>
        /// Gets the dependencies.
        /// </summary>
        protected virtual Dictionary<string, string> Dependencies
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Writes the manifest end element (closes the packages and root elements).
        /// </summary>
        /// <param name="writer">The XML writer to use.</param>
        public static void WriteManifestEndElement(XmlWriter writer)
        {
            // Close packages Element
            writer.WriteEndElement();

            // Close root Element
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the manifest start elements.
        /// </summary>
        /// <param name="writer">The XML writer to use.</param>
        public static void WriteManifestStartElement(XmlWriter writer)
        {
            // Start the new Root Element
            writer.WriteStartElement("dotnetnuke");
            writer.WriteAttributeString("type", "Package");
            writer.WriteAttributeString("version", "5.0");

            // Start packages Element
            writer.WriteStartElement("packages");
        }

        /// <summary>
        /// Adds a file to the package.
        /// </summary>
        /// <param name="file">The file to add.</param>
        public virtual void AddFile(InstallFile file)
        {
            switch (file.Type)
            {
                case InstallFileType.AppCode:
                    this.appCodeFiles[file.FullName.ToLowerInvariant()] = file;
                    break;
                case InstallFileType.Assembly:
                    this.assemblies[file.FullName.ToLowerInvariant()] = file;
                    break;
                case InstallFileType.CleanUp:
                    this.cleanUpFiles[file.FullName.ToLowerInvariant()] = file;
                    break;
                case InstallFileType.Script:
                    this.scripts[file.FullName.ToLowerInvariant()] = file;
                    break;
                default:
                    this.files[file.FullName.ToLowerInvariant()] = file;
                    break;
            }

            if ((file.Type == InstallFileType.CleanUp || file.Type == InstallFileType.Script) && FileVersionMatchRegex.IsMatch(file.Name))
            {
                string version = Path.GetFileNameWithoutExtension(file.Name);
                if (!this.versions.Contains(version))
                {
                    this.versions.Add(version);
                }
            }
        }

        /// <summary>
        /// Adds a resource file to the package.
        /// </summary>
        /// <param name="file">The resource file to add.</param>
        public void AddResourceFile(InstallFile file)
        {
            this.resources[file.FullName.ToLowerInvariant()] = file;
        }

        /// <summary>
        /// Creates a package.
        /// </summary>
        /// <param name="archiveName">Name of the zip archive to produce.</param>
        /// <param name="manifestName">Name of the manifest.</param>
        /// <param name="manifest">The manifest content.</param>
        /// <param name="createManifest">A value indicating whether to create the manifest.</param>
        public void CreatePackage(string archiveName, string manifestName, string manifest, bool createManifest)
        {
            if (createManifest)
            {
                this.WriteManifest(manifestName, manifest);
            }

            this.AddFile(manifestName);
            this.CreateZipFile(archiveName);
        }

        /// <summary>
        /// Gets the files to include in the package.
        /// </summary>
        /// <param name="includeSource">If true, the source code files will also be included.</param>
        public void GetFiles(bool includeSource)
        {
            this.GetFiles(includeSource, true);
        }

        /// <summary>Writes an existing manifest.</summary>
        /// <param name="manifestName">The name of the manifest file.</param>
        /// <param name="manifest">The manifest.</param>
        /// <remarks>This overload takes a package manifest and writes it to a file.</remarks>
        public void WriteManifest(string manifestName, string manifest)
        {
            using (XmlWriter writer = XmlWriter.Create(Path.Combine(this.applicationStatusInfo.ApplicationMapPath, Path.Combine(this.BasePath, manifestName)), XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                this.Log.StartJob(Util.WRITER_CreatingManifest);
                this.WriteManifest(writer, manifest);
                this.Log.EndJob(Util.WRITER_CreatedManifest);
            }
        }

        /// <summary>WriteManifest writes a package manifest to an XmlWriter.</summary>
        /// <param name="writer">The XmlWriter.</param>
        /// <param name="manifest">The manifest.</param>
        /// <remarks>This overload takes a package manifest and writes it to a Writer.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void WriteManifest(XmlWriter writer, string manifest)
        {
            WriteManifestStartElement(writer);
            writer.WriteRaw(manifest);
            WriteManifestEndElement(writer);
            writer.Close();
        }

        /// <summary>WriteManifest writes the manifest associated with this PackageWriter to a string.</summary>
        /// <param name="packageFragment">A flag that indicates whether to return the package element
        /// as a fragment (True) or whether to add the outer <c>dotnetnuke</c> and <c>packages</c> elements (False).</param>
        /// <returns>The manifest as a string.</returns>
        public string WriteManifest(bool packageFragment)
        {
            // Create a writer to create the processed manifest
            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                this.WriteManifest(writer, packageFragment);
                writer.Close();
                return sb.ToString();
            }
        }

        /// <summary>
        /// Writes the manifest.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="packageFragment">If true, will not write the start and end Xml elements because we are adding only a fragment.</param>
        public void WriteManifest(XmlWriter writer, bool packageFragment)
        {
            this.Log.StartJob(Util.WRITER_CreatingManifest);

            if (!packageFragment)
            {
                // Start dotnetnuke element
                WriteManifestStartElement(writer);
            }

            // Start package Element
            this.WritePackageStartElement(writer);

            // write Script Component
            if (this.Scripts.Count > 0)
            {
                var scriptWriter = new ScriptComponentWriter(this.BasePath, this.Scripts, this.Package);
                scriptWriter.WriteManifest(writer);
            }

            // write Clean Up Files Component
            if (this.CleanUpFiles.Count > 0)
            {
                var cleanupFileWriter = new CleanupComponentWriter(this.BasePath, this.CleanUpFiles);
                cleanupFileWriter.WriteManifest(writer);
            }

            // Write the Custom Component
            this.WriteManifestComponent(writer);

            // Write Assemblies Component
            if (this.Assemblies.Count > 0)
            {
                var assemblyWriter = new AssemblyComponentWriter(this.AssemblyPath, this.Assemblies, this.Package);
                assemblyWriter.WriteManifest(writer);
            }

            // Write AppCode Files Component
            if (this.AppCodeFiles.Count > 0)
            {
                var fileWriter = new FileComponentWriter(this.AppCodePath, this.AppCodeFiles, this.Package);
                fileWriter.WriteManifest(writer);
            }

            // write Files Component
            if (this.Files.Count > 0)
            {
                this.WriteFilesToManifest(writer);
            }

            // write ResourceFiles Component
            if (this.Resources.Count > 0)
            {
                var fileWriter = new ResourceFileComponentWriter(this.BasePath, this.Resources, this.Package);
                fileWriter.WriteManifest(writer);
            }

            // Close Package
            WritePackageEndElement(writer);

            if (!packageFragment)
            {
                // Close Dotnetnuke Element
                WriteManifestEndElement(writer);
            }

            this.Log.EndJob(Util.WRITER_CreatedManifest);
        }

        /// <summary>
        /// Adds a file to the package.
        /// </summary>
        /// <param name="fileName">Name of the file to add.</param>
        protected virtual void AddFile(string fileName)
        {
            this.AddFile(new InstallFile(fileName, this.Package.InstallerInfo));
        }

        /// <summary>
        /// Adds a file to the package.
        /// </summary>
        /// <param name="fileName">Name of the file to use in the package.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        protected virtual void AddFile(string fileName, string sourceFileName)
        {
            this.AddFile(new InstallFile(fileName, sourceFileName, this.Package.InstallerInfo));
        }

        /// <summary>
        /// Converts a legacy manifest to current standards.
        /// </summary>
        /// <param name="legacyManifest">The legacy manifest.</param>
        /// <param name="writer">The XML writer to use.</param>
        protected virtual void ConvertLegacyManifest(XPathNavigator legacyManifest, XmlWriter writer)
        {
        }

        /// <summary>
        /// Gets the files to include in the package.
        /// </summary>
        /// <param name="includeSource">If true, will also include the source code files.</param>
        /// <param name="includeAppCode">If true, files in the App_Code folder will also be included.</param>
        protected virtual void GetFiles(bool includeSource, bool includeAppCode)
        {
            string baseFolder = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, this.BasePath);
            if (Directory.Exists(baseFolder))
            {
                // Create the DirectoryInfo object
                var folderInfo = new DirectoryInfo(baseFolder);

                // Get the Project File in the folder
                FileInfo[] files = folderInfo.GetFiles("*.??proj");

                if (files.Length == 0)
                {
                    // Assume Dynamic (App_Code based) Module
                    // Add the files in the DesktopModules Folder
                    this.ParseFolder(baseFolder, baseFolder);

                    // Add the files in the AppCode Folder
                    if (includeAppCode)
                    {
                        string appCodeFolder = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, this.AppCodePath);
                        this.ParseFolder(appCodeFolder, appCodeFolder);
                    }
                }
                else
                {
                    // WAP Project File is present
                    this.HasProjectFile = true;

                    // Parse the Project files (probably only one)
                    foreach (FileInfo projFile in files)
                    {
                        this.ParseProjectFile(projFile, includeSource);
                    }
                }
            }
        }

        /// <summary>
        /// Parses the files in a folder.
        /// </summary>
        /// <param name="folder">The folder to get the files from.</param>
        /// <param name="rootPath">The root path to use so the files are relative in the manifest.</param>
        protected virtual void ParseFiles(DirectoryInfo folder, string rootPath)
        {
            // Add the Files in the Folder
            FileInfo[] files = folder.GetFiles();
            foreach (FileInfo file in files)
            {
                string filePath = folder.FullName.Replace(rootPath, string.Empty);
                if (filePath.StartsWith("\\"))
                {
                    filePath = filePath.Substring(1);
                }

                if (folder.FullName.ToLowerInvariant().Contains("app_code"))
                {
                    filePath = "[app_code]" + filePath;
                }

                if (!file.Extension.Equals(".dnn", StringComparison.InvariantCultureIgnoreCase) && (file.Attributes & FileAttributes.Hidden) == 0)
                {
                    this.AddFile(Path.Combine(filePath, file.Name));
                }
            }
        }

        /// <summary>
        /// Parses a folder for files recursively.
        /// </summary>
        /// <param name="folderName">Name of the folder to parse.</param>
        /// <param name="rootPath">The root path so the files are relative in the manifest.</param>
        protected virtual void ParseFolder(string folderName, string rootPath)
        {
            if (Directory.Exists(folderName))
            {
                var folder = new DirectoryInfo(folderName);

                // Recursively parse the subFolders
                DirectoryInfo[] subFolders = folder.GetDirectories();
                foreach (DirectoryInfo subFolder in subFolders)
                {
                    if ((subFolder.Attributes & FileAttributes.Hidden) == 0)
                    {
                        this.ParseFolder(subFolder.FullName, rootPath);
                    }
                }

                // Add the Files in the Folder
                this.ParseFiles(folder, rootPath);
            }
        }

        /// <summary>
        /// Parses a project file.
        /// </summary>
        /// <param name="projFile">The proj file to parse.</param>
        /// <param name="includeSource">If true, the source code files will also be included.</param>
        protected void ParseProjectFile(FileInfo projFile, bool includeSource)
        {
            string fileName = string.Empty;

            // Create an XPathDocument from the Xml
            var doc = new XPathDocument(new FileStream(projFile.FullName, FileMode.Open, FileAccess.Read));
            XPathNavigator rootNav = doc.CreateNavigator();
            var manager = new XmlNamespaceManager(rootNav.NameTable);
            manager.AddNamespace("proj", "http://schemas.microsoft.com/developer/msbuild/2003");
            rootNav.MoveToFirstChild();

            XPathNavigator assemblyNav = rootNav.SelectSingleNode("proj:PropertyGroup/proj:AssemblyName", manager);
            fileName = assemblyNav.Value;
            XPathNavigator buildPathNav = rootNav.SelectSingleNode("proj:PropertyGroup/proj:OutputPath", manager);
            string buildPath = buildPathNav.Value.Replace("..\\", string.Empty);
            buildPath = buildPath.Replace(this.AssemblyPath + "\\", string.Empty);
            this.AddFile(Path.Combine(buildPath, fileName + ".dll"));

            // Check for referenced assemblies
            foreach (XPathNavigator itemNav in rootNav.Select("proj:ItemGroup/proj:Reference", manager))
            {
                fileName = Util.ReadAttribute(itemNav, "Include");
                if (fileName.IndexOf(",") > -1)
                {
                    fileName = fileName.Substring(0, fileName.IndexOf(","));
                }

                if (
                    !(fileName.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || fileName.StartsWith("microsoft", StringComparison.InvariantCultureIgnoreCase) || fileName.Equals("dotnetnuke", StringComparison.InvariantCultureIgnoreCase) ||
                      fileName.Equals("dotnetnuke.webutility", StringComparison.InvariantCultureIgnoreCase) || fileName.Equals("dotnetnuke.webcontrols", StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.AddFile(fileName + ".dll");
                }
            }

            // Add all the files that are classified as None
            foreach (XPathNavigator itemNav in rootNav.Select("proj:ItemGroup/proj:None", manager))
            {
                fileName = Util.ReadAttribute(itemNav, "Include");
                this.AddFile(fileName);
            }

            // Add all the files that are classified as Content
            foreach (XPathNavigator itemNav in rootNav.Select("proj:ItemGroup/proj:Content", manager))
            {
                fileName = Util.ReadAttribute(itemNav, "Include");
                this.AddFile(fileName);
            }

            // Add all the files that are classified as Compile
            if (includeSource)
            {
                foreach (XPathNavigator itemNav in rootNav.Select("proj:ItemGroup/proj:Compile", manager))
                {
                    fileName = Util.ReadAttribute(itemNav, "Include");
                    this.AddFile(fileName);
                }
            }
        }

        /// <summary>
        /// Writes the files to the manifest.
        /// </summary>
        /// <param name="writer">The XML writer to use.</param>
        protected virtual void WriteFilesToManifest(XmlWriter writer)
        {
            var fileWriter = new FileComponentWriter(this.BasePath, this.Files, this.Package);
            fileWriter.WriteManifest(writer);
        }

        /// <summary>
        /// Writes the manifest component.
        /// </summary>
        /// <param name="writer">The XML writer to use.</param>
        protected virtual void WriteManifestComponent(XmlWriter writer)
        {
        }

        private static void WritePackageEndElement(XmlWriter writer)
        {
            // Close components Element
            writer.WriteEndElement();

            // Close package Element
            writer.WriteEndElement();
        }

        private void AddFilesToZip(ZipArchive stream, IDictionary<string, InstallFile> files, string basePath)
        {
            foreach (InstallFile packageFile in files.Values)
            {
                string filepath;
                if (string.IsNullOrEmpty(basePath))
                {
                    filepath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, packageFile.FullName);
                }
                else
                {
                    filepath = Path.Combine(Path.Combine(this.applicationStatusInfo.ApplicationMapPath, basePath), packageFile.FullName.Replace(basePath + "\\", string.Empty));
                }

                if (File.Exists(filepath))
                {
                    string packageFilePath = packageFile.Path;
                    if (!string.IsNullOrEmpty(basePath))
                    {
                        packageFilePath = packageFilePath.Replace(basePath + "\\", string.Empty);
                    }

                    FileSystemUtils.AddToZip(ref stream, filepath, packageFile.Name, packageFilePath);
                    this.Log.AddInfo(string.Format(Util.WRITER_SavedFile, packageFile.FullName));
                }
            }
        }

        private void CreateZipFile(string zipFileName)
        {
            var zipFile = new FileInfo(zipFileName);

            string zipFileShortName = zipFile.Name;

            FileStream strmZipFile = null;
            this.Log.StartJob(Util.WRITER_CreatingPackage);
            try
            {
                this.Log.AddInfo(string.Format(Util.WRITER_CreateArchive, zipFileShortName));
                strmZipFile = File.Create(zipFileName);
                ZipArchive strmZipStream = null;
                try
                {
                    strmZipStream = new ZipArchive(strmZipFile, ZipArchiveMode.Create, true);

                    // Add Files To zip
                    this.AddFilesToZip(strmZipStream, this.assemblies, string.Empty);
                    this.AddFilesToZip(strmZipStream, this.appCodeFiles, this.AppCodePath);
                    this.AddFilesToZip(strmZipStream, this.files, this.BasePath);
                    this.AddFilesToZip(strmZipStream, this.cleanUpFiles, this.BasePath);
                    this.AddFilesToZip(strmZipStream, this.resources, this.BasePath);
                    this.AddFilesToZip(strmZipStream, this.scripts, this.BasePath);
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                    this.Log.AddFailure(string.Format(Util.WRITER_SaveFileError, ex));
                }
                finally
                {
                    if (strmZipStream != null)
                    {
                        strmZipStream.Dispose();
                    }
                }

                this.Log.EndJob(Util.WRITER_CreatedPackage);
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                this.Log.AddFailure(string.Format(Util.WRITER_SaveFileError, ex));
            }
            finally
            {
                strmZipFile?.Close();
            }
        }

        private void WritePackageStartElement(XmlWriter writer)
        {
            // Start package Element
            writer.WriteStartElement("package");
            writer.WriteAttributeString("name", this.Package.Name);
            writer.WriteAttributeString("type", this.Package.PackageType);
            writer.WriteAttributeString("version", this.Package.Version.ToString(3));

            // Write FriendlyName
            writer.WriteElementString("friendlyName", this.Package.FriendlyName);

            // Write Description
            writer.WriteElementString("description", this.Package.Description);
            writer.WriteElementString("iconFile", Util.ParsePackageIconFileName(this.Package));

            // Write Author
            writer.WriteStartElement("owner");

            writer.WriteElementString("name", this.Package.Owner);
            writer.WriteElementString("organization", this.Package.Organization);
            writer.WriteElementString("url", this.Package.Url);
            writer.WriteElementString("email", this.Package.Email);

            // Write Author End
            writer.WriteEndElement();

            // Write License
            writer.WriteElementString("license", this.Package.License);

            // Write Release Notes
            writer.WriteElementString("releaseNotes", this.Package.ReleaseNotes);

            // Write Dependencies
            if (this.Dependencies.Count > 0)
            {
                writer.WriteStartElement("dependencies");
                foreach (KeyValuePair<string, string> kvp in this.Dependencies)
                {
                    writer.WriteStartElement("dependency");
                    writer.WriteAttributeString("type", kvp.Key);
                    writer.WriteString(kvp.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            // Write components Element
            writer.WriteStartElement("components");
        }
    }
}
