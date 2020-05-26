// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

#endregion

namespace Dnn.PersonaBar.ConfigConsole.Components
{
    public class ConfigConsoleController
    {
        public IEnumerable<string> GetConfigFilesList()
        {
            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.config");
            IEnumerable<string> fileList = (from file in files select Path.GetFileName(file));
            return fileList;
        }

        public string GetConfigFile(string configFile)
        {
            ValidateFilePath(configFile);

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

        public void UpdateConfigFile(string fileName, string fileContent)
        {
            ValidateFilePath(fileName);

            var configDoc = new XmlDocument { XmlResolver = null };
            configDoc.LoadXml(fileContent);
            Config.Save(configDoc, fileName);
        }

        public void MergeConfigFile(string fileContent)
        {
            if (IsValidXmlMergDocument(fileContent))
            {
                var doc = new XmlDocument { XmlResolver = null };
                doc.LoadXml(fileContent);
                var app = DotNetNukeContext.Current.Application;
                var merge = new DotNetNuke.Services.Installer.XmlMerge(doc, Globals.FormatVersion(app.Version), app.Description);
                merge.UpdateConfigs();
            }
        }

        #region Private Functions

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

        #endregion
    }
}
