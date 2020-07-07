// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;

    /// <summary>
    /// The PageIndexData class is used during the page index build process.
    /// </summary>
    [Serializable]
    internal class PageIndexData
    {
        public string LastPageKey;
        public string LastPageValue;
    }
}
