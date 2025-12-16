// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Services.Localization;

    /// <summary>The EnumEditControl control provides a standard UI component for editing enumerated properties.</summary>
    [ToolboxData("<{0}:EnumEditControl runat=server></{0}:EnumEditControl>")]
    public class EnumEditControl : EditControl
    {
        private readonly Type enumType;

        /// <summary>Initializes a new instance of the <see cref="EnumEditControl"/> class.</summary>
        public EnumEditControl()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EnumEditControl"/> class.</summary>
        /// <param name="type">The name of the <see cref="Type"/> of the enum.</param>
        public EnumEditControl(string type)
        {
            this.SystemType = type;
            this.enumType = Type.GetType(type);
        }

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        protected override string StringValue
        {
            get
            {
                var retValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
                return retValue.ToString(CultureInfo.InvariantCulture);
            }

            set
            {
                int setValue = int.Parse(value, CultureInfo.InvariantCulture);
                this.Value = setValue;
            }
        }

        /// <summary>OnDataChanged runs when the PostbackData has changed.  It raises the <see cref="EditControl.ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataChanged(EventArgs e)
        {
            int intValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
            int intOldValue = Convert.ToInt32(this.OldValue, CultureInfo.InvariantCulture);

            var args = new PropertyEditorEventArgs(this.Name)
            { Value = Enum.ToObject(this.enumType, intValue), OldValue = Enum.ToObject(this.enumType, intOldValue) };

            this.OnValueChanged(args);
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int propValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
            var enumValues = Enum.GetValues(this.enumType);

            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int i = 0; i <= enumValues.Length - 1; i++)
            {
                int enumValue = (int)enumValues.GetValue(i);
                string enumName = Enum.GetName(this.enumType, enumValue);
                enumName = Localization.GetString(enumName, this.LocalResourceFile);

                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, enumValue.ToString(CultureInfo.InvariantCulture));

                if (enumValue == propValue)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(enumName);
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            int propValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
            string enumValue = Enum.Format(this.enumType, propValue, "G");

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(enumValue);
            writer.RenderEndTag();
        }
    }
}
