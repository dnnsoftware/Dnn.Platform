// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            return !isHostMenu || !m.DisableOnHost;
        }
    }
}
