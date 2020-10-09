﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.ConfigConsole.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    public class ConfigConsoleController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConfigConsoleController));
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
                var doc = File.ReadAllText(Path.Combine(Globals.ApplicationMapPath, "\\", configFile));
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
                SaveNonConfig(fileContent, fileName);
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

        private static string SaveNonConfig(string document, string filename)
        {
            var retMsg = string.Empty;
            try
            {
                var strFilePath = Path.Combine(Globals.ApplicationMapPath, "\\", filename);
                var existingFileAttributes = FileAttributes.Normal;
                if (File.Exists(strFilePath))
                {
                    // save current file attributes
                    existingFileAttributes = File.GetAttributes(strFilePath);

                    // change to normal ( in case it is flagged as read-only )
                    File.SetAttributes(strFilePath, FileAttributes.Normal);
                }

                // Attempt a few times in case the file was locked; occurs during modules' installation due
                // to application restarts where IIS can overlap old application shutdown and new one start.
                const int maxRetires = 4;
                const double miltiplier = 2.5;
                for (var retry = maxRetires; retry >= 0; retry--)
                {
                    try
                    {
                        // save the config file
                        File.WriteAllText(strFilePath, document);

                        break;
                    }
                    catch (IOException exc)
                    {
                        if (retry == 0)
                        {
                            Logger.Error(exc);
                            retMsg = exc.Message;
                        }

                        // try incremental delay; maybe the file lock is released by then
                        Thread.Sleep((int)(miltiplier * (maxRetires - retry + 1)) * 1000);
                    }
                }

                // reset file attributes
                File.SetAttributes(strFilePath, existingFileAttributes);
            }
            catch (Exception exc)
            {
                // the file permissions may not be set properly
                Logger.Error(exc);
                retMsg = exc.Message;
            }

            return retMsg;
        }
    }
}
