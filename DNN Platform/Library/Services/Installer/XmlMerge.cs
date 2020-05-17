﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

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

#endregion

namespace DotNetNuke.Services.Installer
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The XmlMerge class is a utility class for XmlSplicing config files
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class XmlMerge
    {
        #region Private Properties

        private IDictionary<string, XmlDocument> _pendingDocuments;

        #endregion
        
        #region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the XmlMerge class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceFileName"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(string sourceFileName, string version, string sender)
        {
            Version = version;
            Sender = sender;
            SourceConfig = new XmlDocument { XmlResolver = null };
            SourceConfig.Load(sourceFileName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the XmlMerge class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceStream"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(Stream sourceStream, string version, string sender)
        {
            Version = version;
            Sender = sender;
            SourceConfig = new XmlDocument { XmlResolver = null };
            SourceConfig.Load(sourceStream);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the XmlMerge class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceReader"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(TextReader sourceReader, string version, string sender)
        {
            Version = version;
            Sender = sender;
            SourceConfig = new XmlDocument { XmlResolver = null };
            SourceConfig.Load(sourceReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the XmlMerge class.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="sender"></param>
        /// <param name="sourceDoc"></param>
        /// -----------------------------------------------------------------------------
        public XmlMerge(XmlDocument sourceDoc, string version, string sender)
        {
            Version = version;
            Sender = sender;
            SourceConfig = sourceDoc;
        }
		
		#endregion

        #region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Source for the Config file
        /// </summary>
        /// <value>An XmlDocument</value>
        /// -----------------------------------------------------------------------------
        public XmlDocument SourceConfig { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Sender (source) of the changes to be merged
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Sender { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Target Config file
        /// </summary>
        /// <value>An XmlDocument</value>
        /// -----------------------------------------------------------------------------
        public XmlDocument TargetConfig { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the File Name of the Target Config file
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string TargetFileName { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the changes to be merged
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Version { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the last update performed by this instance resulted in any changes
        /// </summary>
        /// <value><c>true</c> if there were changes, <c>false</c> if no changes were made to the target document</value>
        /// -----------------------------------------------------------------------------
        public bool ConfigUpdateChangedNodes { get; private set; }

        public IDictionary<string, XmlDocument> PendingDocuments
        {
            get
            {
                if (_pendingDocuments == null)
                {
                    _pendingDocuments = new Dictionary<string, XmlDocument>();
                }

                return _pendingDocuments;
            }
        }

        #endregion

		#region "Private Methods"

        private bool AddNode(XmlNode rootNode, XmlNode actionNode)
        {
            var changedNode = false;
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.AppendChild(TargetConfig.ImportNode(child, true));
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddNode:" + child.InnerXml.ToString());
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
                    rootNode.PrependChild(TargetConfig.ImportNode(child, true));
                    DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "PrependNode:" + child.InnerXml.ToString());
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
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InsertNode:" + child.InnerXml.ToString());
                    switch (mode)
                    {
                        case NodeInsertType.Before:
                            rootNode.InsertBefore(TargetConfig.ImportNode(child, true), childRootNode);
                            changedNode = true;
                            break;
                        case NodeInsertType.After:
                            rootNode.InsertAfter(TargetConfig.ImportNode(child, true), childRootNode);
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

            XmlNode rootNode = FindMatchingNode(targetRoot, node, "path");
            
            string nodeAction = node.Attributes["action"].Value.ToLowerInvariant();

			if (rootNode == null)
			{
			    return false; //not every TargetRoot will contain every target node
			}
            
            switch (nodeAction)
            {
                case "add":
                    return AddNode(rootNode, node);
                case "prepend":
                    return PrependNode(rootNode, node);
                case "insertbefore":
                    return InsertNode(rootNode, node, NodeInsertType.Before);
                case "insertafter":
                    return InsertNode(rootNode, node, NodeInsertType.After);
                case "remove":
                    return RemoveNode(rootNode);
                case "removeattribute":
                    return RemoveAttribute(rootNode, node);
                case "update":
                    return UpdateNode(rootNode, node);
                case "updateattribute":
                    return UpdateAttribute(rootNode, node);
                default:
                    return false;
            }
        }

        private XmlNode FindNode(XmlNode root, string rootNodePath, XmlNamespaceManager nsmgr)
        {
            rootNodePath = AdjustRootNodePathRelativeToLocationElements(root, rootNodePath);
            return root.SelectSingleNode(rootNodePath, nsmgr);
        }

        private XmlNode FindNode(XmlNode root, string rootNodePath)
        {
            rootNodePath = AdjustRootNodePathRelativeToLocationElements(root, rootNodePath);
            return root.SelectSingleNode(rootNodePath);
        }

        private string AdjustRootNodePathRelativeToLocationElements(XmlNode root, string rootNodePath)
        {
            if(root.Name != "location")
            {
                return rootNodePath;
            }

            var index = rootNodePath.IndexOf("configuration");
            var adjustedPath = rootNodePath.Substring(index + "configuration".Length);
            adjustedPath = adjustedPath.TrimStart(new[] {'/'});
            if(String.IsNullOrEmpty(adjustedPath))
            {
                adjustedPath = ".";
            }
            
            return adjustedPath;
        }

        private bool ProcessNodes(XmlNodeList nodes, bool saveConfig)
        {
            var changedNodes = false;

            //The nodes definition is not correct so skip changes
            if (TargetConfig != null)
            {
                //in web.config it is possible to add <location> nodes that contain nodes that would
                //otherwise be in the root <configuration> node, therefore some files can have multiple roots
                //making it tricky to decide where to apply the xml merge operations
                var targetRoots = GetTargetRoots().ToList();

                if(targetRoots.Count == 1)
                {
                    var root = targetRoots[0];
                    foreach (XmlNode node in nodes)
                    {
                        changedNodes = ProcessNode(node, root) || changedNodes;
                    }
                }
                else
                {
                    foreach (XmlNode node in nodes)
                    {
                        var hits = FindMatchingNodes(node, targetRoots, "path").ToList();

                        if(hits.Count == 0)
                        {
                            continue;
                        }
                        
                        if(hits.Count == 1)
                        {
                            changedNodes = ProcessNode(node, hits[0]) || changedNodes;
                        }
                        else if(hits.Count < targetRoots.Count)
                        {
                            changedNodes = ProcessNode(node, hits[0]) || changedNodes;
                        }
                        else
                        {
                            //hit on all roots
                            XmlNode hit = FindMatchingNodes(node, hits, "targetpath").FirstOrDefault();
                            if (hit != null)
                            {
                                changedNodes = ProcessNode(node, hit) || changedNodes;
                            }
                            else
                            {
                                //all paths match at root level but no targetpaths match below that so default to the initial root
                                changedNodes = ProcessNode(node, hits[0]) || changedNodes;
                            }
                        }
                    }
                }
                
                if (saveConfig && changedNodes)
                {
                    Config.Save(TargetConfig, TargetFileName);
                }
            }

            return changedNodes;
        }

        private IEnumerable<XmlNode> FindMatchingNodes(XmlNode mergeNode, IEnumerable<XmlNode> rootNodes, string pathAttributeName)
        {
            foreach (var targetRoot in rootNodes)
            {
                var rootNode = FindMatchingNode(targetRoot, mergeNode, pathAttributeName);

                if(rootNode != null)
                {
                    yield return targetRoot;
                }
            }
        }

        private XmlNode FindMatchingNode(XmlNode rootNode, XmlNode mergeNode, string pathAttributeName)
        {
            Debug.Assert(mergeNode.Attributes != null);

            XmlNode matchingNode = null;
            if(mergeNode.Attributes[pathAttributeName] != null)
            {
                string rootNodePath = mergeNode.Attributes[pathAttributeName].Value;
                if (mergeNode.Attributes["nameSpace"] == null)
                {
                    matchingNode = FindNode(rootNode, rootNodePath);
                }
                else
                {
                    //Use Namespace Manager
                    string xmlNameSpace = mergeNode.Attributes["nameSpace"].Value;
                    string xmlNameSpacePrefix = mergeNode.Attributes["nameSpacePrefix"].Value;
                    var nsmgr = new XmlNamespaceManager(TargetConfig.NameTable);
                    nsmgr.AddNamespace(xmlNameSpacePrefix, xmlNameSpace);
                    matchingNode = FindNode(rootNode, rootNodePath, nsmgr);
                }
            }
            return matchingNode;
        }

        private IEnumerable<XmlNode> GetTargetRoots()
        {
            yield return TargetConfig.DocumentElement;
            
            var locations = TargetConfig.SelectNodes("/configuration/location");
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
						DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveAttribute:attributeName=" + attributeName.ToString());
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
                //Get Parent
                XmlNode parentNode = node.ParentNode;

                //Remove current Node
                if (parentNode != null)
                {
                    parentNode.RemoveChild(node);
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveNode:" + node.InnerXml.ToString());
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
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpdateAttribute:attributeName=" + attributeName.ToString());
                    if (rootNode.Attributes[attributeName] == null)
                    {
                        rootNode.Attributes.Append(TargetConfig.CreateAttribute(attributeName));
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
            string keyAttribute = "";
            if (actionNode.Attributes["key"] != null)
            {
                keyAttribute = actionNode.Attributes["key"].Value;
				DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpdateNode: keyAttribute=" + keyAttribute.ToString());
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
                        targetNode = FindMatchingNode(rootNode, actionNode, "targetpath");
                    }
                    if (targetNode == null)
                    {
                        //Since there is no collision we can just add the node
                        rootNode.AppendChild(TargetConfig.ImportNode(child, true));
                        changedNode = true;
                        continue;
                    }

                    //There is a collision so we need to determine what to do.
                    string collisionAction = actionNode.Attributes["collision"].Value;
                    switch (collisionAction.ToLowerInvariant())
                    {
                        case "overwrite":
                            var oldContent = rootNode.InnerXml;
                            rootNode.ReplaceChild(TargetConfig.ImportNode(child, true), targetNode);
                            var newContent = rootNode.InnerXml;
                            changedNode = !string.Equals(oldContent, newContent, StringComparison.Ordinal);
                            break;
                        case "save":
                            string commentHeaderText = string.Format(Localization.Localization.GetString("XMLMERGE_Upgrade", Localization.Localization.SharedResourceFile),
                                                                     Environment.NewLine,
                                                                     Sender,
                                                                     Version,
                                                                     DateTime.Now);
                            XmlComment commentHeader = TargetConfig.CreateComment(commentHeaderText);

		                    var targetNodeContent = GetNodeContentWithoutComment(targetNode);
							XmlComment commentNode = TargetConfig.CreateComment(targetNodeContent);
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
		    RemoveCommentNodes(cloneNode);

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
					RemoveCommentNodes(childNode);
				}
		    }

		    if (commentNodes.Count > 0)
		    {
			    commentNodes.ForEach(n => { node.RemoveChild(n); });
		    }
	    }
		
		#endregion

		#region "Public Methods"


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UpdateConfig method processes the source file and updates the Target
        /// Config Xml Document.
        /// </summary>
        /// <param name="target">An Xml Document represent the Target Xml File</param>
        /// -----------------------------------------------------------------------------
        public void UpdateConfig(XmlDocument target)
        {
            var changedAnyNodes = false;
            TargetConfig = target;
            if (TargetConfig != null)
            {
                changedAnyNodes = ProcessNodes(SourceConfig.SelectNodes("/configuration/nodes/node"), false);
            }

            ConfigUpdateChangedNodes = changedAnyNodes;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UpdateConfig method processes the source file and updates the Target
        /// Config file.
        /// </summary>
        /// <param name="target">An Xml Document represent the Target Xml File</param>
        /// <param name="fileName">The fileName for the Target Xml File - relative to the webroot</param>
        /// -----------------------------------------------------------------------------
        public void UpdateConfig(XmlDocument target, string fileName)
        {
            var changedAnyNodes = false;
            TargetFileName = fileName;
            TargetConfig = target;
            if (TargetConfig != null)
            {
                changedAnyNodes = ProcessNodes(SourceConfig.SelectNodes("/configuration/nodes/node"), true);
            }

            ConfigUpdateChangedNodes = changedAnyNodes;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UpdateConfigs method processes the source file and updates the various config 
        /// files
        /// </summary>
        /// -----------------------------------------------------------------------------

        public void UpdateConfigs()
        {
            UpdateConfigs(true);
        }

        public void UpdateConfigs(bool autoSave)
        {
            var changedAnyNodes = false;
            var nodes = SourceConfig.SelectNodes("/configuration/nodes");
            if (nodes != null)
            {
                foreach (XmlNode configNode in nodes)
                {
                    Debug.Assert(configNode.Attributes != null, "configNode.Attributes != null");

                    //Attempt to load TargetFile property from configFile Atribute
                    TargetFileName = configNode.Attributes["configfile"].Value;
                    string targetProductName = "";
                    if (configNode.Attributes["productName"] != null)
                    {
                        targetProductName = configNode.Attributes["productName"].Value;
                    }
                    bool isAppliedToProduct;

                    if (!File.Exists(Globals.ApplicationMapPath + "\\" + TargetFileName))
                    {
                        DnnInstallLogger.InstallLogInfo($"Target File {TargetFileName} doesn't exist, ignore the merge process");
                        return;
                    }

                    TargetConfig = Config.Load(TargetFileName);
                    if (String.IsNullOrEmpty(targetProductName) || targetProductName == "All")
                    {
                        isAppliedToProduct = true;
                    }
                    else
                    {
                        isAppliedToProduct = DotNetNukeContext.Current.Application.ApplyToProduct(targetProductName);
                    }
                    //The nodes definition is not correct so skip changes
                    if (TargetConfig != null && isAppliedToProduct)
                    {
                        var changedNodes = ProcessNodes(configNode.SelectNodes("node"), autoSave);
                        changedAnyNodes = changedAnyNodes || changedNodes;
                        if (!autoSave && changedNodes)
                        {
                            PendingDocuments.Add(TargetFileName, TargetConfig);
                        }
                    }
                }
            }

            ConfigUpdateChangedNodes = changedAnyNodes;
        }

        public void SavePendingConfigs()
        {
            foreach (var key in PendingDocuments.Keys)
            {
                Config.Save(PendingDocuments[key], key);
            }
        }

        #endregion
    }
}
