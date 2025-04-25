// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    public enum PageExtensionUsageType
    {
        /// <summary>Always use page extension.</summary>
        AlwaysUse = 0,

        /// <summary>Only use page extension for pages.</summary>
        PageOnly = 1,

        /// <summary>Never user the page extension.</summary>
        Never = 2,
    }
}
