// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Upgrade;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The XmlMerge class is a utility class for XmlSplicing config files.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class XmlMerge
    {
        private IDictionary<string, XmlDocument> _pendingDocuments;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMerge"/> class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceFileName"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(string sourceFileName, string version, string sender)
        {
            this.Version = version;
            this.Sender = sender;
            this.SourceConfig = new XmlDocument { XmlResolver = null };
            this.SourceConfig.Load(sourceFileName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMerge"/> class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceStream"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(Stream sourceStream, string version, string sender)
        {
            this.Version = version;
            this.Sender = sender;
            this.SourceConfig = new XmlDocument { XmlResolver = null };
            this.SourceConfig.Load(sourceStream);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMerge"/> class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceReader"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(TextReader sourceReader, string version, string sender)
        {
            this.Version = version;
            this.Sender = sender;
            this.SourceConfig = new XmlDocument { XmlResolver = null };
            this.SourceConfig.Load(sourceReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMerge"/> class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceDoc"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(XmlDocument sourceDoc, string version, string sender)
        {
            this.Version = version;
            this.Sender = sender;
            this.SourceConfig = sourceDoc;
        }

        public IDictionary<string, XmlDocument> PendingDocuments
        {
            get
            {
                if (this._pendingDocuments == null)
                {
                    this._pendingDocuments = new Dictionary<string, XmlDocument>();
                }

                return this._pendingDocuments;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Source for the Config file.
        /// </summary>
        /// <value>An XmlDocument.</value>
        /// -----------------------------------------------------------------------------
        public XmlDocument SourceConfig { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Sender (source) of the changes to be merged.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string Sender { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Target Config file.
        /// </summary>
        /// <value>An XmlDocument.</value>
        /// -----------------------------------------------------------------------------
        public XmlDocument TargetConfig { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the File Name of the Target Config file.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string TargetFileName { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the changes to be merged.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string Version { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the last update performed by this instance resulted in any changes.
        /// </summary>
        /// <value><c>true</c> if there were changes, <c>false</c> if no changes were made to the target document.</value>
        /// -----------------------------------------------------------------------------
        public bool ConfigUpdateChangedNodes { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UpdateConfig method processes the source file and updates the Target
        /// Config Xml Document.
        /// </summary>
        /// <param name="target">An Xml Document represent the Target Xml File.</param>
        /// -----------------------------------------------------------------------------
        public void UpdateConfig(XmlDocument target)
        {
            var changedAnyNodes = false;
            this.TargetConfig = target;
            if (this.TargetConfig != null)
            {
                changedAnyNodes = this.ProcessNodes(this.SourceConfig.SelectNodes("/configuration/nodes/node"), false);
            }

            this.ConfigUpdateChangedNodes = changedAnyNodes;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UpdateConfig method processes the source file and updates the Target
        /// Config file.
        /// </summary>
        /// <param name="target">An Xml Document represent the Target Xml File.</param>
        /// <param name="fileName">The fileName for the Target Xml File - relative to the webroot.</param>
        /// -----------------------------------------------------------------------------
        public void UpdateConfig(XmlDocument target, string fileName)
        {
            var changedAnyNodes = false;
            this.TargetFileName = fileName;
            this.TargetConfig = target;
            if (this.TargetConfig != null)
            {
                changedAnyNodes = this.ProcessNodes(this.SourceConfig.SelectNodes("/configuration/nodes/node"), true);
            }

            this.ConfigUpdateChangedNodes = changedAnyNodes;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UpdateConfigs method processes the source file and updates the various config
        /// files.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void UpdateConfigs()
        {
            this.UpdateConfigs(true);
        }

        public void UpdateConfigs(bool autoSave)
        {
            var changedAnyNodes = false;
            var nodes = this.SourceConfig.SelectNodes("/configuration/nodes");
            if (nodes != null)
            {
                foreach (XmlNode configNode in nodes)
                {
                    Debug.Assert(configNode.Attributes != null, "configNode.Attributes != null");

                    // Attempt to load TargetFile property from configFile Atribute
                    this.TargetFileName = configNode.Attributes["configfile"].Value;
                    string targetProductName = string.Empty;
                    if (configNode.Attributes["productName"] != null)
                    {
                        targetProductName = configNode.Attributes["productName"].Value;
                    }

                    bool isAppliedToProduct;

                    if (!File.Exists(Globals.ApplicationMapPath + "\\" + this.TargetFileName))
                    {
                        DnnInstallLogger.InstallLogInfo($"Target File {this.TargetFileName} doesn't exist, ignore the merge process");
                        return;
                    }

                    this.TargetConfig = Config.Load(this.TargetFileName);
                    if (string.IsNullOrEmpty(targetProductName) || targetProductName == "All")
                    {
                        isAppliedToProduct = true;
                    }
                    else
                    {
                        isAppliedToProduct = DotNetNukeContext.Current.Application.ApplyToProduct(targetProductName);
                    }

                    // The nodes definition is not correct so skip changes
                    if (this.TargetConfig != null && isAppliedToProduct)
                    {
                        var changedNodes = this.ProcessNodes(configNode.SelectNodes("node"), autoSave);
                        changedAnyNodes = changedAnyNodes || changedNodes;
                        if (!autoSave && changedNodes)
                        {
                            this.PendingDocuments.Add(this.TargetFileName, this.TargetConfig);
                        }
                    }
                }
            }

            this.ConfigUpdateChangedNodes = changedAnyNodes;
        }

        public void SavePendingConfigs()
        {
            foreach (var key in this.PendingDocuments.Keys)
            {
                Config.Save(this.PendingDocuments[key], key);
            }
        }

        private bool AddNode(XmlNode rootNode, XmlNode actionNode)
        {
            var changedNode = false;
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.AppendChild(this.TargetConfig.ImportNode(child, true));
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddNode:" + child.InnerXml.ToString());
                    changedNode = true;
                }
            }

            return changedNode;
        }

        private bool PrependNode(XmlNode rootNode, XmlNode actionNode)
        {
            var changedNode = false;
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.PrependChild(this.TargetConfig.ImportNode(child, true));
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "PrependNode:" + child.InnerXml.ToString());
                    changedNode = true;
                }
            }

            return changedNode;
        }

        private bool InsertNode(XmlNode childRootNode, XmlNode actionNode, NodeInsertType mode)
        {
            XmlNode rootNode = childRootNode.ParentNode;
            Debug.Assert(rootNode != null);

            var changedNode = false;
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InsertNode:" + child.InnerXml.ToString());
                    switch (mode)
                    {
                        case NodeInsertType.Before:
                            rootNode.InsertBefore(this.TargetConfig.ImportNode(child, true), childRootNode);
                            changedNode = true;
                            break;
                        case NodeInsertType.After:
                            rootNode.InsertAfter(this.TargetConfig.ImportNode(child, true), childRootNode);
                            changedNode = true;
                            break;
                    }
                }
            }

            return changedNode;
        }

        private bool ProcessNode(XmlNode node, XmlNode targetRoot)
        {
            Debug.Assert(node.Attributes != null, "node.Attributes != null");

            XmlNode rootNode = this.FindMatchingNode(targetRoot, node, "path");

            string nodeAction = node.Attributes["action"].Value.ToLowerInvariant();

            if (rootNode == null)
            {
                return false; // not every TargetRoot will contain every target node
            }

            switch (nodeAction)
            {
                case "add":
                    return this.AddNode(rootNode, node);
                case "prepend":
                    return this.PrependNode(rootNode, node);
                case "insertbefore":
                    return this.InsertNode(rootNode, node, NodeInsertType.Before);
                case "insertafter":
                    return this.InsertNode(rootNode, node, NodeInsertType.After);
                case "remove":
                    return this.RemoveNode(rootNode);
                case "removeattribute":
                    return this.RemoveAttribute(rootNode, node);
                case "update":
                    return this.UpdateNode(rootNode, node);
                case "updateattribute":
                    return this.UpdateAttribute(rootNode, node);
                default:
                    return false;
            }
        }

        private XmlNode FindNode(XmlNode root, string rootNodePath, XmlNamespaceManager nsmgr)
        {
            rootNodePath = this.AdjustRootNodePathRelativeToLocationElements(root, rootNodePath);
            return root.SelectSingleNode(rootNodePath, nsmgr);
        }

        private XmlNode FindNode(XmlNode root, string rootNodePath)
        {
            rootNodePath = this.AdjustRootNodePathRelativeToLocationElements(root, rootNodePath);
            return root.SelectSingleNode(rootNodePath);
        }

        private string AdjustRootNodePathRelativeToLocationElements(XmlNode root, string rootNodePath)
        {
            if (root.Name != "location")
            {
                return rootNodePath;
            }

            var index = rootNodePath.IndexOf("configuration");
            var adjustedPath = rootNodePath.Substring(index + "configuration".Length);
            adjustedPath = adjustedPath.TrimStart(new[] { '/' });
            if (string.IsNullOrEmpty(adjustedPath))
            {
                adjustedPath = ".";
            }

            return adjustedPath;
        }

        private bool ProcessNodes(XmlNodeList nodes, bool saveConfig)
        {
            var changedNodes = false;

            // The nodes definition is not correct so skip changes
            if (this.TargetConfig != null)
            {
                // in web.config it is possible to add <location> nodes that contain nodes that would
                // otherwise be in the root <configuration> node, therefore some files can have multiple roots
                // making it tricky to decide where to apply the xml merge operations
                var targetRoots = this.GetTargetRoots().ToList();

                if (targetRoots.Count == 1)
                {
                    var root = targetRoots[0];
                    foreach (XmlNode node in nodes)
                    {
                        changedNodes = this.ProcessNode(node, root) || changedNodes;
                    }
                }
                else
                {
                    foreach (XmlNode node in nodes)
                    {
                        var hits = this.FindMatchingNodes(node, targetRoots, "path").ToList();

                        if (hits.Count == 0)
                        {
                            continue;
                        }

                        if (hits.Count == 1)
                        {
                            changedNodes = this.ProcessNode(node, hits[0]) || changedNodes;
                        }
                        else if (hits.Count < targetRoots.Count)
                        {
                            changedNodes = this.ProcessNode(node, hits[0]) || changedNodes;
                        }
                        else
                        {
                            // hit on all roots
                            XmlNode hit = this.FindMatchingNodes(node, hits, "targetpath").FirstOrDefault();
                            if (hit != null)
                            {
                                changedNodes = this.ProcessNode(node, hit) || changedNodes;
                            }
                            else
                            {
                                // all paths match at root level but no targetpaths match below that so default to the initial root
                                changedNodes = this.ProcessNode(node, hits[0]) || changedNodes;
                            }
                        }
                    }
                }

                if (saveConfig && changedNodes)
                {
                    Config.Save(this.TargetConfig, this.TargetFileName);
                }
            }

            return changedNodes;
        }

        private IEnumerable<XmlNode> FindMatchingNodes(XmlNode mergeNode, IEnumerable<XmlNode> rootNodes, string pathAttributeName)
        {
            foreach (var targetRoot in rootNodes)
            {
                var rootNode = this.FindMatchingNode(targetRoot, mergeNode, pathAttributeName);

                if (rootNode != null)
                {
                    yield return targetRoot;
                }
            }
        }

        private XmlNode FindMatchingNode(XmlNode rootNode, XmlNode mergeNode, string pathAttributeName)
        {
            Debug.Assert(mergeNode.Attributes != null);

            XmlNode matchingNode = null;
            if (mergeNode.Attributes[pathAttributeName] != null)
            {
                string rootNodePath = mergeNode.Attributes[pathAttributeName].Value;
                if (mergeNode.Attributes["nameSpace"] == null)
                {
                    matchingNode = this.FindNode(rootNode, rootNodePath);
                }
                else
                {
                    // Use Namespace Manager
                    string xmlNameSpace = mergeNode.Attributes["nameSpace"].Value;
                    string xmlNameSpacePrefix = mergeNode.Attributes["nameSpacePrefix"].Value;
                    var nsmgr = new XmlNamespaceManager(this.TargetConfig.NameTable);
                    nsmgr.AddNamespace(xmlNameSpacePrefix, xmlNameSpace);
                    matchingNode = this.FindNode(rootNode, rootNodePath, nsmgr);
                }
            }

            return matchingNode;
        }

        private IEnumerable<XmlNode> GetTargetRoots()
        {
            yield return this.TargetConfig.DocumentElement;

            var locations = this.TargetConfig.SelectNodes("/configuration/location");
            if (locations != null)
            {
                foreach (XmlNode node in locations)
                {
                    yield return node;
                }
            }
        }

        private bool RemoveAttribute(XmlNode rootNode, XmlNode actionNode)
        {
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");

            var changedNode = false;
            if (actionNode.Attributes["name"] != null)
            {
                string attributeName = actionNode.Attributes["name"].Value;
                if (!string.IsNullOrEmpty(attributeName))
                {
                    if (rootNode.Attributes[attributeName] != null)
                    {
                        rootNode.Attributes.Remove(rootNode.Attributes[attributeName]);
                        DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveAttribute:attributeName=" + attributeName.ToString());
                        changedNode = true;
                    }
                }
            }

            return changedNode;
        }

        private bool RemoveNode(XmlNode node)
        {
            var changedNode = false;
            if (node != null)
            {
                // Get Parent
                XmlNode parentNode = node.ParentNode;

                // Remove current Node
                if (parentNode != null)
                {
                    parentNode.RemoveChild(node);
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveNode:" + node.InnerXml.ToString());
                    changedNode = true;
                }
            }

            return changedNode;
        }

        private bool UpdateAttribute(XmlNode rootNode, XmlNode actionNode)
        {
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");

            var changedNode = false;
            if (actionNode.Attributes["name"] != null && actionNode.Attributes["value"] != null)
            {
                string attributeName = actionNode.Attributes["name"].Value;
                string attributeValue = actionNode.Attributes["value"].Value;
                if (!string.IsNullOrEmpty(attributeName))
                {
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateAttribute:attributeName=" + attributeName.ToString());
                    if (rootNode.Attributes[attributeName] == null)
                    {
                        rootNode.Attributes.Append(this.TargetConfig.CreateAttribute(attributeName));
                        changedNode = true;
                    }

                    var oldAttributeValue = rootNode.Attributes[attributeName].Value;
                    rootNode.Attributes[attributeName].Value = attributeValue;
                    if (!string.Equals(oldAttributeValue, attributeValue, StringComparison.Ordinal))
                    {
                        changedNode = true;
                    }
                }
            }

            return changedNode;
        }

        private bool UpdateNode(XmlNode rootNode, XmlNode actionNode)
        {
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");

            var changedNode = false;
            string keyAttribute = string.Empty;
            if (actionNode.Attributes["key"] != null)
            {
                keyAttribute = actionNode.Attributes["key"].Value;
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateNode: keyAttribute=" + keyAttribute.ToString());
            }

            foreach (XmlNode child in actionNode.ChildNodes)
            {
                Debug.Assert(child.Attributes != null, "child.Attributes != null");

                if (child.NodeType == XmlNodeType.Element)
                {
                    XmlNode targetNode = null;
                    if (!string.IsNullOrEmpty(keyAttribute))
                    {
                        if (child.Attributes[keyAttribute] != null)
                        {
                            string path = string.Format("{0}[@{1}='{2}']", child.LocalName, keyAttribute, child.Attributes[keyAttribute].Value);
                            targetNode = rootNode.SelectSingleNode(path);
                        }
                    }
                    else
                    {
                        targetNode = this.FindMatchingNode(rootNode, actionNode, "targetpath");
                    }

                    if (targetNode == null)
                    {
                        // Since there is no collision we can just add the node
                        rootNode.AppendChild(this.TargetConfig.ImportNode(child, true));
                        changedNode = true;
                        continue;
                    }

                    // There is a collision so we need to determine what to do.
                    string collisionAction = actionNode.Attributes["collision"].Value;
                    switch (collisionAction.ToLowerInvariant())
                    {
                        case "overwrite":
                            var oldContent = rootNode.InnerXml;
                            rootNode.ReplaceChild(this.TargetConfig.ImportNode(child, true), targetNode);
                            var newContent = rootNode.InnerXml;
                            changedNode = !string.Equals(oldContent, newContent, StringComparison.Ordinal);
                            break;
                        case "save":
                            string commentHeaderText = string.Format(
                                Localization.GetString("XMLMERGE_Upgrade", Localization.SharedResourceFile),
                                Environment.NewLine,
                                this.Sender,
                                this.Version,
                                DateTime.Now);
                            XmlComment commentHeader = this.TargetConfig.CreateComment(commentHeaderText);

                            var targetNodeContent = this.GetNodeContentWithoutComment(targetNode);
                            XmlComment commentNode = this.TargetConfig.CreateComment(targetNodeContent);
                            var newChild = this.TargetConfig.ImportNode(child, true);
                            rootNode.ReplaceChild(newChild, targetNode);
                            rootNode.InsertBefore(commentHeader, newChild);
                            rootNode.InsertBefore(commentNode, newChild);
                            changedNode = true;
                            break;
                        case "ignore":
                            break;
                    }
                }
            }

            return changedNode;
        }

        private string GetNodeContentWithoutComment(XmlNode node)
        {
            var cloneNode = node.Clone();
            this.RemoveCommentNodes(cloneNode);

            return cloneNode.OuterXml;
        }

        private void RemoveCommentNodes(XmlNode node)
        {
            var commentNodes = new List<XmlNode>();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Comment)
                {
                    commentNodes.Add(childNode);
                }
                else if (childNode.HasChildNodes)
                {
                    this.RemoveCommentNodes(childNode);
                }
            }

            if (commentNodes.Count > 0)
            {
                commentNodes.ForEach(n => { node.RemoveChild(n); });
            }
        }
    }
}
