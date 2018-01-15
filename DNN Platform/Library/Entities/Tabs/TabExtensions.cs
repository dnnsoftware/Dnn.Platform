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

using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.Entities.Tabs
{
    public static class TabExtensions
    {
        public static bool ContainsAlias(this List<TabAliasSkinInfo> aliases, string httpAlias)
        {
            return aliases.Any(tas => System.String.Compare(httpAlias, tas.HttpAlias, System.StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static TabAliasSkinInfo FindByHttpAlias(this List<TabAliasSkinInfo> aliases, string httpAlias)
        {
            return aliases.FirstOrDefault(tas => System.String.Compare(httpAlias, tas.HttpAlias, System.StringComparison.OrdinalIgnoreCase) == 0);
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
                        //602 : when seqnum == 0, then duplicate key problems arise
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
                        //602 : don't allow seqnum to become zero
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
            //look at the results
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
