// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation;

using System.Collections.Generic;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.UI.WebControls;
using effority.Ealo.Specialized;

using EaloTabInfo = effority.Ealo.Specialized.TabInfo;
using TabInfo = DotNetNuke.Entities.Tabs.TabInfo;

/// <summary>Deprecated Ealo localization support.</summary>
[DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
public partial class Ealo : ILocalisation
{
    private bool haveChecked;
    private bool found;

    /// <inheritdoc cref="ILocalisation.HaveApi"/>
    [DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
    public partial bool HaveApi()
    {
        if (!this.haveChecked)
        {
            this.found = DesktopModuleController.GetDesktopModuleByModuleName("effority.Ealo.Tabs", PortalSettings.Current.PortalId) != null;
            this.haveChecked = true;
        }

        return this.found;
    }

    /// <inheritdoc cref="ILocalisation.LocaliseTab"/>
    [DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
    public partial TabInfo LocaliseTab(TabInfo tab, int portalId)
    {
        return EaloWorker.LocaliseTab(tab, portalId);
    }

    /// <inheritdoc cref="ILocalisation.LocaliseNodes"/>
    [DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
    public partial DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
    {
        return null;
    }

    // Separate class only instantiated if Ealo is available.
    private static class EaloWorker
    {
        private static readonly Dictionary<string, Dictionary<int, EaloTabInfo>> EaloTabLookup =
            new Dictionary<string, Dictionary<int, EaloTabInfo>>();

        public static TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
            var culture = DNNAbstract.GetCurrentCulture();
            Dictionary<int, EaloTabInfo> ealoTabs;
            if (!EaloTabLookup.TryGetValue(culture, out ealoTabs))
            {
                ealoTabs = Tabs.GetAllTabsAsDictionary(culture, true);
                lock (EaloTabLookup)
                {
                    if (!EaloTabLookup.ContainsKey(culture))
                    {
                        EaloTabLookup.Add(culture, ealoTabs);
                    }
                }
            }

            EaloTabInfo ealoTab;
            if (ealoTabs.TryGetValue(tab.TabID, out ealoTab))
            {
                if (ealoTab.EaloTabName != null)
                {
                    tab.TabName = ealoTab.EaloTabName.StringTextOrFallBack;
                }

                if (ealoTab.EaloTitle != null)
                {
                    tab.Title = ealoTab.EaloTitle.StringTextOrFallBack;
                }
            }

            return tab;
        }
    }
}
