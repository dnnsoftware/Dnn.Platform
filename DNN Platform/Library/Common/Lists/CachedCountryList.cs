// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Lists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common.Utilities;

    [Serializable]
    public class CachedCountryList : Dictionary<string, CachedCountryList.Country>
    {
        public CachedCountryList(string locale)
            : base()
        {
            foreach (ListEntryInfo li in new ListController().GetListEntryInfoItems("Country"))
            {
                string text = li.Text;
                Country c = new Country
                {
                    Id = li.EntryID,
                    Code = li.Value,
                    FullName = string.Format("{0} ({1})", text, li.Value),
                    Name = text,
                };
                c.NormalizedFullName = c.FullName.NormalizeString();
                this.Add(li.Value, c);
            }
        }

        public static CachedCountryList GetCountryList(string locale)
        {
            CachedCountryList res = null;
            try
            {
                res = (CachedCountryList)DotNetNuke.Common.Utilities.DataCache.GetCache(CacheKey(locale));
            }
            catch (Exception)
            {
                // do nothing here.
            }

            if (res == null)
            {
                res = new CachedCountryList(locale);
                DotNetNuke.Common.Utilities.DataCache.SetCache(CacheKey(locale), res);
            }

            return res;
        }

        public static string CacheKey(string locale)
        {
            return string.Format("CountryList:{0}", locale);
        }

        [Serializable]
        public struct Country
        {
            public int Id;
            public string Name;
            public string Code;
            public string FullName;
            public string NormalizedFullName;
        }
    }
}
