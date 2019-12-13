// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
