// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>
    /// Creates a control that render one item in a list ($lt;li> control).
    /// </summary>
    /// <remarks></remarks>
    public class DnnUnsortedListItem : WebControl
    {
        public DnnUnsortedListItem()
            : base(HtmlTextWriterTag.Li)
        {
        }

        public void AddControls(params Control[] childControls)
        {
            foreach (var childControl in childControls)
            {
                if (childControl != null)
                {
                    this.Controls.Add(childControl);
                }
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            }

            if (!string.IsNullOrEmpty(this.ToolTip))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.ToolTip);
            }
        }
    }
}
