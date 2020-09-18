// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DnnImageButton : ImageButton
    {
        public string IconKey { get; set; }

        public string IconSize { get; set; }

        public string IconStyle { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (string.IsNullOrEmpty(this.ImageUrl))
            {
                this.ImageUrl = Entities.Icons.IconController.IconURL(this.IconKey, this.IconSize, this.IconStyle);
            }
        }
    }
}
