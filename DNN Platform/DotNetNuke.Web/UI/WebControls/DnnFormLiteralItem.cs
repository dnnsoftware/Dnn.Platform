// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormLiteralItem : DnnFormItemBase
    {
        public DnnFormLiteralItem() : base()
        {
            ViewStateMode = ViewStateMode.Disabled;    
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            var literal = new Label {ID = ID + "_Label", Text = Convert.ToString(Value)};
            container.Controls.Add(literal);
            return literal;
        }
    }
}
