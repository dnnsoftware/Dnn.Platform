// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Tabs;

    /// <summary>
    /// The Xml Helpers class is used to read in parameter rewrite/replace/redirect rules from the friendlyUrlParms.config file.
    /// </summary>
    internal static class XmlHelpers
    {
        /// <summary>
        /// Returns a tab id from either a raw tabId, or a list of tab names delimited by ';'.
        /// </summary>
        /// <param name="tabIdsRaw"></param>
        /// <param name="tabNames"></param>
        /// <param name="portalId"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
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
                        if (string.Compare(tabName, "default.aspx", StringComparison.OrdinalIgnoreCase) == 0)
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
