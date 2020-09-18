// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MultiLineTextEditControl control provides a standard UI component for editing
    /// string/text properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:MultiLineTextEditControl runat=server></{0}:MultiLineTextEditControl>")]
    public class MultiLineTextEditControl : TextEditControl
    {
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            string propValue = Convert.ToString(this.Value);
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute("aria-label", "editor");
            writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
            writer.Write(propValue);
            writer.RenderEndTag();
        }
    }
}
