using System;
using System.Reflection;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using DotNetNuke.Entities.Portals;

    public class Generic : ILocalisation
	{
		private bool haveChecked;
		private object locApi;
		private MethodInfo locTab;
		private MethodInfo locNodes;

		public bool HaveApi()
		{
			if (!haveChecked)
			{
				var modules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
				foreach (var moduleKeyPair in modules)
				{
                    if (!String.IsNullOrEmpty(moduleKeyPair.Value.BusinessControllerClass))
					{
						try
						{
                            locApi = Reflection.CreateObject(moduleKeyPair.Value.BusinessControllerClass, moduleKeyPair.Value.BusinessControllerClass);
							locTab = locApi.GetType().GetMethod("LocaliseTab", new[] {typeof(TabInfo), typeof(int)});
							if (locTab != null)
							{
								if (locTab.IsStatic)
								{
									locApi = null;
								}
								break;
							}

							locNodes = locApi.GetType().GetMethod("LocaliseNodes", new[] {typeof(DNNNodeCollection)});
							if (locNodes != null)
							{
								if (locNodes.IsStatic)
								{
									locApi = null;
								}
								break;
							}
						}
// ReSharper disable EmptyGeneralCatchClause
						catch
						{
						}
// ReSharper restore EmptyGeneralCatchClause
					}
				}
				haveChecked = true;
			}

			return (locTab != null) || (locNodes != null);
		}

		public TabInfo LocaliseTab(TabInfo tab, int portalId)
		{
			return (locTab == null) ? null : (TabInfo)locTab.Invoke(locApi, new object[] {tab, portalId});
		}

		public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
		{
			return (locNodes == null) ? null : (DNNNodeCollection)locNodes.Invoke(locApi, new object[] {nodes});
		}
	}
}