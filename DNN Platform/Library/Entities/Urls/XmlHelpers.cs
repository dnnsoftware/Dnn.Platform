// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Tabs;

    /// <summary>The Xml Helpers class is used to read in parameter rewrite/replace/redirect rules from the friendlyUrlParms.config file.</summary>
    internal static class XmlHelpers
    {
        /// <summary>Converts <paramref name="tabIdsRaw"/> and <paramref name="tabNames"/> into a list of tab IDs.</summary>
        /// <param name="tabIdsRaw">A semicolon-delimited list of tab IDs.</param>
        /// <param name="tabNames">Either <c>"All"</c> or a semicolon-delimited list of tab names.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="messages">A list to be filled with messages.</param>
        /// <returns>A <see cref="List{T}"/> of tab IDs (<c>-1</c> indicates "all", <c>-2</c> indicates "default.aspx", and <c>-3</c> indicates site root).</returns>
        internal static List<int> TabIdsFromAttributes(string tabIdsRaw, string tabNames, int portalId, ref List<string> messages)
        {
            if (messages == null)
            {
                messages = new List<string>();
            }

            var tabIds = new List<int>();
            if (!string.IsNullOrEmpty(tabIdsRaw))
            {
                string[] rawTabids = tabIdsRaw.Split(';');
                foreach (string rawTabId in rawTabids)
                {
                    int tabId;
                    if (int.TryParse(rawTabId, out tabId))
                    {
                        tabIds.Add(tabId);
                    }
                }
            }

            if (tabNames != null)
            {
                // get the portal by name
                if (tabNames == "All")
                {
                    tabIds.Add(-1);
                }
                else
                {
                    // loop through all specified tab names
                    foreach (string tabName in tabNames.Split(';'))
                    {
                        if (string.Equals(tabName, "default.aspx", StringComparison.OrdinalIgnoreCase))
                        {
                            // default.aspx is marked with a -2 tabid
                            tabIds.Add(-2);
                        }
                        else
                        {
                            // 593 : add in site root rewriting processing
                            if (tabName == "/")
                            {
                                // site root marked with a -3 tabid
                                tabIds.Add(-3);
                            }
                            else
                            {
                                // portal id specified : specific portal
                                TabInfo tab = TabController.Instance.GetTabByName(tabName, portalId);
                                if (tab != null)
                                {
                                    tabIds.Add(tab.TabID);
                                }
                                else
                                {
                                    messages.Add("TabName " + tabName + " not found for portalId " + portalId.ToString());
                                }
                            }
                        }
                    }
                }
            }

            return tabIds;
        }
    }
}
