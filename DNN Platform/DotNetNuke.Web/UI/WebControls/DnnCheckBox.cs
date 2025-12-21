// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI.WebControls;

    public class DnnCheckBox : CheckBox
    {
        public string CommandArgument
        {
            get
            {
                return Convert.ToString(this.ViewState["CommandArgument"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.ViewState["CommandArgument"] = value;
            }
        }
    }
}
