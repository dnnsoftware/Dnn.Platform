// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;

    [Serializable]
    public class PathSizes
    {
        public int MaxAliasDepth { get; set; }

        public int MaxTabPathDepth { get; set; }

        public int MinAliasDepth { get; set; }

        public int MinTabPathDepth { get; set; }

        public void SetAliasDepth(string httpAlias)
        {
            int aliasPathDepth = httpAlias.Length - httpAlias.Replace("/", string.Empty).Length;
            if (aliasPathDepth > this.MaxAliasDepth)
            {
                this.MaxAliasDepth = aliasPathDepth;
            }

            if (aliasPathDepth < this.MinAliasDepth)
            {
                this.MinAliasDepth = aliasPathDepth;
            }
        }

        public void SetTabPathDepth(int tabPathDepth)
        {
            if (tabPathDepth > this.MaxTabPathDepth)
            {
                this.MaxTabPathDepth = tabPathDepth;
            }

            if (tabPathDepth < this.MinTabPathDepth)
            {
                this.MinTabPathDepth = tabPathDepth;
            }
        }
    }
}
