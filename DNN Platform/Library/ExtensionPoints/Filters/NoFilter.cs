// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.ExtensionPoints.Filters
{
    public class NoFilter : IExtensionPointFilter
    {        
        public bool Condition(IExtensionPointData m)
        {
            return true;
        }
    }
}
