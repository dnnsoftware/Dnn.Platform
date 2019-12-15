// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.ExtensionPoints.Filters
{
    public class FilterByUnauthenticated : IExtensionPointFilter
    {
        private readonly bool isAuthenticated;

        public FilterByUnauthenticated(bool isAuthenticated)
        {
            this.isAuthenticated = isAuthenticated;
        }

        public bool Condition(IExtensionPointData m)
        {
            return isAuthenticated || !m.DisableUnauthenticated;
        }
    }
}
