// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Xml.Serialization;

    using DotNetNuke.Entities;

    [Serializable]
    public class FileVersionInfo : BaseEntityInfo
    {
        public FileVersionInfo()
        {
            this.Version = 1;
        }

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
    }
}
