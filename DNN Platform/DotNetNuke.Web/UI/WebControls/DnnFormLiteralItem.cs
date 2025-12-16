// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DnnFormLiteralItem : DnnFormItemBase
    {
        public DnnFormLiteralItem()
            : base()
        {
            this.ViewStateMode = ViewStateMode.Disabled;
        }

        /// <inheritdoc/>
        protected override WebControl CreateControlInternal(Control container)
        {
            var literal = new Label { ID = this.ID + "_Label", Text = Convert.ToString(this.Value, CultureInfo.InvariantCulture), };
            container.Controls.Add(literal);
            return literal;
        }
    }
}
