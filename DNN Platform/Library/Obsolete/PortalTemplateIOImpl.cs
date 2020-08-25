// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Deprecated in DotNetNuke 7.3.0. Use PortalTemplateIO. Scheduled removal in v10.0.0.")]
    public class PortalTemplateIOImpl : IPortalTemplateIO
    {
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

            return string.Empty;
        }
    }
}
