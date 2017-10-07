#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
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

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(XmlMerge));
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
            SourceConfig = new XmlDocument();
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
            SourceConfig = new XmlDocument();
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
            SourceConfig = new XmlDocument();
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

        private void AddNode(XmlNode rootNode, XmlNode actionNode)
        {
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.AppendChild(TargetConfig.ImportNode(child, true));
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddNode:" + child.InnerXml.ToString());
                }
            }
        }

        private void PrependNode(XmlNode rootNode, XmlNode actionNode)
        {
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.PrependChild(TargetConfig.ImportNode(child, true));
                    DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "PrependNode:" + child.InnerXml.ToString());
                }
            }
        }

        private void InsertNode(XmlNode childRootNode, XmlNode actionNode, NodeInsertType mode)
        {
            XmlNode rootNode = childRootNode.ParentNode;
            Debug.Assert(rootNode != null);
            
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InsertNode:" + child.InnerXml.ToString());
                    switch (mode)
                    {
                        case NodeInsertType.Before:
                            rootNode.InsertBefore(TargetConfig.ImportNode(child, true), childRootNode);
                            break;
                        case NodeInsertType.After:
                            rootNode.InsertAfter(TargetConfig.ImportNode(child, true), childRootNode);
                            break;
                    }
                }
            }
        }

        private void ProcessNode(XmlNode node, XmlNode targetRoot)
        {
            Debug.Assert(node.Attributes != null, "node.Attributes != null");

            XmlNode rootNode = FindMatchingNode(targetRoot, node, "path");

            if (rootNode == null)
            {
                return; //not every TargetRoot will contain every target node
            }

            string nodeAction = node.Attributes["action"].Value.ToLowerInvariant();

            //IF the execute condition doesn't matched, then ignore process the node.
            if (node.Attributes["condition"] != null
                && !string.IsNullOrEmpty(node.Attributes["condition"].Value)
                && !ConditionMatched(node.Attributes["condition"].Value))
            {
                Logger.InfoFormat(" doesn't merged as condition doesn't matched, current status: {1}", node.OuterXml, Globals.Status);
                return;
            }
            
            switch (nodeAction)
            {
                case "add":
                    AddNode(rootNode, node);
                    break;
                case "prepend":
                    PrependNode(rootNode, node);
                    break;
                case "insertbefore":
                    InsertNode(rootNode, node, NodeInsertType.Before);
                    break;
                case "insertafter":
                    InsertNode(rootNode, node, NodeInsertType.After);
                    break;
                case "remove":
                    RemoveNode(rootNode);
                    break;
                case "removeattribute":
                    RemoveAttribute(rootNode, node);
                    break;
                case "update":
                    UpdateNode(rootNode, node);
                    break;
                case "updateattribute":
                    UpdateAttribute(rootNode, node);
                    break;
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

        private void ProcessNodes(XmlNodeList nodes, bool saveConfig)
        {
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
                        ProcessNode(node, root);
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
                            ProcessNode(node, hits[0]);
                        }
                        else if(hits.Count < targetRoots.Count)
                        {
                            ProcessNode(node, hits[0]);
                        }
                        else
                        {
                            //hit on all roots
                            XmlNode hit = FindMatchingNodes(node, hits, "targetpath").FirstOrDefault();
                            if (hit != null)
                            {
                                ProcessNode(node, hit);
                            }
                            else
                            {
                                //all paths match at root level but no targetpaths match below that so default to the initial root
                                ProcessNode(node, hits[0]);
                            }
                        }
                    }
                }
                
                if (saveConfig)
                {
                    Config.Save(TargetConfig, TargetFileName);
                }
            }
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

        private void RemoveAttribute(XmlNode rootNode, XmlNode actionNode)
        {
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");
            
            if (actionNode.Attributes["name"] != null)
            {
                string attributeName = actionNode.Attributes["name"].Value;
                if (!string.IsNullOrEmpty(attributeName))
                {
                    if (rootNode.Attributes[attributeName] != null)
                    {
                        rootNode.Attributes.Remove(rootNode.Attributes[attributeName]);
						DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveAttribute:attributeName=" + attributeName.ToString());
                    }
                }
            }
        }

        private void RemoveNode(XmlNode node)
        {
            if (node != null)
            {
                
                //Get Parent
                XmlNode parentNode = node.ParentNode;

                //Remove current Node
                if (parentNode != null)
                {
                    parentNode.RemoveChild(node);
					DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveNode:" + node.InnerXml.ToString());
                }
            }
        }

        private void UpdateAttribute(XmlNode rootNode, XmlNode actionNode)
        {
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");
            
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
                    }
                    rootNode.Attributes[attributeName].Value = attributeValue;
                }
            }
        }

        private void UpdateNode(XmlNode rootNode, XmlNode actionNode)
        {
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            
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
                        continue;
                    }

                    //There is a collision so we need to determine what to do.
                    string collisionAction = actionNode.Attributes["collision"].Value;
                    switch (collisionAction.ToLowerInvariant())
                    {
                        case "overwrite":
                            rootNode.RemoveChild(targetNode);
                            rootNode.InnerXml = rootNode.InnerXml + child.OuterXml;
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
                            rootNode.RemoveChild(targetNode);
                            rootNode.InnerXml = rootNode.InnerXml + commentHeader.OuterXml + commentNode.OuterXml + child.OuterXml;
                            break;
                        case "ignore":
                            break;
                    }
                }
            }
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

        private bool ConditionMatched(string condition)
        {
            var isNotCondition = false;
            if (condition.StartsWith("!"))
            {
                isNotCondition = true;
                condition = condition.Substring(1);
            }

            try
            {
                var statusToCheck = (Globals.UpgradeStatus) Enum.Parse(typeof (Globals.UpgradeStatus), condition, true);

                return isNotCondition ? statusToCheck != Globals.Status : statusToCheck == Globals.Status;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("The Condition \"{0}\" Parse Failed: {1}", condition, ex.Message);
                return true; //when condition parse failed, we should always merge the node.
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
            TargetConfig = target;
            if (TargetConfig != null)
            {
                ProcessNodes(SourceConfig.SelectNodes("/configuration/nodes/node"), false);
            }
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
            TargetFileName = fileName;
            TargetConfig = target;
            if (TargetConfig != null)
            {
                ProcessNodes(SourceConfig.SelectNodes("/configuration/nodes/node"), true);
            }
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
                        ProcessNodes(configNode.SelectNodes("node"), autoSave);

                        if (!autoSave)
                        {
                            PendingDocuments.Add(TargetFileName, TargetConfig);
                        }
                    }
                }
            }
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
