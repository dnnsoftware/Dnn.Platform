// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text.RegularExpressions;

    using ICSharpCode.SharpZipLib.Zip;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallFile class represents a single file in an Installer Package.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class InstallFile
    {
        private static readonly Regex FileTypeMatchRegex = new Regex(Util.REGEX_Version + ".txt", RegexOptions.Compiled);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFile"/> class.
        /// This Constructor creates a new InstallFile instance from a ZipInputStream and a ZipEntry.
        /// </summary>
        /// <remarks>The ZipInputStream is read into a byte array (Buffer), and the ZipEntry is used to
        /// set up the properties of the InstallFile class.</remarks>
        /// <param name="zip">The ZipInputStream.</param>
        /// <param name="entry">The ZipEntry.</param>
        /// <param name="info">An INstallerInfo instance.</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(ZipInputStream zip, ZipEntry entry, InstallerInfo info)
        {
            this.Encoding = TextEncoding.UTF8;
            this.InstallerInfo = info;
            this.ReadZip(zip, entry);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFile"/> class.
        /// This Constructor creates a new InstallFile instance.
        /// </summary>
        /// <param name="fileName">The fileName of the File.</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName)
        {
            this.Encoding = TextEncoding.UTF8;
            this.ParseFileName(fileName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFile"/> class.
        /// This Constructor creates a new InstallFile instance.
        /// </summary>
        /// <param name="fileName">The fileName of the File.</param>
        /// <param name="info">An INstallerInfo instance.</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, InstallerInfo info)
        {
            this.Encoding = TextEncoding.UTF8;
            this.ParseFileName(fileName);
            this.InstallerInfo = info;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFile"/> class.
        /// This Constructor creates a new InstallFile instance.
        /// </summary>
        /// <param name="fileName">The fileName of the File.</param>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="info">An INstallerInfo instance.</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, string sourceFileName, InstallerInfo info)
        {
            this.Encoding = TextEncoding.UTF8;
            this.ParseFileName(fileName);
            this.SourceFileName = sourceFileName;
            this.InstallerInfo = info;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFile"/> class.
        /// This Constructor creates a new InstallFile instance.
        /// </summary>
        /// <param name="fileName">The file name of the File.</param>
        /// <param name="filePath">The file path of the file.</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, string filePath)
        {
            this.Encoding = TextEncoding.UTF8;
            this.Name = fileName;
            this.Path = filePath;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the backup file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string BackupFileName
        {
            get
            {
                return System.IO.Path.Combine(this.BackupPath, this.Name + ".config");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the backup folder.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public virtual string BackupPath
        {
            get
            {
                return System.IO.Path.Combine(this.InstallerInfo.TempInstallFolder, System.IO.Path.Combine("Backup", this.Path));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the File Extension of the file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string Extension
        {
            get
            {
                string ext = System.IO.Path.GetExtension(this.Name);
                if (string.IsNullOrEmpty(ext))
                {
                    return string.Empty;
                }

                return ext.Substring(1);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Full Name of the file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string FullName
        {
            get
            {
                return System.IO.Path.Combine(this.Path, this.Name);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the temporary file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string TempFileName
        {
            get
            {
                string fileName = this.SourceFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = this.FullName;
                }

                return System.IO.Path.Combine(this.InstallerInfo.TempInstallFolder, fileName);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Action for this file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string Action { get; set; }

        public TextEncoding Encoding { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated InstallerInfo.
        /// </summary>
        /// <value>An InstallerInfo object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public InstallerInfo InstallerInfo { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name of the file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string Name { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path of the file.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string Path { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the source file name.
        /// </summary>
        /// <value>A string.</value>
        /// -----------------------------------------------------------------------------
        public string SourceFileName { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Type of the file.
        /// </summary>
        /// <value>An InstallFileType Enumeration.</value>
        /// -----------------------------------------------------------------------------
        public InstallFileType Type { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the file.
        /// </summary>
        /// <value>A System.Version.</value>
        /// -----------------------------------------------------------------------------
        public Version Version { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The SetVersion method sets the version of the file.
        /// </summary>
        /// <param name="version">The version of the file.</param>
        /// -----------------------------------------------------------------------------
        public void SetVersion(Version version)
        {
            this.Version = version;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ParseFileName parses the ZipEntry metadata.
        /// </summary>
        /// <param name="fileName">A String representing the file name.</param>
        /// -----------------------------------------------------------------------------
        private void ParseFileName(string fileName)
        {
            int i = fileName.Replace("\\", "/").LastIndexOf("/", StringComparison.Ordinal);
            if (i < 0)
            {
                this.Name = fileName.Substring(0, fileName.Length);
                this.Path = string.Empty;
            }
            else
            {
                this.Name = fileName.Substring(i + 1, fileName.Length - (i + 1));
                this.Path = fileName.Substring(0, i);
            }

            if (string.IsNullOrEmpty(this.Path) && fileName.StartsWith("[app_code]"))
            {
                this.Name = fileName.Substring(10, fileName.Length - 10);
                this.Path = fileName.Substring(0, 10);
            }

            if (this.Name.Equals("manifest.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Type = InstallFileType.Manifest;
            }
            else
            {
                switch (this.Extension.ToLowerInvariant())
                {
                    case "ascx":
                        this.Type = InstallFileType.Ascx;
                        break;
                    case "dll":
                        this.Type = InstallFileType.Assembly;
                        break;
                    case "dnn":
                    case "dnn5":
                    case "dnn6":
                    case "dnn7":
                        this.Type = InstallFileType.Manifest;
                        break;
                    case "resx":
                        this.Type = InstallFileType.Language;
                        break;
                    case "resources":
                    case "zip":
                        this.Type = InstallFileType.Resources;
                        break;
                    default:
                        if (this.Extension.EndsWith("dataprovider", StringComparison.InvariantCultureIgnoreCase) || this.Extension.Equals("sql", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.Type = InstallFileType.Script;
                        }
                        else if (this.Path.StartsWith("[app_code]"))
                        {
                            this.Type = InstallFileType.AppCode;
                        }
                        else
                        {
                            this.Type = FileTypeMatchRegex.IsMatch(this.Name) ? InstallFileType.CleanUp : InstallFileType.Other;
                        }

                        break;
                }
            }

            // remove [app_code] token
            this.Path = this.Path.Replace("[app_code]", string.Empty);

            // remove starting "\"
            if (this.Path.StartsWith("\\"))
            {
                this.Path = this.Path.Substring(1);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadZip method reads the zip stream and parses the ZipEntry metadata.
        /// </summary>
        /// <param name="unzip">A ZipStream containing the file content.</param>
        /// <param name="entry">A ZipEntry containing the file metadata.</param>
        /// -----------------------------------------------------------------------------
        private void ReadZip(ZipInputStream unzip, ZipEntry entry)
        {
            this.ParseFileName(entry.Name);
            Util.WriteStream(unzip, this.TempFileName);
            File.SetLastWriteTime(this.TempFileName, entry.DateTime);
        }
    }
}
