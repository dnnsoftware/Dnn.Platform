// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters
{
    using System.Collections.Generic;
    using System.Linq;

    public class CompositeFilter : IExtensionPointFilter
    {
        private readonly IList<IExtensionPointFilter> filters;

        public CompositeFilter()
        {
            this.filters = new List<IExtensionPointFilter>();
        }

        public CompositeFilter And(IExtensionPointFilter filter)
        {
            this.filters.Add(filter);
            return this;
        }

        public bool Condition(IExtensionPointData m)
        {
            return this.filters.All(f => f.Condition(m));
        }
    }
}
