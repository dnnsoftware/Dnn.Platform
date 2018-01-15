#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;

using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// The Xml Helpers class is used to read in parameter rewrite/replace/redirect rules from the friendlyUrlParms.config file
    /// </summary>
    internal static class XmlHelpers
    {
        /// <summary>
        /// Returns a tab id from either a raw tabId, or a list of tab names delimited by ';'
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
                    if (Int32.TryParse(rawTabId, out tabId))
                    {
                        tabIds.Add(tabId);
                    }
                }
            }
            if (tabNames != null)
            {
                //get the portal by name
                if (tabNames == "All")
                {
                    tabIds.Add(-1);
                }
                else
                {
                    //loop through all specified tab names
                    foreach (string tabName in tabNames.Split(';'))
                    {
                        if (String.Compare(tabName, "default.aspx", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            //default.aspx is marked with a -2 tabid
                            tabIds.Add(-2);
                        }
                        else
                        {
                            //593 : add in site root rewriting processing
                            if (tabName == "/")
                            {
                                //site root marked with a -3 tabid
                                tabIds.Add(-3);
                            }
                            else
                            {
                                //portal id specified : specific portal
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