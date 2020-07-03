// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using ICSharpCode.SharpZipLib.Zip;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PackageWriterBase
    {
        private static readonly Regex FileVersionMatchRegex = new Regex(Util.REGEX_Version, RegexOptions.Compiled);

        private readonly Dictionary<string, InstallFile> _AppCodeFiles = new Dictionary<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> _Assemblies = new Dictionary<string, InstallFile>();
        private readonly SortedList<string, InstallFile> _CleanUpFiles = new SortedList<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> _Files = new Dictionary<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> _Resources = new Dictionary<string, InstallFile>();
        private readonly Dictionary<string, InstallFile> _Scripts = new Dictionary<string, InstallFile>();
        private readonly List<string> _Versions = new List<string>();
        private string _BasePath = Null.NullString;
        private PackageInfo _Package;

        public PackageWriterBase(PackageInfo package)
        {
            this._Package = package;
            this._Package.AttachInstallerInfo(new InstallerInfo());
        }

        protected PackageWriterBase()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of AppCodeFiles that should be included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> AppCodeFiles
        {
            get
            {
                return this._AppCodeFiles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Assemblies that should be included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> Assemblies
        {
            get
            {
                return this._Assemblies;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of CleanUpFiles that should be included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public SortedList<string, InstallFile> CleanUpFiles
        {
            get
            {
                return this._CleanUpFiles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that should be included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> Files
        {
            get
            {
                return this._Files;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to include Assemblies.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public virtual bool IncludeAssemblies
        {
            get
            {
                return true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Logger.
        /// </summary>
        /// <value>An Logger object.</value>
        /// -----------------------------------------------------------------------------
        public Logger Log
        {
            get
            {
                return this.Package.Log;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Resources that should be included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> Resources
        {
            get
            {
                return this._Resources;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Scripts that should be included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> Scripts
        {
            get
            {
                return this._Scripts;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a List of Versions that should be included in the Package.
        /// </summary>
        /// <value>A List(Of String).</value>
        /// -----------------------------------------------------------------------------
        public List<string> Versions
        {
            get
            {
                return this._Versions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Path for the Package's app code files.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string AppCodePath { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Path for the Package's assemblies.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string AssemblyPath { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Base Path for the Package.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string BasePath
        {
            get
            {
                return this._BasePath;
            }

            set
            {
                this._BasePath = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether a project file is found in the folder.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public bool HasProjectFile { get; set; }

        /// <summary>
        /// Gets or sets and sets whether there are any errors in parsing legacy packages.
        /// </summary>
        /// <value>
        /// <placeholder>And sets whether there are any errors in parsing legacy packages</placeholder>
        /// </value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string LegacyError { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the associated Package.
        /// </summary>
        /// <value>An PackageInfo object.</value>
        /// -----------------------------------------------------------------------------
        public PackageInfo Package
        {
            get
            {
                return this._Package;
            }

            set
            {
                this._Package = value;
            }
        }

        protected virtual Dictionary<string, string> Dependencies
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

        public static void WriteManifestEndElement(XmlWriter writer)
        {
            // Close packages Element
            writer.WriteEndElement();

            // Close root Element
            writer.WriteEndElement();
        }

        public static void WriteManifestStartElement(XmlWriter writer)
        {
            // Start the new Root Element
            writer.WriteStartElement("dotnetnuke");
            writer.WriteAttributeString("type", "Package");
            writer.WriteAttributeString("version", "5.0");

            // Start packages Element
            writer.WriteStartElement("packages");
        }

        public virtual void AddFile(InstallFile file)
        {
            switch (file.Type)
            {
                case InstallFileType.AppCode:
                    this._AppCodeFiles[file.FullName.ToLowerInvariant()] = file;
                    break;
                case InstallFileType.Assembly:
                    this._Assemblies[file.FullName.ToLowerInvariant()] = file;
                    break;
                case InstallFileType.CleanUp:
                    this._CleanUpFiles[file.FullName.ToLowerInvariant()] = file;
                    break;
                case InstallFileType.Script:
                    this._Scripts[file.FullName.ToLowerInvariant()] = file;
                    break;
                default:
                    this._Files[file.FullName.ToLowerInvariant()] = file;
                    break;
            }

            if ((file.Type == InstallFileType.CleanUp || file.Type == InstallFileType.Script) && FileVersionMatchRegex.IsMatch(file.Name))
            {
                string version = Path.GetFileNameWithoutExtension(file.Name);
                if (!this._Versions.Contains(version))
                {
                    this._Versions.Add(version);
                }
            }
        }

        public void AddResourceFile(InstallFile file)
        {
            this._Resources[file.FullName.ToLowerInvariant()] = file;
        }

        public void CreatePackage(string archiveName, string manifestName, string manifest, bool createManifest)
        {
            if (createManifest)
            {
                this.WriteManifest(manifestName, manifest);
            }

            this.AddFile(manifestName);
            this.CreateZipFile(archiveName);
        }

        public void GetFiles(bool includeSource)
        {
            // Call protected method that does the work
            this.GetFiles(includeSource, true);
        }

        /// <summary>
        /// WriteManifest writes an existing manifest.
        /// </summary>
        /// <param name="manifestName">The name of the manifest file.</param>
        /// <param name="manifest">The manifest.</param>
        /// <remarks>This overload takes a package manifest and writes it to a file.</remarks>
        public void WriteManifest(string manifestName, string manifest)
        {
            using (XmlWriter writer = XmlWriter.Create(Path.Combine(Globals.ApplicationMapPath, Path.Combine(this.BasePath, manifestName)), XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                this.Log.StartJob(Util.WRITER_CreatingManifest);
                this.WriteManifest(writer, manifest);
                this.Log.EndJob(Util.WRITER_CreatedManifest);
            }
        }

        /// <summary>
        /// WriteManifest writes a package manifest to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter.</param>
        /// <param name="manifest">The manifest.</param>
        /// <remarks>This overload takes a package manifest and writes it to a Writer.</remarks>
        public void WriteManifest(XmlWriter writer, string manifest)
        {
            WriteManifestStartElement(writer);
            writer.WriteRaw(manifest);

            // Close Dotnetnuke Element
            WriteManifestEndElement(writer);

            // Close Writer
            writer.Close();
        }

        /// <summary>
        /// WriteManifest writes the manifest assoicated with this PackageWriter to a string.
        /// </summary>
        /// <param name="packageFragment">A flag that indicates whether to return the package element
        /// as a fragment (True) or whether to add the outer dotnetnuke and packages elements (False).</param>
        /// <returns>The manifest as a string.</returns>
        /// <remarks></remarks>
        public string WriteManifest(bool packageFragment)
        {
            // Create a writer to create the processed manifest
            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                this.WriteManifest(writer, packageFragment);

                // Close XmlWriter
                writer.Close();

                // Return new manifest
                return sb.ToString();
            }
        }

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
            this.WritePackageEndElement(writer);

            if (!packageFragment)
            {
                // Close Dotnetnuke Element
                WriteManifestEndElement(writer);
            }

            this.Log.EndJob(Util.WRITER_CreatedManifest);
        }

        protected virtual void AddFile(string fileName)
        {
            this.AddFile(new InstallFile(fileName, this.Package.InstallerInfo));
        }

        protected virtual void AddFile(string fileName, string sourceFileName)
        {
            this.AddFile(new InstallFile(fileName, sourceFileName, this.Package.InstallerInfo));
        }

        protected virtual void ConvertLegacyManifest(XPathNavigator legacyManifest, XmlWriter writer)
        {
        }

        protected virtual void GetFiles(bool includeSource, bool includeAppCode)
        {
            string baseFolder = Path.Combine(Globals.ApplicationMapPath, this.BasePath);
            if (Directory.Exists(baseFolder))
            {
                // Create the DirectoryInfo object
                var folderInfo = new DirectoryInfo(baseFolder);

                // Get the Project File in the folder
                FileInfo[] files = folderInfo.GetFiles("*.??proj");

                if (files.Length == 0) // Assume Dynamic (App_Code based) Module
                {
                    // Add the files in the DesktopModules Folder
                    this.ParseFolder(baseFolder, baseFolder);

                    // Add the files in the AppCode Folder
                    if (includeAppCode)
                    {
                        string appCodeFolder = Path.Combine(Globals.ApplicationMapPath, this.AppCodePath);
                        this.ParseFolder(appCodeFolder, appCodeFolder);
                    }
                }
                else // WAP Project File is present
                {
                    this.HasProjectFile = true;

                    // Parse the Project files (probably only one)
                    foreach (FileInfo projFile in files)
                    {
                        this.ParseProjectFile(projFile, includeSource);
                    }
                }
            }
        }

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

        protected virtual void WriteFilesToManifest(XmlWriter writer)
        {
            var fileWriter = new FileComponentWriter(this.BasePath, this.Files, this.Package);
            fileWriter.WriteManifest(writer);
        }

        protected virtual void WriteManifestComponent(XmlWriter writer)
        {
        }

        private void AddFilesToZip(ZipOutputStream stream, IDictionary<string, InstallFile> files, string basePath)
        {
            foreach (InstallFile packageFile in files.Values)
            {
                string filepath;
                if (string.IsNullOrEmpty(basePath))
                {
                    filepath = Path.Combine(Globals.ApplicationMapPath, packageFile.FullName);
                }
                else
                {
                    filepath = Path.Combine(Path.Combine(Globals.ApplicationMapPath, basePath), packageFile.FullName.Replace(basePath + "\\", string.Empty));
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
            int CompressionLevel = 9;
            var zipFile = new FileInfo(zipFileName);

            string ZipFileShortName = zipFile.Name;

            FileStream strmZipFile = null;
            this.Log.StartJob(Util.WRITER_CreatingPackage);
            try
            {
                this.Log.AddInfo(string.Format(Util.WRITER_CreateArchive, ZipFileShortName));
                strmZipFile = File.Create(zipFileName);
                ZipOutputStream strmZipStream = null;
                try
                {
                    strmZipStream = new ZipOutputStream(strmZipFile);
                    strmZipStream.SetLevel(CompressionLevel);

                    // Add Files To zip
                    this.AddFilesToZip(strmZipStream, this._Assemblies, string.Empty);
                    this.AddFilesToZip(strmZipStream, this._AppCodeFiles, this.AppCodePath);
                    this.AddFilesToZip(strmZipStream, this._Files, this.BasePath);
                    this.AddFilesToZip(strmZipStream, this._CleanUpFiles, this.BasePath);
                    this.AddFilesToZip(strmZipStream, this._Resources, this.BasePath);
                    this.AddFilesToZip(strmZipStream, this._Scripts, this.BasePath);
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
                        strmZipStream.Finish();
                        strmZipStream.Close();
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
                if (strmZipFile != null)
                {
                    strmZipFile.Close();
                }
            }
        }

        private void WritePackageEndElement(XmlWriter writer)
        {
            // Close components Element
            writer.WriteEndElement();

            // Close package Element
            writer.WriteEndElement();
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
