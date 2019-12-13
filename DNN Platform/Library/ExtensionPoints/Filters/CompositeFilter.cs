// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.ExtensionPoints.Filters
{
    public class CompositeFilter : IExtensionPointFilter
    {
        private readonly IList<IExtensionPointFilter> filters;

        public CompositeFilter()
        {
            filters = new List<IExtensionPointFilter>();
        }

        public CompositeFilter And(IExtensionPointFilter filter)
        {
            filters.Add(filter);
            return this;
        }

        public bool Condition(IExtensionPointData m)
        {
            return filters.All(f => f.Condition(m));
        }
    }
}
