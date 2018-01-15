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
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

using ICSharpCode.SharpZipLib.Zip;

#endregion

namespace DotNetNuke.Services.Installer
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallFile class represents a single file in an Installer Package
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class InstallFile
    {
        private static readonly Regex FileTypeMatchRegex = new Regex(Util.REGEX_Version + ".txt", RegexOptions.Compiled);

		#region Private Members

        #endregion

		#region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance from a ZipInputStream and a ZipEntry
        /// </summary>
        /// <remarks>The ZipInputStream is read into a byte array (Buffer), and the ZipEntry is used to
        /// set up the properties of the InstallFile class.</remarks>
        /// <param name="zip">The ZipInputStream</param>
        /// <param name="entry">The ZipEntry</param>
        /// <param name="info">An INstallerInfo instance</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(ZipInputStream zip, ZipEntry entry, InstallerInfo info)
        {
            Encoding = TextEncoding.UTF8;
            InstallerInfo = info;
            ReadZip(zip, entry);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The fileName of the File</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName)
        {
            Encoding = TextEncoding.UTF8;
            ParseFileName(fileName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The fileName of the File</param>
        /// <param name="info">An INstallerInfo instance</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, InstallerInfo info)
        {
            Encoding = TextEncoding.UTF8;
            ParseFileName(fileName);
            InstallerInfo = info;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The fileName of the File</param>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="info">An INstallerInfo instance</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, string sourceFileName, InstallerInfo info)
        {
            Encoding = TextEncoding.UTF8;
            ParseFileName(fileName);
            SourceFileName = sourceFileName;
            InstallerInfo = info;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The file name of the File</param>
        /// <param name="filePath">The file path of the file</param>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, string filePath)
        {
            Encoding = TextEncoding.UTF8;
            Name = fileName;
            Path = filePath;
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Action for this file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string Action { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the backup file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string BackupFileName
        {
            get
            {
                return System.IO.Path.Combine(BackupPath, Name + ".config");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the backup folder
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public virtual string BackupPath
        {
            get
            {
                return System.IO.Path.Combine(InstallerInfo.TempInstallFolder, System.IO.Path.Combine("Backup", Path));
            }
        }

        public TextEncoding Encoding { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the File Extension of the file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string Extension
        {
            get
            {
                string ext = System.IO.Path.GetExtension(Name);
                if (String.IsNullOrEmpty(ext))
                {
                    return "";
                }
                return ext.Substring(1);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Full Name of the file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string FullName
        {
            get
            {
                return System.IO.Path.Combine(Path, Name);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated InstallerInfo
        /// </summary>
        /// <value>An InstallerInfo object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public InstallerInfo InstallerInfo { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name of the file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string Name { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path of the file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string Path { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the source file name
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string SourceFileName { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the temporary file
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string TempFileName
        {
            get
            {
                string fileName = SourceFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = FullName;
                }
                return System.IO.Path.Combine(InstallerInfo.TempInstallFolder, fileName);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Type of the file
        /// </summary>
        /// <value>An InstallFileType Enumeration</value>
        /// -----------------------------------------------------------------------------
        public InstallFileType Type { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the file
        /// </summary>
        /// <value>A System.Version</value>
        /// -----------------------------------------------------------------------------
        public Version Version { get; private set; }

        #endregion

		#region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ParseFileName parses the ZipEntry metadata
        /// </summary>
        /// <param name="fileName">A String representing the file name</param>
        /// -----------------------------------------------------------------------------
        private void ParseFileName(string fileName)
        {
            int i = fileName.Replace("\\", "/").LastIndexOf("/", StringComparison.Ordinal);
            if (i < 0)
            {
                Name = fileName.Substring(0, fileName.Length);
                Path = "";
            }
            else
            {
                Name = fileName.Substring(i + 1, fileName.Length - (i + 1));
                Path = fileName.Substring(0, i);
            }
            if (string.IsNullOrEmpty(Path) && fileName.StartsWith("[app_code]"))
            {
                Name = fileName.Substring(10, fileName.Length - 10);
                Path = fileName.Substring(0, 10);
            }
            if (Name.ToLower() == "manifest.xml")
            {
                Type = InstallFileType.Manifest;
            }
            else
            {
                switch (Extension.ToLower())
                {
                    case "ascx":
                        Type = InstallFileType.Ascx;
                        break;
                    case "dll":
                        Type = InstallFileType.Assembly;
                        break;
                    case "dnn":
                    case "dnn5":
                    case "dnn6":
                    case "dnn7":
                        Type = InstallFileType.Manifest;
                        break;
                    case "resx":
                        Type = InstallFileType.Language;
                        break;
                    case "resources":
                    case "zip":
                        Type = InstallFileType.Resources;
                        break;
                    default:
                        if (Extension.ToLower().EndsWith("dataprovider") || Extension.ToLower() == "sql")
                        {
                            Type = InstallFileType.Script;
                        }
                        else if (Path.StartsWith("[app_code]"))
                        {
                            Type = InstallFileType.AppCode;
                        }
                        else
                        {
                            Type = FileTypeMatchRegex.IsMatch(Name) ? InstallFileType.CleanUp : InstallFileType.Other;
                        }
                        break;
                }
            }
			
            //remove [app_code] token
            Path = Path.Replace("[app_code]", "");

            //remove starting "\"
            if (Path.StartsWith("\\"))
            {
                Path = Path.Substring(1);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadZip method reads the zip stream and parses the ZipEntry metadata
        /// </summary>
        /// <param name="unzip">A ZipStream containing the file content</param>
        /// <param name="entry">A ZipEntry containing the file metadata</param>
        /// -----------------------------------------------------------------------------
        private void ReadZip(ZipInputStream unzip, ZipEntry entry)
        {
            ParseFileName(entry.Name);
            Util.WriteStream(unzip, TempFileName);
            File.SetLastWriteTime(TempFileName, entry.DateTime);
        }
		
		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The SetVersion method sets the version of the file
        /// </summary>
        /// <param name="version">The version of the file</param>
        /// -----------------------------------------------------------------------------
        public void SetVersion(Version version)
        {
            Version = version;
        }
		
		#endregion
    }
}
