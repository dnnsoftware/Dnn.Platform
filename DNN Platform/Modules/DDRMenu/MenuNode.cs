using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DotNetNuke.Common;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.DDRMenu.DNNCommon;

namespace DotNetNuke.Web.DDRMenu
{
    [Serializable]
    [XmlRoot("root", Namespace = "")]
    public class MenuNode : IXmlSerializable
    {
        public static List<MenuNode> ConvertDNNNodeCollection(DNNNodeCollection dnnNodes, MenuNode parent)
        {
            var result = new List<MenuNode>();
            foreach (DNNNode node in dnnNodes)
                result.Add(new MenuNode(node, parent));
            return result;
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
        public bool First { get { return (Parent == null) || (Parent.Children[0] == this); } }
        public bool Last { get { return (Parent == null) || (Parent.Children[Parent.Children.Count - 1] == this); } }
        public string Target { get; set; }

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

        public string Keywords { get; set; }
        public string Description { get; set; }

        private List<MenuNode> _Children;
        public List<MenuNode> Children { get { return _Children ?? (_Children = new List<MenuNode>()); } set { _Children = value; } }

        public MenuNode Parent { get; set; }

        public MenuNode()
        {
        }

        public MenuNode(DNNNodeCollection dnnNodes)
        {
            Children = ConvertDNNNodeCollection(dnnNodes, this);
        }

        public MenuNode(List<MenuNode> nodes)
        {
            Children = nodes;
            Children.ForEach(c => c.Parent = this);
        }

        public MenuNode(DNNNode dnnNode, MenuNode parent)
        {
            TabId = Convert.ToInt32(dnnNode.ID);
            Text = dnnNode.Text;
            Url = (dnnNode.ClickAction == eClickAction.PostBack)
                    ? "postback:" + dnnNode.ID
                    : String.IsNullOrEmpty(dnnNode.JSFunction) ? dnnNode.NavigateURL : "javascript:" + dnnNode.JSFunction;
            Enabled = dnnNode.Enabled;
            Selected = dnnNode.Selected;
            Breadcrumb = dnnNode.BreadCrumb;
            Separator = dnnNode.IsBreak;
            Icon = dnnNode.Image;
            Target = dnnNode.Target;
            Title = null;
            Keywords = null;
            Description = null;
            Parent = parent;
            CommandName = dnnNode.get_CustomAttribute("CommandName");
            CommandArgument = dnnNode.get_CustomAttribute("CommandArgument");

            DNNAbstract.DNNNodeToMenuNode(dnnNode, this);

            if ((dnnNode.DNNNodes != null) && (dnnNode.DNNNodes.Count > 0))
                Children = ConvertDNNNodeCollection(dnnNode.DNNNodes, this);
        }

        public MenuNode FindById(int tabId)
        {
            if (tabId == TabId)
                return this;

            foreach (var child in Children)
            {
                var result = child.FindById(tabId);
                if (result != null)
                    return result;
            }

            return null;
        }

        public MenuNode FindByName(string tabName)
        {
            if (tabName.Equals(Text, StringComparison.InvariantCultureIgnoreCase))
                return this;

            foreach (var child in Children)
            {
                var result = child.FindByName(tabName);
                if (result != null)
                    return result;
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
                    flattened.AddRange(FlattenChildren(child));
                }
            }

            return flattened;
        }

        public MenuNode FindByNameOrId(string tabNameOrId)
        {
            if (tabNameOrId.Equals(Text, StringComparison.InvariantCultureIgnoreCase))
                return this;
            if (tabNameOrId == TabId.ToString())
                return this;

            foreach (var child in Children)
            {
                var result = child.FindByNameOrId(tabNameOrId);
                if (result != null)
                    return result;
            }

            return null;
        }

        internal void RemoveAll(List<MenuNode> filteredNodes)
        {
            this.Children.RemoveAll(filteredNodes.Contains);
            foreach (var child in Children)
            {
                child.RemoveAll(filteredNodes);
            }
        }        

