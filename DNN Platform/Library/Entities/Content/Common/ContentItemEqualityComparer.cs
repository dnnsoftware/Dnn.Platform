// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;

namespace DotNetNuke.Entities.Content.Common
{
    internal class ContentItemEqualityComparer : IEqualityComparer<ContentItem>
    {
        #region Implementation of IEqualityComparer<ContentItem>

        public bool Equals(ContentItem x, ContentItem y)
        {
            return x.ContentItemId == y.ContentItemId;
        }

        public int GetHashCode(ContentItem obj)
        {
            return obj.ContentItemId.GetHashCode();
        }

        #endregion
    }
}
