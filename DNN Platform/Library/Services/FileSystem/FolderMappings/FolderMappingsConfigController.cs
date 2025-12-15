// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem.FolderMappings;

    public class FolderMappingsConfigController : ServiceLocator<IFolderMappingsConfigController, FolderMappingsConfigController>, IFolderMappingsConfigController
    {
        private const string ConfigNodeValue = "folderMappingsSettings";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderMappingsConfigController));
        private static readonly string DefaultConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DotNetNuke.folderMappings.config");

        /// <summary>Initializes a new instance of the <see cref="FolderMappingsConfigController"/> class.</summary>
        public FolderMappingsConfigController()
        {
            this.FolderMappings = new Dictionary<string, string>();
            this.FolderTypes = new List<FolderTypeConfig>();
            this.LoadConfig();
        }

        /// <inheritdoc/>
        public string ConfigNode
        {
            get { return ConfigNodeValue; }
        }

        /// <inheritdoc/>
        public IList<FolderTypeConfig> FolderTypes { get; internal set; }

        private Dictionary<string, string> FolderMappings { get; set; }

        /// <inheritdoc/>
        public void LoadConfig()
        {
            try
            {
                if (File.Exists(DefaultConfigFilePath))
                {
                    var configDocument = new XmlDocument { XmlResolver = null };
                    configDocument.Load(DefaultConfigFilePath);
                    this.FillFolderMappings(configDocument);
                    this.FillFolderTypes(configDocument);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <inheritdoc/>
        public void SaveConfig(string folderMappinsSettings)
        {
            if (!File.Exists(DefaultConfigFilePath))
            {
                var folderMappingsConfigContent = "<" + this.ConfigNode + ">" + folderMappinsSettings + "</" + this.ConfigNode + ">";
                File.AppendAllText(DefaultConfigFilePath, folderMappingsConfigContent);
                var configDocument = new XmlDocument { XmlResolver = null };
                configDocument.LoadXml(folderMappingsConfigContent);
                this.FillFolderMappings(configDocument);
                this.FillFolderTypes(configDocument);
            }
        }

        /// <inheritdoc/>
        public FolderMappingInfo GetFolderMapping(int portalId, string folderPath)
        {
            if (!this.FolderMappings.TryGetValue(folderPath, out var mapping))
            {
                return null;
            }

            return FolderMappingController.Instance.GetFolderMapping(portalId, mapping);
        }

        /// <inheritdoc/>
        protected override Func<IFolderMappingsConfigController> GetFactory()
        {
            return () => new FolderMappingsConfigController();
        }

        private static FolderTypeConfig GetFolderMappingFromConfigNode(XmlNode node)
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
                        Encrypt = !string.IsNullOrEmpty(encryptValue) && bool.Parse(encryptValue),
                    });
                }

                folderType.Settings = settings;
            }

            return folderType;
        }

        private void FillFolderMappings(XmlDocument configDocument)
        {
            var folderMappingsNode = configDocument.SelectSingleNode(this.ConfigNode + "/folderMappings");
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
            var folderTypesNode = configDocument.SelectSingleNode(this.ConfigNode + "/folderTypes");
            if (folderTypesNode == null)
            {
                return;
            }

            this.FolderTypes.Clear();
            foreach (XmlNode folderTypeNode in folderTypesNode)
            {
                this.FolderTypes.Add(GetFolderMappingFromConfigNode(folderTypeNode));
            }
        }
    }
}
