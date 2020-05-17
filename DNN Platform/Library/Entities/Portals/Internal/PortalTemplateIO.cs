﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities.Internal;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Portals.Internal
{
    public class PortalTemplateIO : ServiceLocator<IPortalTemplateIO, PortalTemplateIO>, IPortalTemplateIO
    {
        protected override Func<IPortalTemplateIO> GetFactory()
        {
            return () => new PortalTemplateIO();
        }

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
            StreamReader reader = null;
            
            var retryable = new RetryableAction(
                () => reader = new StreamReader(File.Open(filePath, FileMode.Open)),
                filePath, 10, TimeSpan.FromMilliseconds(50), 2);

            retryable.TryIt();
            return reader;
        }

        private static string CheckFilePath(string path)
        {
            return File.Exists(path) ? path : string.Empty;
        }

        #endregion
    }
}
