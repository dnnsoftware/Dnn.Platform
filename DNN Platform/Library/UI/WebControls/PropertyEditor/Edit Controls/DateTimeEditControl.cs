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

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DateTimeEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DateTimeEditControl control provides a standard UI component for editing
    /// date and time properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DateTimeEditControl runat=server></{0}:DateTimeEditControl>")]
    public class DateTimeEditControl : DateEditControl
    {
        private DropDownList ampmField;
        private DropDownList hourField;
        private bool is24HourClock;
        private DropDownList minutesField;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets defaultFormat is a string that will be used to format the date in the absence of a
        /// FormatAttribute.
        /// </summary>
        /// <value>A String representing the default format to use to render the date.</value>
        /// <returns>A Format String.</returns>
        /// -----------------------------------------------------------------------------
        protected override string DefaultFormat
        {
            get
            {
                return "g";
            }
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            DateTime presentValue = this.OldDateValue;
            string postedDate = postCollection[postDataKey + "date"];
            string postedHours = postCollection[postDataKey + "hours"];
            string postedMinutes = postCollection[postDataKey + "minutes"];
            string postedAMPM = postCollection[postDataKey + "ampm"];
            DateTime postedValue = Null.NullDate;
            if (!string.IsNullOrEmpty(postedDate))
            {
                DateTime.TryParse(postedDate, out postedValue);
            }

            if (postedHours != "12" || this.is24HourClock)
            {
                int hours = 0;
                if (int.TryParse(postedHours, out hours))
                {
                    postedValue = postedValue.AddHours(hours);
                }
            }

            postedValue = postedValue.AddMinutes(int.Parse(postedMinutes));
            if (!this.is24HourClock && postedAMPM.Equals("PM"))
            {
                postedValue = postedValue.AddHours(12);
            }

            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue.ToString(CultureInfo.InvariantCulture);
                dataChanged = true;
            }

            return dataChanged;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (string.IsNullOrEmpty(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator))
            {
                this.is24HourClock = true;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.Controls.Add(new LiteralControl("<br/>"));
            this.hourField = new DropDownList();
            int maxHour = 12;
            int minHour = 1;
            if (this.is24HourClock)
            {
                minHour = 0;
                maxHour = 23;
            }

            for (int i = minHour; i <= maxHour; i++)
            {
                this.hourField.Items.Add(new ListItem(i.ToString("00"), i.ToString()));
            }

            this.hourField.ControlStyle.CopyFrom(this.ControlStyle);
            this.hourField.ID = this.ID + "hours";
            this.Controls.Add(this.hourField);
            this.Controls.Add(new LiteralControl("&nbsp"));
            this.minutesField = new DropDownList();
            for (int i = 0; i <= 59; i++)
            {
                this.minutesField.Items.Add(new ListItem(i.ToString("00"), i.ToString()));
            }

            this.minutesField.ControlStyle.CopyFrom(this.ControlStyle);
            this.minutesField.ID = this.ID + "minutes";
            this.Controls.Add(this.minutesField);
            if (!this.is24HourClock)
            {
                this.Controls.Add(new LiteralControl("&nbsp"));
                this.ampmField = new DropDownList();
                this.ampmField.Items.Add(new ListItem("AM", "AM"));
                this.ampmField.Items.Add(new ListItem("PM", "PM"));
                this.ampmField.ControlStyle.CopyFrom(this.ControlStyle);
                this.ampmField.ID = this.ID + "ampm";
                this.Controls.Add(this.ampmField);
            }
        }

        protected override void LoadDateControls()
        {
            base.LoadDateControls();
            int hour = this.DateValue.Hour;
            int minute = this.DateValue.Minute;
            bool isAM = true;
            if (!this.is24HourClock)
            {
                if (hour >= 12)
                {
                    hour -= 12;
                    isAM = false;
                }

                if (hour == 0)
                {
                    hour = 12;
                }
            }

            if (this.hourField.Items.FindByValue(hour.ToString()) != null)
            {
                this.hourField.Items.FindByValue(hour.ToString()).Selected = true;
            }

            if (this.minutesField.Items.FindByValue(minute.ToString()) != null)
            {
                this.minutesField.Items.FindByValue(minute.ToString()).Selected = true;
            }

            if (!this.is24HourClock)
            {
                if (isAM)
                {
                    this.ampmField.SelectedIndex = 0;
                }
                else
                {
                    this.ampmField.SelectedIndex = 1;
                }
            }
        }
    }
}
