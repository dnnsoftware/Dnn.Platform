using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using DotNetNuke.Entities.Portals;

    [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
    public class Apollo : ILocalisation
	{
		private bool haveChecked;
		private MethodInfo apiMember;

        [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
        public bool HaveApi()
		{
			if (!haveChecked)
			{
				try
				{
                    if (DesktopModuleController.GetDesktopModuleByModuleName("PageLocalization", PortalSettings.Current.PortalId) != null)
					{
						var api = Activator.CreateInstance("Apollo.LocalizationApi", "Apollo.DNN_Localization.LocalizeTab").Unwrap();
						var apiType = api.GetType();
						apiMember = apiType.GetMethod("getLocalizedTab", new[] {typeof(TabInfo)});
					}
				}
// ReSharper disable EmptyGeneralCatchClause
				catch
// ReSharper restore EmptyGeneralCatchClause
				{
				}
				haveChecked = true;
			}

			return (apiMember != null);
		}

        [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
        public TabInfo LocaliseTab(TabInfo tab, int portalId)
		{
			return apiMember.Invoke(null, new object[] {tab}) as TabInfo ?? tab;
		}

        [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
		{
			return null;
		}
	}
}