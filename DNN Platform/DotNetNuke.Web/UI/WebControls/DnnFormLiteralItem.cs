// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
