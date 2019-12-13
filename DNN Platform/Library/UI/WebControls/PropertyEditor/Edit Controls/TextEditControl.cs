// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TextEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TextEditControl control provides a standard UI component for editing
    /// string/text properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:TextEditControl runat=server></{0}:TextEditControl>")]
    public class TextEditControl : EditControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a TextEditControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public TextEditControl()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a TextEditControl
        /// </summary>
        /// <param name="type">The type of the property</param>
        /// -----------------------------------------------------------------------------
        public TextEditControl(string type)
        {
            SystemType = type;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldStringValue returns the Boolean representation of the OldValue
        /// </summary>
        /// <value>A String representing the OldValue</value>
        /// -----------------------------------------------------------------------------
        protected string OldStringValue
        {
            get
            {
                return Convert.ToString(OldValue);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StringValue is the value of the control expressed as a String
        /// </summary>
        /// <value>A string representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (Value != null)
                {
                    strValue = Convert.ToString(Value);
                }
                return strValue;
            }
            set
            {
                Value = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(Name);
            args.Value = StringValue;
            args.OldValue = OldStringValue;
            args.StringValue = StringValue;
            base.OnValueChanged(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int length = Null.NullInteger;
            if ((CustomAttributes != null))
            {
                foreach (Attribute attribute in CustomAttributes)
                {
                    if (attribute is MaxLengthAttribute)
                    {
                        var lengthAtt = (MaxLengthAttribute) attribute;
                        length = lengthAtt.Length;
                        break;
                    }
                }
            }
            ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, StringValue);
            if (length > Null.NullInteger)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString());
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
    }
}
