// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Flags]
    public enum ThemeLevel
    {
        /// <summary>
        /// Themes only available in current site.
        /// </summary>
        Site = 1,

        /// <summary>
        /// Themes only available in current site which saved in portal system directory.
        /// </summary>
        SiteSystem = 2,

        /// <summary>
        /// Themes available in all sites.
        /// </summary>
        Global = 4,

        /// <summary>
        /// All Themes.
        /// </summary>
        All = 7
    }
}
