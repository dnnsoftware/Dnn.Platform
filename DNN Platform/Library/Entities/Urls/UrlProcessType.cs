// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    public enum UrlProcessType
    {
        /// <summary>Rewriting the URL.</summary>
        Rewriting = 0,

        /// <summary>Redirecting the URL.</summary>
        Redirecting = 1,

        /// <summary>Replacing the URL.</summary>
        Replacing = 2,
    }
}
