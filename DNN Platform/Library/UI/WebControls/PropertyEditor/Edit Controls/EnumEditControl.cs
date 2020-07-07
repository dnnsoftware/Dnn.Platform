// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      EnumEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EnumEditControl control provides a standard UI component for editing
    /// enumerated properties.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:EnumEditControl runat=server></{0}:EnumEditControl>")]
    public class EnumEditControl : EditControl
    {
        private readonly Type EnumType;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumEditControl"/> class.
        /// Constructs an EnumEditControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public EnumEditControl()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumEditControl"/> class.
        /// Constructs an EnumEditControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public EnumEditControl(string type)
        {
            this.SystemType = type;
            this.EnumType = Type.GetType(type);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets stringValue is the value of the control expressed as a String.
        /// </summary>
        /// <value>A string representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                var retValue = Convert.ToInt32(this.Value);
                return retValue.ToString(CultureInfo.InvariantCulture);
            }

            set
            {
                int setValue = int.Parse(value);
                this.Value = setValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            int intValue = Convert.ToInt32(this.Value);
            int intOldValue = Convert.ToInt32(this.OldValue);

            var args = new PropertyEditorEventArgs(this.Name)
            { Value = Enum.ToObject(this.EnumType, intValue), OldValue = Enum.ToObject(this.EnumType, intOldValue) };

            this.OnValueChanged(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int propValue = Convert.ToInt32(this.Value);
            Array enumValues = Enum.GetValues(this.EnumType);

            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int I = 0; I <= enumValues.Length - 1; I++)
            {
                int enumValue = Convert.ToInt32(enumValues.GetValue(I));
                string enumName = Enum.GetName(this.EnumType, enumValue);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            int propValue = Convert.ToInt32(this.Value);
            string enumValue = Enum.Format(this.EnumType, propValue, "G");

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(enumValue);
            writer.RenderEndTag();
        }
    }
}
