// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.UI.WebControls;

    [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
    public class Apollo : ILocalisation
    {
        private bool haveChecked;
        private MethodInfo apiMember;

        [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
        public bool HaveApi()
        {
            if (!this.haveChecked)
            {
                try
                {
                    if (DesktopModuleController.GetDesktopModuleByModuleName("PageLocalization", PortalSettings.Current.PortalId) != null)
                    {
                        var api = Activator.CreateInstance("Apollo.LocalizationApi", "Apollo.DNN_Localization.LocalizeTab").Unwrap();
                        var apiType = api.GetType();
                        this.apiMember = apiType.GetMethod("getLocalizedTab", new[] { typeof(TabInfo) });
                    }
                }

                // ReSharper disable EmptyGeneralCatchClause
                catch

                // ReSharper restore EmptyGeneralCatchClause
                {
                }

                this.haveChecked = true;
            }

            return this.apiMember != null;
        }

        [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
        public TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
            return this.apiMember.Invoke(null, new object[] { tab }) as TabInfo ?? tab;
        }

        [Obsolete("Deprecated in 9.4.0, due to limited developer support.  Scheduled removal in v10.0.0.")]
        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
        {
            return null;
        }
    }
}
