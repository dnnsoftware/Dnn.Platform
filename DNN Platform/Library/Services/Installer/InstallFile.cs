#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
    /// <history>
    /// 	[cnurse]	07/24/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class InstallFile
    {
		#region "Private Members"

        private readonly InstallerInfo _InstallerInfo;
        private readonly string _SourceFileName;
        private string _Action;
        private TextEncoding _Encoding = TextEncoding.UTF8;
        private string _Name;
        private string _Path;
        private InstallFileType _Type;
        private Version _Version;

		#endregion

		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance from a ZipInputStream and a ZipEntry
        /// </summary>
        /// <remarks>The ZipInputStream is read into a byte array (Buffer), and the ZipEntry is used to
        /// set up the properties of the InstallFile class.</remarks>
        /// <param name="zip">The ZipInputStream</param>
        /// <param name="entry">The ZipEntry</param>
        /// <param name="info">An INstallerInfo instance</param>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public InstallFile(ZipInputStream zip, ZipEntry entry, InstallerInfo info)
        {
            _InstallerInfo = info;
            ReadZip(zip, entry);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The fileName of the File</param>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName)
        {
            ParseFileName(fileName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The fileName of the File</param>
        /// <param name="info">An INstallerInfo instance</param>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, InstallerInfo info)
        {
            ParseFileName(fileName);
            _InstallerInfo = info;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The fileName of the File</param>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="info">An INstallerInfo instance</param>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, string sourceFileName, InstallerInfo info)
        {
            ParseFileName(fileName);
            _SourceFileName = sourceFileName;
            _InstallerInfo = info;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallFile instance
        /// </summary>
        /// <param name="fileName">The file name of the File</param>
        /// <param name="filePath">The file path of the file</param>
        /// <history>
        /// 	[cnurse]	08/03/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public InstallFile(string fileName, string filePath)
        {
            _Name = fileName;
            _Path = filePath;
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Action for this file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	09/15/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Action
        {
            get
            {
                return _Action;
            }
            set
            {
                _Action = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the backup file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	08/02/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	08/02/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual string BackupPath
        {
            get
            {
                return System.IO.Path.Combine(InstallerInfo.TempInstallFolder, System.IO.Path.Combine("Backup", Path));
            }
        }

        public TextEncoding Encoding
        {
            get
            {
                return _Encoding;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the File Extension of the file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Extension
        {
            get
            {
                string ext = System.IO.Path.GetExtension(_Name);
                if (String.IsNullOrEmpty(ext))
                {
                    return "";
                }
                else
                {
                    return ext.Substring(1);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Full Name of the file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string FullName
        {
            get
            {
                return System.IO.Path.Combine(_Path, _Name);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated InstallerInfo
        /// </summary>
        /// <value>An InstallerInfo object</value>
        /// <history>
        /// 	[cnurse]	08/02/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public InstallerInfo InstallerInfo
        {
            get
            {
                return _InstallerInfo;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name of the file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Name
        {
            get
            {
                return _Name;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path of the file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Path
        {
            get
            {
                return _Path;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the source file name
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	01/29/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string SourceFileName
        {
            get
            {
                return _SourceFileName;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the temporary file
        /// </summary>
        /// <value>A string</value>
        /// <history>
        /// 	[cnurse]	08/02/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public InstallFileType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the file
        /// </summary>
        /// <value>A System.Version</value>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Version Version
        {
            get
            {
                return _Version;
            }
        }
		
		#endregion

		#region "Private Methods"

        private TextEncoding GetTextEncodingType(byte[] Buffer)
        {
            //UTF7 = No byte higher than 127
            //UTF8 = first three bytes EF BB BF
            //UTF16BigEndian = first two bytes FE FF
            //UTF16LittleEndian = first two bytes FF FE

            //Lets do the easy ones first
			if (Buffer[0] == 255 && Buffer[1] == 254)
            {
                return TextEncoding.UTF16LittleEndian;
            }
            if (Buffer[0] == 254 && Buffer[1] == 255)
            {
                return TextEncoding.UTF16BigEndian;
            }
            if (Buffer[0] == 239 && Buffer[1] == 187 && Buffer[2] == 191)
            {
                return TextEncoding.UTF8;
            }
			
            //This does a simple test to verify that there are no bytes with a value larger than 127
            //which would be invalid in UTF-7 encoding
            int i;
            for (i = 0; i <= 100; i++)
            {
                if (Buffer[i] > 127)
                {
                    return TextEncoding.Unknown;
                }
            }
            return TextEncoding.UTF7;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ParseFileName parses the ZipEntry metadata
        /// </summary>
        /// <param name="fileName">A String representing the file name</param>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void ParseFileName(string fileName)
        {
            int i = fileName.Replace("\\", "/").LastIndexOf("/");
            if (i < 0)
            {
                _Name = fileName.Substring(0, fileName.Length);
                _Path = "";
            }
            else
            {
                _Name = fileName.Substring(i + 1, fileName.Length - (i + 1));
                _Path = fileName.Substring(0, i);
            }
            if (string.IsNullOrEmpty(_Path) && fileName.StartsWith("[app_code]"))
            {
                _Name = fileName.Substring(10, fileName.Length - 10);
                _Path = fileName.Substring(0, 10);
            }
            if (_Name.ToLower() == "manifest.xml")
            {
                _Type = InstallFileType.Manifest;
            }
            else
            {
                switch (Extension.ToLower())
                {
                    case "ascx":
                        _Type = InstallFileType.Ascx;
                        break;
                    case "dll":
                        _Type = InstallFileType.Assembly;
                        break;
                    case "dnn":
                    case "dnn5":
                    case "dnn6":
                        _Type = InstallFileType.Manifest;
                        break;
                    case "resx":
                        _Type = InstallFileType.Language;
                        break;
                    case "resources":
                    case "zip":
                        _Type = InstallFileType.Resources;
                        break;
                    default:
                        if (Extension.ToLower().EndsWith("dataprovider") || Extension.ToLower() == "sql")
                        {
                            _Type = InstallFileType.Script;
                        }
                        else if (_Path.StartsWith("[app_code]"))
                        {
                            _Type = InstallFileType.AppCode;
                        }
                        else
                        {
                            if (Regex.IsMatch(_Name, Util.REGEX_Version + ".txt"))
                            {
                                _Type = InstallFileType.CleanUp;
                            }
                            else
                            {
                                _Type = InstallFileType.Other;
                            }
                        }
                        break;
                }
            }
			
            //remove [app_code] token
            _Path = _Path.Replace("[app_code]", "");

            //remove starting "\"
            if (_Path.StartsWith("\\"))
            {
                _Path = _Path.Substring(1);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadZip method reads the zip stream and parses the ZipEntry metadata
        /// </summary>
        /// <param name="unzip">A ZipStream containing the file content</param>
        /// <param name="entry">A ZipEntry containing the file metadata</param>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void ReadZip(ZipInputStream unzip, ZipEntry entry)
        {
            ParseFileName(entry.Name);
            Util.WriteStream(unzip, TempFileName);
            File.SetLastWriteTime(TempFileName, entry.DateTime);
        }
		
		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The SetVersion method sets the version of the file
        /// </summary>
        /// <param name="version">The version of the file</param>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void SetVersion(Version version)
        {
            _Version = version;
        }
		
		#endregion
    }
}
