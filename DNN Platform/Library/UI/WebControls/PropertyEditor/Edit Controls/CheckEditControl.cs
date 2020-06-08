// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Specialized;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TrueFalseEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TrueFalseEditControl control provides a standard UI component for editing
    /// true/false (boolean) properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:CheckEditControl runat=server></{0}:CheckEditControl>")]
    public class CheckEditControl : TrueFalseEditControl
    {
        public CheckEditControl()
        {
            SystemType = "System.Boolean";
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string postedValue = postCollection[postDataKey];
            bool boolValue = false;
            if (!(postedValue == null || postedValue == string.Empty))
            {
                boolValue = true;
            }
            if (!BooleanValue.Equals(boolValue))
            {
                Value = boolValue;
                return true;
            }
            return false;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page != null && EditMode == PropertyEditorMode.Edit)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            if ((BooleanValue))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, "1");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, "");
            }

            writer.AddAttribute("onclick", "this.value = this.checked ? '1' : '';");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            if ((BooleanValue))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
    }
}
