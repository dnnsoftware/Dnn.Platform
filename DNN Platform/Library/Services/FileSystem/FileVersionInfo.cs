// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Xml.Serialization;
using DotNetNuke.Entities;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class FileVersionInfo : BaseEntityInfo
    {
        #region "Constructors"

        public FileVersionInfo()
        {
            Version = 1;
        }

        #endregion

        #region "Properties"

        [XmlElement("fileid")]
        public int FileId { get; set; }

        [XmlElement("version")]
        public int Version { get; set; }

        [XmlElement("filename")]
        public string FileName { get; set; }

        [XmlElement("contenttype")]
        public string ContentType { get; set; }

        [XmlElement("extension")]
        public string Extension { get; set; }
        
        [XmlElement("size")]
        public int Size { get; set; }

        [XmlElement("height")]
        public int Height { get; set; }

        [XmlElement("width")]
        public int Width { get; set; }

        [XmlElement("sha1hash")]
        public string SHA1Hash { get; set; }

        #endregion
    }
}
