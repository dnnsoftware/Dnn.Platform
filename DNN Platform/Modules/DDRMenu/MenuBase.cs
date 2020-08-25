// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.UI;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    public class MenuBase
    {
        private readonly Dictionary<string, string> nodeSelectorAliases = new Dictionary<string, string>
                                                                          {
                                                                            { "rootonly", "*,0,0" },
                                                                            { "rootchildren", "+0" },
                                                                            { "currentchildren", "." },
                                                                          };

        private Settings menuSettings;

        private HttpContext currentContext;

        private PortalSettings hostPortalSettings;

        public TemplateDefinition TemplateDef { get; set; }

        internal PortalSettings HostPortalSettings
        {
            get { return this.hostPortalSettings ?? (this.hostPortalSettings = PortalController.Instance.GetCurrentPortalSettings()); }
        }

        internal MenuNode RootNode { get; set; }

        internal bool SkipLocalisation { get; set; }

        private HttpContext CurrentContext
        {
            get { return this.currentContext ?? (this.currentContext = HttpContext.Current); }
        }

        public static MenuBase Instantiate(string menuStyle)
        {
            try
            {
                var templateDef = TemplateDefinition.FromName(menuStyle, "*menudef.xml");
                return new MenuBase { TemplateDef = templateDef };
            }
            catch (Exception exc)
            {
                throw new ApplicationException(string.Format("Couldn't load menu style '{0}': {1}", menuStyle, exc));
            }
        }

        internal void ApplySettings(Settings settings)
        {
            this.menuSettings = settings;
        }

        internal virtual void PreRender()
        {
            this.TemplateDef.AddTemplateArguments(this.menuSettings.TemplateArguments, true);
            this.TemplateDef.AddClientOptions(this.menuSettings.ClientOptions, true);

            if (!string.IsNullOrEmpty(this.menuSettings.NodeXmlPath))
            {
                this.LoadNodeXml();
            }

            if (!string.IsNullOrEmpty(this.menuSettings.NodeSelector))
            {
                this.ApplyNodeSelector();
            }

            if (!string.IsNullOrEmpty(this.menuSettings.IncludeNodes))
            {
                this.FilterNodes(this.menuSettings.IncludeNodes, false);
            }

            if (!string.IsNullOrEmpty(this.menuSettings.ExcludeNodes))
            {
                this.FilterNodes(this.menuSettings.ExcludeNodes, true);
            }

            if (string.IsNullOrEmpty(this.menuSettings.NodeXmlPath) && !this.SkipLocalisation)
            {
                new Localiser(this.HostPortalSettings.PortalId).LocaliseNode(this.RootNode);
            }

            if (!string.IsNullOrEmpty(this.menuSettings.NodeManipulator))
            {
                this.ApplyNodeManipulator();
            }

            if (!this.menuSettings.IncludeHidden)
            {
                this.FilterHiddenNodes(this.RootNode);
            }

            var imagePathOption =
                this.menuSettings.ClientOptions.Find(o => o.Name.Equals("PathImage", StringComparison.InvariantCultureIgnoreCase));
            this.RootNode.ApplyContext(
                imagePathOption == null ? DNNContext.Current.PortalSettings.HomeDirectory : imagePathOption.Value);

            this.TemplateDef.PreRender();
        }

        internal void Render(HtmlTextWriter htmlWriter)
        {
            if (Host.DebugMode)
            {
                htmlWriter.Write("<!-- DDRmenu v07.04.01 - {0} template -->", this.menuSettings.MenuStyle);
            }

            UserInfo user = null;
            if (this.menuSettings.IncludeContext)
            {
                user = UserController.Instance.GetCurrentUserInfo();
                user.Roles = user.Roles; // Touch roles to populate
            }

            this.TemplateDef.AddClientOptions(new List<ClientOption> { new ClientString("MenuStyle", this.menuSettings.MenuStyle) }, false);

            this.TemplateDef.Render(new MenuXml { root = this.RootNode, user = user }, htmlWriter);
        }

        protected string MapPath(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFullPath(this.CurrentContext.Server.MapPath(path));
        }

        private static List<string> SplitAndTrim(string str)
        {
            return new List<string>(str.Split(',')).ConvertAll(s => s.Trim().ToLowerInvariant());
        }

        private void LoadNodeXml()
        {
            this.menuSettings.NodeXmlPath =
                this.MapPath(
                    new PathResolver(this.TemplateDef.Folder).Resolve(
                        this.menuSettings.NodeXmlPath,
                        PathResolver.RelativeTo.Manifest,
                        PathResolver.RelativeTo.Skin,
                        PathResolver.RelativeTo.Module,
                        PathResolver.RelativeTo.Portal,
                        PathResolver.RelativeTo.Dnn));

            var cache = this.CurrentContext.Cache;
            this.RootNode = cache[this.menuSettings.NodeXmlPath] as MenuNode;
            if (this.RootNode != null)
            {
                return;
            }

            using (var reader = XmlReader.Create(this.menuSettings.NodeXmlPath))
            {
                reader.ReadToFollowing("root");
                this.RootNode = (MenuNode)new XmlSerializer(typeof(MenuNode), string.Empty).Deserialize(reader);
            }

            cache.Insert(this.menuSettings.NodeXmlPath, this.RootNode, new CacheDependency(this.menuSettings.NodeXmlPath));
        }

        private void FilterNodes(string nodeString, bool exclude)
        {
            var nodeTextStrings = SplitAndTrim(nodeString);
            var filteredNodes = new List<MenuNode>();
            var tc = new TabController();
            var flattenedNodes = new MenuNode();

            foreach (var nodeText in nodeTextStrings)
            {
                if (nodeText.StartsWith("["))
                {
                    var roleName = nodeText.Substring(1, nodeText.Length - 2);
                    filteredNodes.AddRange(
                        this.RootNode.Children.FindAll(
                            n =>
                            {
                                var tab = TabController.Instance.GetTab(n.TabId, Null.NullInteger, false);
                                foreach (TabPermissionInfo perm in tab.TabPermissions)
                                {
                                    if (perm.AllowAccess && (perm.PermissionKey == "VIEW") &&
                                        ((perm.RoleID == -1) || (perm.RoleName.ToLowerInvariant() == roleName)))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }));
                }
                else if (nodeText.StartsWith("#"))
                {
                    var tagName = nodeText.Substring(1, nodeText.Length - 1);
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        // flatten nodes first. tagged pages should be flattened and not heirarchical
                        if (flattenedNodes != new MenuNode())
                        {
                            flattenedNodes.Children = this.RootNode.FlattenChildren(this.RootNode);
                        }

                        filteredNodes.AddRange(
                            flattenedNodes.Children.FindAll(
                                n =>
                                {
                                    var tab = tc.GetTab(n.TabId, Null.NullInteger, false);
                                    return tab.Terms.Any(x => x.Name.ToLowerInvariant() == tagName);
                                }));
                    }
                }
                else
                {
                    filteredNodes.Add(this.RootNode.FindByNameOrId(nodeText));
                }
            }

            // if filtered for foksonomy tags, use flat tree to get all related pages in nodeselection
            if (flattenedNodes.HasChildren())
            {
                this.RootNode = flattenedNodes;
            }

            if (exclude)
            {
                this.RootNode.RemoveAll(filteredNodes);
            }
            else
            {
                this.RootNode.Children.RemoveAll(n => filteredNodes.Contains(n) == exclude);
            }
        }

        private void FilterHiddenNodes(MenuNode parentNode)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var filteredNodes = new List<MenuNode>();
            filteredNodes.AddRange(
                parentNode.Children.FindAll(
                    n =>
                    {
                        var tab = TabController.Instance.GetTab(n.TabId, portalSettings.PortalId);
                        return tab == null || !tab.IsVisible;
                    }));

            parentNode.Children.RemoveAll(n => filteredNodes.Contains(n));

            parentNode.Children.ForEach(this.FilterHiddenNodes);
        }

        private void ApplyNodeSelector()
        {
            string selector;
            if (!this.nodeSelectorAliases.TryGetValue(this.menuSettings.NodeSelector.ToLowerInvariant(), out selector))
            {
                selector = this.menuSettings.NodeSelector;
            }

            var selectorSplit = SplitAndTrim(selector);

            var currentTabId = this.HostPortalSettings.ActiveTab.TabID;

            var newRoot = this.RootNode;

            var rootSelector = selectorSplit[0];
            if (rootSelector != "*")
            {
                if (rootSelector.StartsWith("+"))
                {
                    var depth = Convert.ToInt32(rootSelector);
                    newRoot = this.RootNode;
                    for (var i = 0; i <= depth; i++)
                    {
                        newRoot = newRoot.Children.Find(n => n.Breadcrumb);
                        if (newRoot == null)
                        {
                            this.RootNode = new MenuNode();
                            return;
                        }
                    }
                }
                else if (rootSelector.StartsWith("-") || rootSelector == "0" || rootSelector == ".")
                {
                    newRoot = this.RootNode.FindById(currentTabId);
                    if (newRoot == null)
                    {
                        this.RootNode = new MenuNode();
                        return;
                    }

                    if (rootSelector.StartsWith("-"))
                    {
                        for (var n = Convert.ToInt32(rootSelector); n < 0; n++)
                        {
                            if (newRoot.Parent != null)
                            {
                                newRoot = newRoot.Parent;
                            }
                        }
                    }
                }
                else
                {
                    newRoot = this.RootNode.FindByNameOrId(rootSelector);
                    if (newRoot == null)
                    {
                        this.RootNode = new MenuNode();
                        return;
                    }
                }
            }

            // ReSharper disable PossibleNullReferenceException
            this.RootNode = new MenuNode(newRoot.Children);

            // ReSharper restore PossibleNullReferenceException
            if (selectorSplit.Count > 1)
            {
                for (var n = Convert.ToInt32(selectorSplit[1]); n > 0; n--)
                {
                    var newChildren = new List<MenuNode>();
                    foreach (var child in this.RootNode.Children)
                    {
                        newChildren.AddRange(child.Children);
                    }

                    this.RootNode = new MenuNode(newChildren);
                }
            }

            if (selectorSplit.Count > 2)
            {
                var newChildren = this.RootNode.Children;
                for (var n = Convert.ToInt32(selectorSplit[2]); n > 0; n--)
                {
                    var nextChildren = new List<MenuNode>();
                    foreach (var child in newChildren)
                    {
                        nextChildren.AddRange(child.Children);
                    }

                    newChildren = nextChildren;
                }

                foreach (var node in newChildren)
                {
                    node.Children = null;
                }
            }
        }

        private void ApplyNodeManipulator()
        {
            this.RootNode =
                new MenuNode(
                    ((INodeManipulator)Activator.CreateInstance(BuildManager.GetType(this.menuSettings.NodeManipulator, true, true))).
                        ManipulateNodes(this.RootNode.Children, this.HostPortalSettings));
        }
    }
}
