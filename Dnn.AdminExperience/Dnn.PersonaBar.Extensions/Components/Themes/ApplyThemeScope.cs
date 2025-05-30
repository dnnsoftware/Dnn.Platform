// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Components
{
    using System;

    [Flags]
    public enum ApplyThemeScope
    {
        /// <summary>Regular site pages.</summary>
        Site = 1,

        /// <summary>Edit/admin pages/views.</summary>
        Edit = 2,
    }
}
