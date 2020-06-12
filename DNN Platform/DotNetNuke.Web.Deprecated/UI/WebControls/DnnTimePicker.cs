// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTimePicker : RadTimePicker
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EnableEmbeddedBaseStylesheet = true;
            Utilities.ApplySkin(this, string.Empty, "DatePicker");
        }
    }
}
