// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

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
    [ToolboxData("<{0}:TrueFalseEditControl runat=server></{0}:TrueFalseEditControl>")]
    public class TrueFalseEditControl : EditControl
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TrueFalseEditControl));

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrueFalseEditControl"/> class.
        /// Constructs a TrueFalseEditControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public TrueFalseEditControl()
        {
            this.SystemType = "System.Boolean";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether booleanValue returns the Boolean representation of the Value.
        /// </summary>
        /// <value>A Boolean representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected bool BooleanValue
        {
            get
            {
                bool boolValue = Null.NullBoolean;
                try
                {
                    // Try and cast the value to an Boolean
                    boolValue = Convert.ToBoolean(this.Value);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return boolValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether oldBooleanValue returns the Boolean representation of the OldValue.
        /// </summary>
        /// <value>A Boolean representing the OldValue.</value>
        /// -----------------------------------------------------------------------------
        protected bool OldBooleanValue
        {
            get
            {
                bool boolValue = Null.NullBoolean;
                try
                {
                    // Try and cast the value to an Boolean
                    boolValue = Convert.ToBoolean(this.OldValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return boolValue;
            }
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
                return this.BooleanValue.ToString();
            }

            set
            {
                bool setValue = bool.Parse(value);
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
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.BooleanValue;
            args.OldValue = this.OldBooleanValue;
            args.StringValue = this.StringValue;
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
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            if (this.BooleanValue)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Value, "True");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Localization.GetString("True", Localization.SharedResourceFile));
            writer.RenderEndTag();
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            if (!this.BooleanValue)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Value, "False");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Localization.GetString("False", Localization.SharedResourceFile));
            writer.RenderEndTag();
        }
    }
}
