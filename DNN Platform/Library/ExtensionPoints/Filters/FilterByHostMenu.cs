// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters
{
    public class FilterByHostMenu : IExtensionPointFilter
    {
        private readonly bool isHostMenu;

        public FilterByHostMenu(bool isHostMenu)
        {
            this.isHostMenu = isHostMenu;
        }

        public bool Condition(IExtensionPointData m)
        {
            return !this.isHostMenu || !m.DisableOnHost;
        }
    }
}
