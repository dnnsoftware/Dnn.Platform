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
            FolderMappings = new Dictionary<string, string>();
            FolderTypes = new List<FolderTypeConfig>();    
            LoadConfig();     
        }
        #endregion

        #region private methods
        private IDictionary<string, string> FolderMappings { get; set; }

        private void FillFolderMappings(XmlDocument configDocument)
        {
            var folderMappingsNode = configDocument.SelectSingleNode(ConfigNode+"/folderMappings");            
            if (folderMappingsNode == null)
            {
                return;
            }
            FolderMappings.Clear();
            foreach (XmlNode folderMappingNode in folderMappingsNode)
            {
                FolderMappings.Add(XmlUtils.GetNodeValue(folderMappingNode, "folderPath"), XmlUtils.GetNodeValue(folderMappingNode, "folderTypeName"));
            }
        }

        private void FillFolderTypes(XmlDocument configDocument)
        {
            var folderTypesNode = configDocument.SelectSingleNode(ConfigNode+"/folderTypes");
            if (folderTypesNode == null)
            {
                return;
            }
            FolderTypes.Clear();
            foreach (XmlNode folderTypeNode in folderTypesNode)
            {
                FolderTypes.Add(GetFolderMappingFromConfigNode(folderTypeNode));
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
                    var configDocument = new XmlDocument();
                    configDocument.Load(defaultConfigFilePath);
                    FillFolderMappings(configDocument);
                    FillFolderTypes(configDocument);
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
                var folderMappingsConfigContent = "<" + ConfigNode + ">" + folderMappinsSettings + "</" + ConfigNode +">";
                File.AppendAllText(defaultConfigFilePath, folderMappingsConfigContent);
                var configDocument = new XmlDocument();
                configDocument.LoadXml(folderMappingsConfigContent);
                FillFolderMappings(configDocument);
                FillFolderTypes(configDocument);
            }
        }

        public FolderMappingInfo GetFolderMapping(int portalId, string folderPath)
        {
            if (!FolderMappings.ContainsKey(folderPath))
            {
                return null;
            }
            return FolderMappingController.Instance.GetFolderMapping(portalId, FolderMappings[folderPath]);
        }
        #endregion

        protected override Func<IFolderMappingsConfigController> GetFactory()
        {
            return () => new FolderMappingsConfigController();
        }
    }
}