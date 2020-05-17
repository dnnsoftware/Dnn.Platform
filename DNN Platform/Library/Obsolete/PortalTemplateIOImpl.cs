﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DotNetNuke.Common;

namespace DotNetNuke.Entities.Portals.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Deprecated in DotNetNuke 7.3.0. Use PortalTemplateIO. Scheduled removal in v10.0.0.")]
    public class PortalTemplateIOImpl : IPortalTemplateIO
    {
        #region IPortalTemplateIO Members

        public IEnumerable<string> EnumerateTemplates()
        {
            string path = Globals.HostMapPath;
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.template").Where(x => Path.GetFileNameWithoutExtension(x) != "admin");
            }

            return new string[0];
        }

        public IEnumerable<string> EnumerateLanguageFiles()
        {
            string path = Globals.HostMapPath;
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.template.??-??.resx");
            }

            return new string[0];
        }

        public string GetResourceFilePath(string templateFilePath)
        {
            return CheckFilePath(templateFilePath + ".resources");
        }

        public string GetLanguageFilePath(string templateFilePath, string cultureCode)
        {
            return CheckFilePath(string.Format("{0}.{1}.resx", templateFilePath, cultureCode));
        }

        public TextReader OpenTextReader(string filePath)
        {
            return new StreamReader(File.Open(filePath, FileMode.Open));
        }

        private static string CheckFilePath(string path)
        {
            if (File.Exists(path))
            {
                return path;
            }

            return "";
        }

        #endregion
    }
}
