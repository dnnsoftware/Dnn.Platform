// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Framework.JavaScriptLibraries;

namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    public partial class jQuery : SkinObjectBase
    {
        public bool DnnjQueryPlugins { get; set; }
        public bool jQueryHoverIntent { get; set; }
        public bool jQueryUI { get; set; }

        protected override void OnInit(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);

            if (this.jQueryUI)
            {
                JavaScript.RequestRegistration(CommonJs.jQueryUI);
            }

            if (this.DnnjQueryPlugins)
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            }

            if (this.jQueryHoverIntent)
            {
                JavaScript.RequestRegistration(CommonJs.HoverIntent);
            }
        }
    }
}
