// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Common.Utilities;

    /// <summary>Abstraction of the <see cref="DataCache"/> class to enable DI and unit testing.</summary>
    internal interface IDataCache
    {
        /// <summary>Clears the cache.</summary>
        void ClearCache();
    }
}
