// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.ConfigConsole.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;

    public class ConfigConsoleController
    {
        private const string CONFIG_EXT = ".config";
        private const string ROBOTS_EXT = "robots.txt";  // in multi-portal instances, there may be multiple robots.txt files (e.g., site1.com.robots.txt, site2.com.robots.txt, etc.)

        public IEnumerable<string> GetConfigFilesList()
        {
            var files = Directory
                .EnumerateFiles(Globals.ApplicationMapPath) 
                .Where(file => file.ToLower().EndsWith(CONFIG_EXT, StringComparison.InvariantCultureIgnoreCase) || file.ToLower().EndsWith(ROBOTS_EXT, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            IEnumerable<string> fileList = (from file in files select Path.GetFileName(file));
            return fileList;
        }

        public string GetConfigFile(string configFile)
        {
            this.ValidateFilePath(configFile);

            if (configFile.EndsWith(CONFIG_EXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configDoc = Config.Load(configFile);
                using (var txtWriter = new StringWriter())
                {
                    using (var writer = new XmlTextWriter(txtWriter))
                    {
                        writer.Formatting = Formatting.Indented;
                        configDoc.WriteTo(writer);
                    }

                    return txtWriter.ToString();
                }
            }
            else
            {
                var doc = Config.LoadNonConfig(configFile);
                return doc;
            }
        }

        public void UpdateConfigFile(string fileName, string fileContent)
        {
            this.ValidateFilePath(fileName);

            if (fileName.EndsWith(CONFIG_EXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configDoc = new XmlDocument {XmlResolver = null};
                configDoc.LoadXml(fileContent);
                Config.Save(configDoc, fileName);
            }
            else
            {
                Config.SaveNonConfig(fileContent, fileName);
            }
        }

        public void MergeConfigFile(string fileContent)
        {
            if (this.IsValidXmlMergDocument(fileContent))
            {
                var doc = new XmlDocument { XmlResolver = null };
                doc.LoadXml(fileContent);
                var app = DotNetNukeContext.Current.Application;
                var merge = new DotNetNuke.Services.Installer.XmlMerge(doc, Globals.FormatVersion(app.Version), app.Description);
                merge.UpdateConfigs();
            }
        }

        private bool IsValidXmlMergDocument(string mergeDocText)
        {
            if (string.IsNullOrEmpty(mergeDocText.Trim()))
            {
                return false;
            }
            //TODO: Add more checks here
            return true;
        }

        private void ValidateFilePath(string filePath)
        {
            var physicalPath = Path.Combine(Globals.ApplicationMapPath, filePath);
            var fileInfo = new FileInfo(physicalPath);
            if (!fileInfo.DirectoryName.StartsWith(Globals.ApplicationMapPath, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Invalid File Path");
            }
        }
    }
}