        public bool HasChildren()
        {
            return (Children.Count > 0);
        }

        internal void ApplyContext(string defaultImagePath)
        {
            Icon = ApplyContextToImagePath(Icon, defaultImagePath);
            LargeImage = ApplyContextToImagePath(LargeImage, defaultImagePath);

            if (Url != null && Url.StartsWith("postback:"))
            {
                var postbackControl = DNNContext.Current.HostControl;
                Url = postbackControl.Page.ClientScript.GetPostBackClientHyperlink(postbackControl, Url.Substring(9));
            }

            Children.ForEach(c => c.ApplyContext(defaultImagePath));
        }

        private string ApplyContextToImagePath(string imagePath, string defaultImagePath)
        {
            var result = imagePath;
            if (!String.IsNullOrEmpty(result))
            {
                if (result.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
                    result = Globals.ResolveUrl(result);
                else if (!(result.Contains("://") || result.StartsWith("/")))
                    result = defaultImagePath + result;
            }
            return result;
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
                            TabId = Convert.ToInt32(reader.Value);
                            break;
                        case "text":
                            Text = reader.Value;
                            break;
                        case "title":
                            Title = reader.Value;
                            break;
                        case "url":
                            Url = reader.Value;
                            break;
                        case "enabled":
                            Enabled = (reader.Value == "1");
                            break;
                        case "selected":
                            Selected = (reader.Value == "1");
                            break;
                        case "breadcrumb":
                            Breadcrumb = (reader.Value == "1");
                            break;
                        case "separator":
                            Separator = (reader.Value == "1");
                            break;
                        case "icon":
                            Icon = reader.Value;
                            break;
                        case "largeimage":
                            LargeImage = reader.Value;
                            break;
                        case "commandname":
                            CommandName = reader.Value;
                            break;
                        case "commandargument":
                            CommandArgument = reader.Value;
                            break;
                        case "target":
                            Target = reader.Value;
                            break;
                        //default:
                        //    throw new XmlException(String.Format("Unexpected attribute '{0}'", reader.Name));
                    }
                }
                while (reader.MoveToNextAttribute());
            }

            if (empty)
                return;

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
                                Children.Add(child);
                                break;
                            case "keywords":
                                Keywords = reader.ReadElementContentAsString().Trim();
                                break;
                            case "description":
                                Description = reader.ReadElementContentAsString().Trim();
                                break;
                            case "root":
                                break;
                            default:
                                throw new XmlException(String.Format("Unexpected element '{0}'", reader.Name));
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
            if (Parent != null)
            {
                writer.WriteStartElement("node");

                AddXmlAttribute(writer, "id", TabId);
                AddXmlAttribute(writer, "text", Text);
                AddXmlAttribute(writer, "title", Title);
                AddXmlAttribute(writer, "url", Url);
                AddXmlAttribute(writer, "enabled", Enabled);
                AddXmlAttribute(writer, "selected", Selected);
                AddXmlAttribute(writer, "breadcrumb", Breadcrumb);
                AddXmlAttribute(writer, "separator", Separator);
                AddXmlAttribute(writer, "target", Target);
                AddXmlAttribute(writer, "icon", Icon);
                AddXmlAttribute(writer, "largeimage", LargeImage);
                AddXmlAttribute(writer, "commandname", CommandName);
                AddXmlAttribute(writer, "commandargument", CommandArgument);
                AddXmlAttribute(writer, "first", First);
                AddXmlAttribute(writer, "last", Last);
                AddXmlAttribute(writer, "only", First && Last);
                AddXmlAttribute(writer, "depth", Depth);
                AddXmlElement(writer, "keywords", Keywords);
                AddXmlElement(writer, "description", Description);
            }

            Children.ForEach(c => c.WriteXml(writer));

            if (Parent != null)
                writer.WriteEndElement();
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, string value)
        {
            if (!String.IsNullOrEmpty(value))
                writer.WriteAttributeString(name, value);
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
            if (!String.IsNullOrEmpty(value))
                writer.WriteElementString(name, value);
        }
    }
}