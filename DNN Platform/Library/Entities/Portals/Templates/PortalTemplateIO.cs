// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;

    /// <inheritdoc cref="IPortalTemplateIO" />
    [DnnDeprecated(9, 11, 1, "No replacement")]
    public partial class PortalTemplateIO : ServiceLocator<IPortalTemplateIO, PortalTemplateIO>, IPortalTemplateIO
    {
        /// <inheritdoc/>
        public IEnumerable<string> EnumerateTemplates()
        {
            string path = Globals.HostMapPath;
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.template").Where(x => Path.GetFileNameWithoutExtension(x) != "admin");
            }

            return [];
        }

        /// <inheritdoc/>
        public IEnumerable<string> EnumerateLanguageFiles()
        {
            string path = Globals.HostMapPath;
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.template.??-??.resx");
            }

            return [];
        }

        /// <inheritdoc/>
        public string GetResourceFilePath(string templateFilePath)
        {
            return CheckFilePath(templateFilePath + ".resources");
        }

        /// <inheritdoc/>
        public string GetLanguageFilePath(string templateFilePath, string cultureCode)
        {
            return CheckFilePath($"{templateFilePath}.{cultureCode}.resx");
        }

        /// <inheritdoc/>
        public TextReader OpenTextReader(string filePath)
        {
            StreamReader reader = null;

            var retryable = new RetryableAction(
                () => reader = new StreamReader(File.Open(filePath, FileMode.Open)),
                filePath,
                10,
                TimeSpan.FromMilliseconds(50),
                2);

            retryable.TryIt();
            return reader;
        }

        /// <inheritdoc/>
        public (string, List<string>) GetTemplateLanguages(string templateFilePath)
        {
            var defaultLanguage = string.Empty;
            var locales = new List<string>();
            var templateXml = new XmlDocument() { XmlResolver = null };
            templateXml.Load(templateFilePath);
            var node = templateXml.SelectSingleNode("//settings/defaultlanguage");
            if (node != null)
            {
                defaultLanguage = node.InnerText;
            }

            node = templateXml.SelectSingleNode("//locales");
            if (node != null)
            {
                foreach (XmlNode lnode in node.SelectNodes("//locale"))
                {
                    locales.Add(lnode.InnerText);
                }
            }

            return (defaultLanguage, locales);
        }

        /// <inheritdoc/>
        protected override Func<IPortalTemplateIO> GetFactory()
        {
            return () => new PortalTemplateIO();
        }

        private static string CheckFilePath(string path)
        {
            return File.Exists(path) ? path : string.Empty;
        }
    }
}
