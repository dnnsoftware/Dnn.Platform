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

namespace DotNetNuke.Web.DDRMenu
{
	public class MenuBase
	{
		public static MenuBase Instantiate(string menuStyle)
		{
			try
			{
				var templateDef = TemplateDefinition.FromName(menuStyle, "*menudef.xml");
				return new MenuBase {TemplateDef = templateDef};
			}
			catch (Exception exc)
			{
				throw new ApplicationException(String.Format("Couldn't load menu style '{0}': {1}", menuStyle, exc));
			}
		}

		private Settings menuSettings;
		internal MenuNode RootNode { get; set; }
		internal Boolean SkipLocalisation { get; set; }
		public TemplateDefinition TemplateDef { get; set; }

		private HttpContext currentContext;
		private HttpContext CurrentContext { get { return currentContext ?? (currentContext = HttpContext.Current); } }

		private PortalSettings hostPortalSettings;
		internal PortalSettings HostPortalSettings { get { return hostPortalSettings ?? (hostPortalSettings = PortalController.Instance.GetCurrentPortalSettings()); } }

		private readonly Dictionary<string, string> nodeSelectorAliases = new Dictionary<string, string>
																		  {
																			{"rootonly", "*,0,0"},
																			{"rootchildren", "+0"},
																			{"currentchildren", "."}
																		  };

		internal void ApplySettings(Settings settings)
		{
			menuSettings = settings;
		}

		internal virtual void PreRender()
		{
			TemplateDef.AddTemplateArguments(menuSettings.TemplateArguments, true);
			TemplateDef.AddClientOptions(menuSettings.ClientOptions, true);

			if (!String.IsNullOrEmpty(menuSettings.NodeXmlPath))
			{
				LoadNodeXml();
			}
			if (!String.IsNullOrEmpty(menuSettings.NodeSelector))
			{
				ApplyNodeSelector();
			}
			if (!String.IsNullOrEmpty(menuSettings.IncludeNodes))
			{
				FilterNodes(menuSettings.IncludeNodes, false);
			}
			if (!String.IsNullOrEmpty(menuSettings.ExcludeNodes))
			{
				FilterNodes(menuSettings.ExcludeNodes, true);
			}
			if (String.IsNullOrEmpty(menuSettings.NodeXmlPath) && !SkipLocalisation)
			{
				new Localiser(HostPortalSettings.PortalId).LocaliseNode(RootNode);
			}
			if (!String.IsNullOrEmpty(menuSettings.NodeManipulator))
			{
				ApplyNodeManipulator();
			}

		    if (!menuSettings.IncludeHidden)
		    {
		        FilterHiddenNodes();
		    }

			var imagePathOption =
				menuSettings.ClientOptions.Find(o => o.Name.Equals("PathImage", StringComparison.InvariantCultureIgnoreCase));
			RootNode.ApplyContext(
				imagePathOption == null ? DNNContext.Current.PortalSettings.HomeDirectory : imagePathOption.Value);

			TemplateDef.PreRender();
		}

		internal void Render(HtmlTextWriter htmlWriter)
		{
		    if (Host.DebugMode)
		    {
                htmlWriter.Write("<!-- DDRmenu v07.04.01 - {0} template -->", menuSettings.MenuStyle);
		    }

			UserInfo user = null;
			if (menuSettings.IncludeContext)
			{
				user = UserController.Instance.GetCurrentUserInfo();
				user.Roles = user.Roles; // Touch roles to populate
			}

			TemplateDef.AddClientOptions(new List<ClientOption> {new ClientString("MenuStyle", menuSettings.MenuStyle)}, false);

			TemplateDef.Render(new MenuXml {root = RootNode, user = user}, htmlWriter);
		}

