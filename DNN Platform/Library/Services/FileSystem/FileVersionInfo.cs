﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
