// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Common
{
    using System.Collections.Generic;

    internal class ContentItemEqualityComparer : IEqualityComparer<ContentItem>
    {
        public bool Equals(ContentItem x, ContentItem y)
        {
            return x.ContentItemId == y.ContentItemId;
        }

        public int GetHashCode(ContentItem obj)
        {
            return obj.ContentItemId.GetHashCode();
        }
    }
}
