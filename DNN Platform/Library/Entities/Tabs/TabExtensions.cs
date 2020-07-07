// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System.Collections.Generic;
    using System.Linq;

    public static class TabExtensions
    {
        public static bool ContainsAlias(this List<TabAliasSkinInfo> aliases, string httpAlias)
        {
            return aliases.Any(tas => string.Compare(httpAlias, tas.HttpAlias, System.StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static TabAliasSkinInfo FindByHttpAlias(this List<TabAliasSkinInfo> aliases, string httpAlias)
        {
            return aliases.FirstOrDefault(tas => string.Compare(httpAlias, tas.HttpAlias, System.StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static int GetNextAvailableSeqNum(this List<TabUrlInfo> redirects, bool positive)
        {
            int seqNum = 1;
            foreach (TabUrlInfo redirect in redirects)
            {
                if (positive)
                {
                    if (redirect.SeqNum >= seqNum)
                    {
                        seqNum = redirect.SeqNum + 1;

                        // 602 : when seqnum == 0, then duplicate key problems arise
                        if (seqNum == 0)
                        {
                            seqNum++;
                        }
                    }
                }
                else
                {
                    seqNum = -1;
                    if (redirect.SeqNum <= seqNum)
                    {
                        seqNum = redirect.SeqNum - 1;

                        // 602 : don't allow seqnum to become zero
                        if (seqNum == 0)
                        {
                            seqNum--;
                        }
                    }
                }
            }

            return seqNum;
        }

        public static TabUrlInfo FindByAliasId(this List<TabUrlInfo> redirects, int portalAliasId)
        {
            return redirects.FirstOrDefault(redirect => redirect.PortalAliasId == portalAliasId && portalAliasId != 0);
        }

        public static TabUrlInfo CurrentUrl(this List<TabUrlInfo> redirects, string cultureCode)
        {
            TabUrlInfo result = null;
            TabUrlInfo lastSystemUrl = null;
            TabUrlInfo lastCustomUrl = null;
            foreach (var redirect in redirects)
            {
                if (redirect.HttpStatus == "200" && redirect.CultureCode == cultureCode)
                {
                    if (redirect.IsSystem)
                    {
                        lastSystemUrl = redirect;
                    }
                    else
                    {
                        lastCustomUrl = redirect;
                    }
                }
            }

            // look at the results
            if (lastCustomUrl != null)
            {
                result = lastCustomUrl;
            }
            else if (lastSystemUrl != null)
            {
                result = lastSystemUrl;
            }

            return result;
        }
    }
}
