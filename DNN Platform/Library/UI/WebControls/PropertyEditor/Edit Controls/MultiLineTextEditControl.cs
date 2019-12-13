// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
    ///-----------------------------------------------------------------------------
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
            string propValue = Convert.ToString(Value);
            ControlStyle.AddAttributesToRender(writer);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddAttribute("aria-label", "editor");
            writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
            writer.Write(propValue);
            writer.RenderEndTag();
        }
    }
}
