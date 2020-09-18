// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    using Calendar = DotNetNuke.Common.Utilities.Calendar;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DateEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DateEditControl control provides a standard UI component for editing
    /// date properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DateEditControl runat=server></{0}:DateEditControl>")]
    public class DateEditControl : EditControl
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DateEditControl));
        private TextBox dateField;
        private HyperLink linkCalendar;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets dateValue returns the Date representation of the Value.
        /// </summary>
        /// <value>A Date representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected DateTime DateValue
        {
            get
            {
                DateTime dteValue = Null.NullDate;
                try
                {
                    var dteString = Convert.ToString(this.Value);
                    DateTime.TryParse(dteString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dteValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return dteValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets defaultDateFormat is a string that will be used to format the date in the absence of a
        /// FormatAttribute.
        /// </summary>
        /// <value>A String representing the default format to use to render the date.</value>
        /// <returns>A Format String.</returns>
        /// -----------------------------------------------------------------------------
        protected virtual string DefaultFormat
        {
            get
            {
                return "d";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets format is a string that will be used to format the date in View mode.
        /// </summary>
        /// <value>A String representing the format to use to render the date.</value>
        /// <returns>A Format String.</returns>
        /// -----------------------------------------------------------------------------
        protected virtual string Format
        {
            get
            {
                string _Format = this.DefaultFormat;
                if (this.CustomAttributes != null)
                {
                    foreach (Attribute attribute in this.CustomAttributes)
                    {
                        if (attribute is FormatAttribute)
                        {
                            var formatAtt = (FormatAttribute)attribute;
                            _Format = formatAtt.Format;
                            break;
                        }
                    }
                }

                return _Format;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets oldDateValue returns the Date representation of the OldValue.
        /// </summary>
        /// <value>A Date representing the OldValue.</value>
        /// -----------------------------------------------------------------------------
        protected DateTime OldDateValue
        {
            get
            {
                DateTime dteValue = Null.NullDate;
                try
                {
                    // Try and cast the value to an DateTime
                    var dteString = this.OldValue as string;
                    dteValue = DateTime.Parse(dteString, CultureInfo.InvariantCulture);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return dteValue;
            }
        }

        /// <summary>
        /// Gets or sets the Value expressed as a String.
        /// </summary>
        protected override string StringValue
        {
            get
            {
                string _StringValue = Null.NullString;
                if (this.DateValue.ToUniversalTime().Date != DateTime.Parse("1754/01/01") && this.DateValue != Null.NullDate)
                {
                    _StringValue = this.DateValue.ToString(this.Format);
                }

                return _StringValue;
            }

            set
            {
                this.Value = DateTime.Parse(value);
            }
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            this.EnsureChildControls();
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey + "date"];
            if (!presentValue.Equals(postedValue))
            {
                this.Value = DateTime.Parse(postedValue).ToString(CultureInfo.InvariantCulture);
                dataChanged = true;
            }

            return dataChanged;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.dateField = new TextBox();
            this.dateField.ControlStyle.CopyFrom(this.ControlStyle);
            this.dateField.ID = this.ID + "date";
            this.Controls.Add(this.dateField);

            this.Controls.Add(new LiteralControl("&nbsp;"));

            this.linkCalendar = new HyperLink();
            this.linkCalendar.CssClass = "CommandButton";
            this.linkCalendar.Text = "<img src=\"" + Globals.ApplicationPath + "/images/calendar.png\" border=\"0\" />&nbsp;&nbsp;" + Localization.GetString("Calendar");
            this.linkCalendar.NavigateUrl = Calendar.InvokePopupCal(this.dateField);
            this.Controls.Add(this.linkCalendar);
        }

        protected virtual void LoadDateControls()
        {
            if (this.DateValue != Null.NullDate)
            {
                this.dateField.Text = this.DateValue.Date.ToString("d");
            }
        }

        /// <summary>
        /// OnDataChanged is called by the PostBack Handler when the Data has changed.
        /// </summary>
        /// <param name="e">An EventArgs object.</param>
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.DateValue;
            args.OldValue = this.OldDateValue;
            args.StringValue = this.DateValue.ToString(CultureInfo.InvariantCulture);
            this.OnValueChanged(args);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.LoadDateControls();

            if (this.Page != null && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        /// <summary>
        /// RenderEditMode is called by the base control to render the control in Edit Mode.
        /// </summary>
        /// <param name="writer"></param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(this.StringValue);
            writer.RenderEndTag();
        }
    }
}
