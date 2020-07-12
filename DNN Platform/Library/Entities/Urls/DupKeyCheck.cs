// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// The DupKeyCheck class is a small helper class used to maintain state of what to do with a duplicate Url when building the Url Index.
    /// </summary>
    internal class DupKeyCheck
    {
        public DupKeyCheck(string tabKey, string tabIdOriginal, string tabPath, bool isDeleted)
        {
            this.TabKey = tabKey;
            this.TabIdOriginal = tabIdOriginal;
            this.TabPath = tabPath;
            this.IsDeleted = isDeleted;
        }

        public string TabKey { get; set; }

        public string TabIdOriginal { get; set; }

        public string TabPath { get; set; }

        public bool IsDeleted { get; set; }
    }
}
