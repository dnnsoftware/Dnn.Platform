#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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