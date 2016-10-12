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
        /// Themes available in all sites.
        /// </summary>
        Global = 2
    }
}