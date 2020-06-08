// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Urls
{
    [Serializable]
    public class PathSizes
    {
        public int MaxAliasDepth { get; set; }
        public int MaxTabPathDepth{ get; set; }
        public int MinAliasDepth { get; set; }
        public int MinTabPathDepth { get; set; }

        public void SetAliasDepth(string httpAlias)
        {
            int aliasPathDepth = httpAlias.Length - httpAlias.Replace("/", "").Length;
            if (aliasPathDepth > MaxAliasDepth)
            {
                MaxAliasDepth = aliasPathDepth;
            }
            if (aliasPathDepth < MinAliasDepth)
            {
                MinAliasDepth = aliasPathDepth;
            }
        }

        public void SetTabPathDepth(int tabPathDepth)
        {
            if (tabPathDepth > MaxTabPathDepth)
            {
                MaxTabPathDepth = tabPathDepth;
            }
            if (tabPathDepth < MinTabPathDepth)
            {
                MinTabPathDepth = tabPathDepth;
            }
        }
    }
}
