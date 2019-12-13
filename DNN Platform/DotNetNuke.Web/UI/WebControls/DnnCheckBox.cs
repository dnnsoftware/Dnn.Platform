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
    public class DnnCheckBox : CheckBox
    {
        public string CommandArgument
        {
            get
            {
                return Convert.ToString(ViewState["CommandArgument"]);
            }
            set
            {
                ViewState["CommandArgument"] = value;
            }
        }
    }
}
