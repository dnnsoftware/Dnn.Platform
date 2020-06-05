﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem.FolderMappings;

namespace DotNetNuke.Services.FileSystem
{
    public class FolderMappingsConfigController: ServiceLocator<IFolderMappingsConfigController, FolderMappingsConfigController>, IFolderMappingsConfigController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderMappingsConfigController));
        private static readonly string defaultConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DotNetNuke.folderMappings.config");
        #region Constructor
        public FolderMappingsConfigController()
        {
            this.FolderMappings = new Dictionary<string, string>();
            this.FolderTypes = new List<FolderTypeConfig>();    
            this.LoadConfig();     
        }
        #endregion

        #region private methods
        private IDictionary<string, string> FolderMappings { get; set; }

        private void FillFolderMappings(XmlDocument configDocument)
        {
            var folderMappingsNode = configDocument.SelectSingleNode(this.ConfigNode+"/folderMappings");            
            if (folderMappingsNode == null)
            {
                return;
            }
            this.FolderMappings.Clear();
            foreach (XmlNode folderMappingNode in folderMappingsNode)
            {
                this.FolderMappings.Add(XmlUtils.GetNodeValue(folderMappingNode, "folderPath"), XmlUtils.GetNodeValue(folderMappingNode, "folderTypeName"));
            }
        }

        private void FillFolderTypes(XmlDocument configDocument)
        {
            var folderTypesNode = configDocument.SelectSingleNode(this.ConfigNode+"/folderTypes");
            if (folderTypesNode == null)
            {
                return;
            }
            this.FolderTypes.Clear();
            foreach (XmlNode folderTypeNode in folderTypesNode)
            {
                this.FolderTypes.Add(this.GetFolderMappingFromConfigNode(folderTypeNode));
            }
        }

        private FolderTypeConfig GetFolderMappingFromConfigNode(XmlNode node)
        {
            var nodeNavigator = node.CreateNavigator();
            var folderType = new FolderTypeConfig()
            {
                Name = XmlUtils.GetAttributeValue(nodeNavigator, "name"),
                Provider = XmlUtils.GetNodeValue(nodeNavigator, "provider"),                
            };
            XmlNodeList settingsNode = node.SelectNodes("settings/setting");
            if (settingsNode != null)
            {
                var settings = new List<FolderTypeSettingConfig>();
                foreach (XmlNode settingNode in settingsNode)
                {
                    var encryptValue = XmlUtils.GetAttributeValue(settingNode.CreateNavigator(), "encrypt");
                    settings.Add(new FolderTypeSettingConfig
                    {
                        Name = XmlUtils.GetAttributeValue(settingNode.CreateNavigator(), "name"),
                        Value = settingNode.InnerText,
                        Encrypt = !String.IsNullOrEmpty(encryptValue) && Boolean.Parse(encryptValue)
                    });
                }
                folderType.Settings = settings;
            }

            return folderType;
        }
        #endregion

        #region public Properties
        public IList<FolderTypeConfig> FolderTypes { get; internal set; } 
        
        private const string configNode = "folderMappingsSettings";
        public string ConfigNode
        {
            get { return configNode; }
        }
        #endregion

        #region public Methods
        public void LoadConfig()
        {
            try
            {                
                if (File.Exists(defaultConfigFilePath))
                {
                    var configDocument = new XmlDocument { XmlResolver = null };
                    configDocument.Load(defaultConfigFilePath);
                    this.FillFolderMappings(configDocument);
                    this.FillFolderTypes(configDocument);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void SaveConfig(string folderMappinsSettings)
        {            
            if (!File.Exists(defaultConfigFilePath))
            {
                var folderMappingsConfigContent = "<" + this.ConfigNode + ">" + folderMappinsSettings + "</" + this.ConfigNode +">";
                File.AppendAllText(defaultConfigFilePath, folderMappingsConfigContent);
                var configDocument = new XmlDocument { XmlResolver = null };
                configDocument.LoadXml(folderMappingsConfigContent);
                this.FillFolderMappings(configDocument);
                this.FillFolderTypes(configDocument);
            }
        }

        public FolderMappingInfo GetFolderMapping(int portalId, string folderPath)
        {
            if (!this.FolderMappings.ContainsKey(folderPath))
            {
                return null;
            }
            return FolderMappingController.Instance.GetFolderMapping(portalId, this.FolderMappings[folderPath]);
        }
        #endregion

        protected override Func<IFolderMappingsConfigController> GetFactory()
        {
            return () => new FolderMappingsConfigController();
        }
    }
}
