// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.DDRMenu.DNNCommon;

    [Serializable]
    [XmlRoot("root", Namespace = "")]
    public class MenuNode : IXmlSerializable
    {
        private List<MenuNode> _Children;

        public MenuNode()
        {
        }

        public MenuNode(DNNNodeCollection dnnNodes)
        {
            this.Children = ConvertDNNNodeCollection(dnnNodes, this);
        }

        public MenuNode(List<MenuNode> nodes)
        {
            this.Children = nodes;
            this.Children.ForEach(c => c.Parent = this);
        }

        public MenuNode(DNNNode dnnNode, MenuNode parent)
        {
            this.TabId = Convert.ToInt32(dnnNode.ID);
            this.Text = dnnNode.Text;
            this.Url = (dnnNode.ClickAction == eClickAction.PostBack)
                    ? "postback:" + dnnNode.ID
                    : string.IsNullOrEmpty(dnnNode.JSFunction) ? dnnNode.NavigateURL : "javascript:" + dnnNode.JSFunction;
            this.Enabled = dnnNode.Enabled;
            this.Selected = dnnNode.Selected;
            this.Breadcrumb = dnnNode.BreadCrumb;
            this.Separator = dnnNode.IsBreak;
            this.Icon = dnnNode.Image;
            this.Target = dnnNode.Target;
            this.Title = null;
            this.Keywords = null;
            this.Description = null;
            this.Parent = parent;
            this.CommandName = dnnNode.get_CustomAttribute("CommandName");
            this.CommandArgument = dnnNode.get_CustomAttribute("CommandArgument");

            DNNAbstract.DNNNodeToMenuNode(dnnNode, this);

            if ((dnnNode.DNNNodes != null) && (dnnNode.DNNNodes.Count > 0))
            {
                this.Children = ConvertDNNNodeCollection(dnnNode.DNNNodes, this);
            }
        }

        public bool First
        {
            get { return (this.Parent == null) || (this.Parent.Children[0] == this); }
        }

        public bool Last
        {
            get { return (this.Parent == null) || (this.Parent.Children[this.Parent.Children.Count - 1] == this); }
        }

        public int Depth
        {
            get
            {
                var result = -1;
                var current = this;
                while (current.Parent != null)
                {
                    result++;
                    current = current.Parent;
                }

                return result;
            }
        }

        public int TabId { get; set; }

        public string Text { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public bool Enabled { get; set; }

        public bool Selected { get; set; }

        public bool Breadcrumb { get; set; }

        public bool Separator { get; set; }

        public string Icon { get; set; }

        public string LargeImage { get; set; }

        public string CommandName { get; set; }

        public string CommandArgument { get; set; }

        public string Target { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }

        public List<MenuNode> Children
        {
            get { return this._Children ?? (this._Children = new List<MenuNode>()); }
            set { this._Children = value; }
        }

        public MenuNode Parent { get; set; }

        public static List<MenuNode> ConvertDNNNodeCollection(DNNNodeCollection dnnNodes, MenuNode parent)
        {
            var result = new List<MenuNode>();
            foreach (DNNNode node in dnnNodes)
            {
                result.Add(new MenuNode(node, parent));
            }

            return result;
        }

        public MenuNode FindById(int tabId)
        {
            if (tabId == this.TabId)
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.FindById(tabId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public MenuNode FindByName(string tabName)
        {
            if (tabName.Equals(this.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.FindByName(tabName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public List<MenuNode> FlattenChildren(MenuNode root)
        {
            var flattened = new List<MenuNode>();
            if (root.TabId != 0)
            {
                flattened.Add(root);
            }

            var children = root.Children;

            if (children != null)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(this.FlattenChildren(child));
                }
            }

            return flattened;
        }

        public MenuNode FindByNameOrId(string tabNameOrId)
        {
            if (tabNameOrId.Equals(this.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return this;
            }

            if (tabNameOrId == this.TabId.ToString())
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.FindByNameOrId(tabNameOrId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public bool HasChildren()
        {
            return this.Children.Count > 0;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var empty = reader.IsEmptyElement;

            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "id":
                            this.TabId = Convert.ToInt32(reader.Value);
                            break;
                        case "text":
                            this.Text = reader.Value;
                            break;
                        case "title":
                            this.Title = reader.Value;
                            break;
                        case "url":
                            this.Url = reader.Value;
                            break;
                        case "enabled":
                            this.Enabled = reader.Value == "1";
                            break;
                        case "selected":
                            this.Selected = reader.Value == "1";
                            break;
                        case "breadcrumb":
                            this.Breadcrumb = reader.Value == "1";
                            break;
                        case "separator":
                            this.Separator = reader.Value == "1";
                            break;
                        case "icon":
                            this.Icon = reader.Value;
                            break;
                        case "largeimage":
                            this.LargeImage = reader.Value;
                            break;
                        case "commandname":
                            this.CommandName = reader.Value;
                            break;
                        case "commandargument":
                            this.CommandArgument = reader.Value;
                            break;
                        case "target":
                            this.Target = reader.Value;
                            break;

                            // default:
                            //    throw new XmlException(String.Format("Unexpected attribute '{0}'", reader.Name));
                    }
                }
                while (reader.MoveToNextAttribute());
            }

            if (empty)
            {
                return;
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name.ToLowerInvariant())
                        {
                            case "node":
                                var child = new MenuNode { Parent = this };
                                child.ReadXml(reader);
                                this.Children.Add(child);
                                break;
                            case "keywords":
                                this.Keywords = reader.ReadElementContentAsString().Trim();
                                break;
                            case "description":
                                this.Description = reader.ReadElementContentAsString().Trim();
                                break;
                            case "root":
                                break;
                            default:
                                throw new XmlException(string.Format("Unexpected element '{0}'", reader.Name));
                        }

                        break;
                    case XmlNodeType.EndElement:
                        reader.ReadEndElement();
                        return;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.Parent != null)
            {
                writer.WriteStartElement("node");

                AddXmlAttribute(writer, "id", this.TabId);
                AddXmlAttribute(writer, "text", this.Text);
                AddXmlAttribute(writer, "title", this.Title);
                AddXmlAttribute(writer, "url", this.Url);
                AddXmlAttribute(writer, "enabled", this.Enabled);
                AddXmlAttribute(writer, "selected", this.Selected);
                AddXmlAttribute(writer, "breadcrumb", this.Breadcrumb);
                AddXmlAttribute(writer, "separator", this.Separator);
                AddXmlAttribute(writer, "target", this.Target);
                AddXmlAttribute(writer, "icon", this.Icon);
                AddXmlAttribute(writer, "largeimage", this.LargeImage);
                AddXmlAttribute(writer, "commandname", this.CommandName);
                AddXmlAttribute(writer, "commandargument", this.CommandArgument);
                AddXmlAttribute(writer, "first", this.First);
                AddXmlAttribute(writer, "last", this.Last);
                AddXmlAttribute(writer, "only", this.First && this.Last);
                AddXmlAttribute(writer, "depth", this.Depth);
                AddXmlElement(writer, "keywords", this.Keywords);
                AddXmlElement(writer, "description", this.Description);
            }

            this.Children.ForEach(c => c.WriteXml(writer));

            if (this.Parent != null)
            {
                writer.WriteEndElement();
            }
        }

        internal void RemoveAll(List<MenuNode> filteredNodes)
        {
            this.Children.RemoveAll(filteredNodes.Contains);
            foreach (var child in this.Children)
            {
                child.RemoveAll(filteredNodes);
            }
        }

        internal void ApplyContext(string defaultImagePath)
        {
            this.Icon = this.ApplyContextToImagePath(this.Icon, defaultImagePath);
            this.LargeImage = this.ApplyContextToImagePath(this.LargeImage, defaultImagePath);

            if (this.Url != null && this.Url.StartsWith("postback:"))
            {
                var postbackControl = DNNContext.Current.HostControl;
                this.Url = postbackControl.Page.ClientScript.GetPostBackClientHyperlink(postbackControl, this.Url.Substring(9));
            }

            this.Children.ForEach(c => c.ApplyContext(defaultImagePath));
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.WriteAttributeString(name, value);
            }
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, int value)
        {
            writer.WriteAttributeString(name, value.ToString());
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, bool value)
        {
            writer.WriteAttributeString(name, value ? "1" : "0");
        }

        private static void AddXmlElement(XmlWriter writer, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.WriteElementString(name, value);
            }
        }

        private string ApplyContextToImagePath(string imagePath, string defaultImagePath)
        {
            var result = imagePath;
            if (!string.IsNullOrEmpty(result))
            {
                if (result.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = Globals.ResolveUrl(result);
                }
                else if (!(result.Contains("://") || result.StartsWith("/")))
                {
                    result = defaultImagePath + result;
                }
            }

            return result;
        }
    }
}
