// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using System;
    using System.Reflection;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.WebControls;

    public class Generic : ILocalisation
    {
        private bool haveChecked;
        private object locApi;
        private MethodInfo locTab;
        private MethodInfo locNodes;

        public bool HaveApi()
        {
            if (!this.haveChecked)
            {
                var modules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
                foreach (var moduleKeyPair in modules)
                {
                    if (!string.IsNullOrEmpty(moduleKeyPair.Value.BusinessControllerClass))
                    {
                        try
                        {
                            this.locApi = Reflection.CreateObject(moduleKeyPair.Value.BusinessControllerClass, moduleKeyPair.Value.BusinessControllerClass);
                            this.locTab = this.locApi.GetType().GetMethod("LocaliseTab", new[] { typeof(TabInfo), typeof(int) });
                            if (this.locTab != null)
                            {
                                if (this.locTab.IsStatic)
                                {
                                    this.locApi = null;
                                }

                                break;
                            }

                            this.locNodes = this.locApi.GetType().GetMethod("LocaliseNodes", new[] { typeof(DNNNodeCollection) });
                            if (this.locNodes != null)
                            {
                                if (this.locNodes.IsStatic)
                                {
                                    this.locApi = null;
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

                this.haveChecked = true;
            }

            return (this.locTab != null) || (this.locNodes != null);
        }

        public TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
            return (this.locTab == null) ? null : (TabInfo)this.locTab.Invoke(this.locApi, new object[] { tab, portalId });
        }

        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
        {
            return (this.locNodes == null) ? null : (DNNNodeCollection)this.locNodes.Invoke(this.locApi, new object[] { nodes });
        }
    }
}
