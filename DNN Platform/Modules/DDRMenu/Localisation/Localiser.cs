// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.UI.WebControls;

    using MenuNode = DotNetNuke.Web.DDRMenu.MenuNode;

    public class Localiser
    {
        private static bool apiChecked;
        private static ILocalisation _LocalisationApi;
        private readonly int portalId;

        public Localiser(int portalId)
        {
            this.portalId = portalId;
        }

        private static ILocalisation LocalisationApi
        {
            get
            {
                if (!apiChecked)
                {
                    foreach (var api in new ILocalisation[] { new Generic(), new Ealo(), new Apollo() }) // new Adequation()
                    {
                        if (api.HaveApi())
                        {
                            _LocalisationApi = api;
                            break;
                        }
                    }

                    apiChecked = true;
                }

                return _LocalisationApi;
            }
        }

        public static DNNNodeCollection LocaliseDNNNodeCollection(DNNNodeCollection nodes)
        {
            return (LocalisationApi == null) ? nodes : (LocalisationApi.LocaliseNodes(nodes) ?? nodes);
        }

        public void LocaliseNode(MenuNode node)
        {
            var tab = (node.TabId > 0) ? TabController.Instance.GetTab(node.TabId, Null.NullInteger, false) : null;
            if (tab != null)
            {
                var localised = this.LocaliseTab(tab);
                tab = localised ?? tab;

                if (localised != null)
                {
                    node.TabId = tab.TabID;
                    node.Text = tab.TabName;
                    node.Enabled = !tab.DisableLink;
                    if (!tab.IsVisible)
                    {
                        node.TabId = -1;
                    }
                }

                node.Title = tab.Title;
                node.Description = tab.Description;
                node.Keywords = tab.KeyWords;
            }
            else
            {
                node.TabId = -1;
            }

            node.Children.ForEach(this.LocaliseNode);
        }

        private TabInfo LocaliseTab(TabInfo tab)
        {
            return (LocalisationApi == null) ? null : LocalisationApi.LocaliseTab(tab, this.portalId);
        }
    }
}
