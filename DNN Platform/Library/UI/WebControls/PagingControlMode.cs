// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// <summary>The PagingControlMode Enum provides an enumeration of the modes of the Paging Control.</summary>
    public enum PagingControlMode
    {
        /// <summary>Manage paging events by processing a post request.</summary>
        PostBack = 0,

        /// <summary>Manage paging event by redirecting to a new URL.</summary>
        URL = 1,
    }
}
