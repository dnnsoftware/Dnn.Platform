// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{

    public class DnnTimeZoneComboBox : DropDownList
    {

        protected override void OnInit(System.EventArgs e)
        {
            //Utilities.ApplySkin(this);
            base.OnInit(e);

            this.DataTextField = "DisplayName";
            this.DataValueField = "Id";

            this.DataSource = TimeZoneInfo.GetSystemTimeZones();
			this.DataBind();
        }
    }
}

