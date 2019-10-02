using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Themes.Components
{
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