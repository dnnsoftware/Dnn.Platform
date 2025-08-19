// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections
{
    public enum ConnectorCategories
    {
        /// <summary>A social connector.</summary>
        Social = 0,

        /// <summary>A file system connector.</summary>
        FileSystem = 1,

        /// <summary>An analytics connector.</summary>
        Analytics = 2,

        /// <summary>A marketing connector.</summary>
        Marketting = 3,

        /// <summary>Any other type of connector.</summary>
        Other = 4,
    }
}
