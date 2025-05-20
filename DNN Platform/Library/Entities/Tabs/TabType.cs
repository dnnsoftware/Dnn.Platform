// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    public enum TabType
    {
        /// <summary>Redirect to a file.</summary>
        File = 0,

        /// <summary>A normal tab with its own content.</summary>
        Normal = 1,

        /// <summary>Redirect to another tab.</summary>
        Tab = 2,

        /// <summary>Redirect to an arbitrary URL.</summary>
        Url = 3,

        /// <summary>Redirect to a user's profile page.</summary>
        Member = 4,
    }
}
