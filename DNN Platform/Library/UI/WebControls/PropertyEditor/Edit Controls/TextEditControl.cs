// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;

    /// <summary>The TextEditControl control provides a standard UI component for editing string/text properties.</summary>
    [ToolboxData("<{0}:TextEditControl runat=server></{0}:TextEditControl>")]
    public class TextEditControl : EditControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditControl"/> class.
        /// Constructs a TextEditControl.
        /// </summary>
        public TextEditControl()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditControl"/> class.
        /// Constructs a TextEditControl.
        /// </summary>
        /// <param name="type">The type of the property.</param>
        public TextEditControl(string type)
        {
            this.SystemType = type;
        }

        /// <summary>Gets oldStringValue returns the Boolean representation of the OldValue.</summary>
        /// <value>A String representing the OldValue.</value>
        protected string OldStringValue => Convert.ToString(this.OldValue, CultureInfo.InvariantCulture);

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        protected override string StringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (this.Value != null)
                {
                    strValue = Convert.ToString(this.Value, CultureInfo.InvariantCulture);
                }

                return strValue;
            }

            set
            {
                this.Value = value;
            }
        }

        /// <summary>OnDataChanged runs when the PostbackData has changed.  It raises the <see cref="EditControl.ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.StringValue;
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
            this.OnValueChanged(args);
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int length = Null.NullInteger;
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    if (attribute is MaxLengthAttribute maxLengthAttribute)
                    {
                        length = maxLengthAttribute.Length;
                        break;
                    }
                }
            }

            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.StringValue);
            if (length > Null.NullInteger)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString(CultureInfo.InvariantCulture));
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
    }
}
