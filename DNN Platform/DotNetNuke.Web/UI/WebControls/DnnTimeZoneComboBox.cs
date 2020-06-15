// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    public class DnnTimeZoneComboBox : DropDownList
    {
        protected override void OnInit(System.EventArgs e)
        {
            // Utilities.ApplySkin(this);
            base.OnInit(e);

            this.DataTextField = "DisplayName";
            this.DataValueField = "Id";

            this.DataSource = TimeZoneInfo.GetSystemTimeZones();
            this.DataBind();
        }
    }
}