		private void LoadNodeXml()
		{
			menuSettings.NodeXmlPath =
				MapPath(
					new PathResolver(TemplateDef.Folder).Resolve(
						menuSettings.NodeXmlPath,
						PathResolver.RelativeTo.Manifest,
						PathResolver.RelativeTo.Skin,
						PathResolver.RelativeTo.Module,
						PathResolver.RelativeTo.Portal,
						PathResolver.RelativeTo.Dnn));

			var cache = CurrentContext.Cache;
			RootNode = cache[menuSettings.NodeXmlPath] as MenuNode;
			if (RootNode != null)
			{
				return;
			}

			using (var reader = XmlReader.Create(menuSettings.NodeXmlPath))
			{
				reader.ReadToFollowing("root");
				RootNode = (MenuNode)(new XmlSerializer(typeof(MenuNode), "").Deserialize(reader));
			}
			cache.Insert(menuSettings.NodeXmlPath, RootNode, new CacheDependency(menuSettings.NodeXmlPath));
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
						RootNode.Children.FindAll(
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
                        //flatten nodes first. tagged pages should be flattened and not heirarchical
                        if (flattenedNodes != new MenuNode())
			                flattenedNodes.Children = RootNode.FlattenChildren(RootNode);

                        filteredNodes.AddRange(
                            flattenedNodes.Children.FindAll(
                                n =>
                                {
                                    var tab = tc.GetTab(n.TabId, Null.NullInteger, false);
                                    return (tab.Terms.Any(x => x.Name.ToLower() == tagName));
                                }));
			        }

			    }
				else
				{
					var nodeText2 = nodeText;
					filteredNodes.AddRange(
						RootNode.Children.FindAll(
							n =>
							{
								var nodeName = n.Text.ToLowerInvariant();
								var nodeId = n.TabId.ToString();
								return (nodeText2 == nodeName || nodeText2 == nodeId);
							}));
				}
			}

            // if filtered for foksonomy tags, use flat tree to get all related pages in nodeselection
		    if (flattenedNodes.HasChildren())
		        RootNode = flattenedNodes;
			RootNode.Children.RemoveAll(n => filteredNodes.Contains(n) == exclude);
		}

        private void FilterHiddenNodes()
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var filteredNodes = new List<MenuNode>();
            filteredNodes.AddRange(
                RootNode.Children.FindAll(
                    n =>
                    {                     
                        var tab = TabController.Instance.GetTab(n.TabId, portalSettings.PortalId);
                        return tab == null || !tab.IsVisible;
                    }));

            RootNode.Children.RemoveAll(n => filteredNodes.Contains(n));
        }

        private void ApplyNodeSelector()
		{
			string selector;
			if (!nodeSelectorAliases.TryGetValue(menuSettings.NodeSelector.ToLowerInvariant(), out selector))
			{
				selector = menuSettings.NodeSelector;
			}

			var selectorSplit = SplitAndTrim(selector);

			var currentTabId = HostPortalSettings.ActiveTab.TabID;

			var newRoot = RootNode;

			var rootSelector = selectorSplit[0];
			if (rootSelector != "*")
			{
				if (rootSelector.StartsWith("+"))
				{
					var depth = Convert.ToInt32(rootSelector);
					newRoot = RootNode;
					for (var i = 0; i <= depth; i++)
					{
						newRoot = newRoot.Children.Find(n => n.Breadcrumb);
						if (newRoot == null)
						{
							RootNode = new MenuNode();
							return;
						}
					}
				}
				else if (rootSelector.StartsWith("-") || rootSelector == "0" || rootSelector == ".")
				{
					newRoot = RootNode.FindById(currentTabId);
					if (newRoot == null)
					{
						RootNode = new MenuNode();
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
					newRoot = RootNode.FindByNameOrId(rootSelector);
					if (newRoot == null)
					{
						RootNode = new MenuNode();
						return;
					}
				}
			}

// ReSharper disable PossibleNullReferenceException
			RootNode = new MenuNode(newRoot.Children);
// ReSharper restore PossibleNullReferenceException

			if (selectorSplit.Count > 1)
			{
				for (var n = Convert.ToInt32(selectorSplit[1]); n > 0; n--)
				{
					var newChildren = new List<MenuNode>();
					foreach (var child in RootNode.Children)
					{
						newChildren.AddRange(child.Children);
					}
					RootNode = new MenuNode(newChildren);
				}
			}

			if (selectorSplit.Count > 2)
			{
				var newChildren = RootNode.Children;
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
			RootNode =
				new MenuNode(
					((INodeManipulator)Activator.CreateInstance(BuildManager.GetType(menuSettings.NodeManipulator, true, true))).
						ManipulateNodes(RootNode.Children, HostPortalSettings));
		}

		protected string MapPath(string path)
		{
			return String.IsNullOrEmpty(path) ? "" : Path.GetFullPath(CurrentContext.Server.MapPath(path));
		}

		private static List<string> SplitAndTrim(string str)
		{
			return new List<String>(str.Split(',')).ConvertAll(s => s.Trim().ToLowerInvariant());
		}
	}
}