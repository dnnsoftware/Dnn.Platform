// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// Implementation of <see cref="IDataCache"/> that relies on the <see cref="DataCache"/> class.
    /// </summary>
    internal class DataCacheShim : IDataCache
    {
        /// <inheritdoc/>
        public void ClearCache()
        {
            DataCache.ClearCache();
        }
    }
}
