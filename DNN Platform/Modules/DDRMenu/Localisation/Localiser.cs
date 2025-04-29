// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

using MenuNode = DotNetNuke.Web.DDRMenu.MenuNode;

/// <summary>Implements the localization logic for all providers.</summary>
public class Localiser
{
    private static bool apiChecked;
    private static ILocalisation localisationApi;
    private readonly int portalId;

    /// <summary>Initializes a new instance of the <see cref="Localiser"/> class.</summary>
    /// <param name="portalId">The id of the portal on which the menu is displayed.</param>
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
#pragma warning disable CS0618 // Type or member is obsolete, Ealo and Apollor can be removed from here when those classes are removed in v10.
                foreach (var api in new ILocalisation[] { new Generic(), new Ealo(), new Apollo() })
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    if (api.HaveApi())
                    {
                        localisationApi = api;
                        break;
                    }
                }

                apiChecked = true;
            }

            return localisationApi;
        }
    }

    /// <summary>Localizes the menu nodes.</summary>
    /// <param name="nodes">The collection of nodes to localize.</param>
    /// <returns>The localized collection of nodes.</returns>
    public static DNNNodeCollection LocaliseDNNNodeCollection(DNNNodeCollection nodes)
    {
        return (LocalisationApi == null) ? nodes : (LocalisationApi.LocaliseNodes(nodes) ?? nodes);
    }

    /// <summary>Localizes a single node.</summary>
    /// <param name="node">The node to localize.</param>
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
