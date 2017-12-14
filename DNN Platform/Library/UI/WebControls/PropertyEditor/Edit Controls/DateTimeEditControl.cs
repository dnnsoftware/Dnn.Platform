#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
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
        /// DefaultFormat is a string that will be used to format the date in the absence of a 
        /// FormatAttribute
        /// </summary>
        /// <value>A String representing the default format to use to render the date</value>
        /// <returns>A Format String</returns>
        /// -----------------------------------------------------------------------------
        protected override string DefaultFormat
        {
            get
            {
                return "g";
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (string.IsNullOrEmpty(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator))
            {
                is24HourClock = true;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Add(new LiteralControl("<br/>"));
            hourField = new DropDownList();
            int maxHour = 12;
            int minHour = 1;
            if (is24HourClock)
            {
                minHour = 0;
                maxHour = 23;
            }
            for (int i = minHour; i <= maxHour; i++)
            {
                hourField.Items.Add(new ListItem(i.ToString("00"), i.ToString()));
            }
            hourField.ControlStyle.CopyFrom(ControlStyle);
            hourField.ID = ID + "hours";
            Controls.Add(hourField);
            Controls.Add(new LiteralControl("&nbsp"));
            minutesField = new DropDownList();
            for (int i = 0; i <= 59; i++)
            {
                minutesField.Items.Add(new ListItem(i.ToString("00"), i.ToString()));
            }
            minutesField.ControlStyle.CopyFrom(ControlStyle);
            minutesField.ID = ID + "minutes";
            Controls.Add(minutesField);
            if (!is24HourClock)
            {
                Controls.Add(new LiteralControl("&nbsp"));
                ampmField = new DropDownList();
                ampmField.Items.Add(new ListItem("AM", "AM"));
                ampmField.Items.Add(new ListItem("PM", "PM"));
                ampmField.ControlStyle.CopyFrom(ControlStyle);
                ampmField.ID = ID + "ampm";
                Controls.Add(ampmField);
            }
        }

        protected override void LoadDateControls()
        {
            base.LoadDateControls();
            int hour = DateValue.Hour;
            int minute = DateValue.Minute;
            bool isAM = true;
            if (!is24HourClock)
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
            if (hourField.Items.FindByValue(hour.ToString()) != null)
            {
                hourField.Items.FindByValue(hour.ToString()).Selected = true;
            }
            if (minutesField.Items.FindByValue(minute.ToString()) != null)
            {
                minutesField.Items.FindByValue(minute.ToString()).Selected = true;
            }
            if (!is24HourClock)
            {
                if (isAM)
                {
                    ampmField.SelectedIndex = 0;
                }
                else
                {
                    ampmField.SelectedIndex = 1;
                }
            }
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            DateTime presentValue = OldDateValue;
            string postedDate = postCollection[postDataKey + "date"];
            string postedHours = postCollection[postDataKey + "hours"];
            string postedMinutes = postCollection[postDataKey + "minutes"];
            string postedAMPM = postCollection[postDataKey + "ampm"];
            DateTime postedValue = Null.NullDate;
            if (!string.IsNullOrEmpty(postedDate))
            {
                DateTime.TryParse(postedDate, out postedValue);
            }
            if (postedHours != "12" || is24HourClock)
            {
                int hours = 0;                
                if(Int32.TryParse(postedHours, out hours)) postedValue = postedValue.AddHours(hours);
            }
            postedValue = postedValue.AddMinutes(Int32.Parse(postedMinutes));
            if (!is24HourClock && postedAMPM.Equals("PM"))
            {
                postedValue = postedValue.AddHours(12);
            }
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue.ToString(CultureInfo.InvariantCulture);
                dataChanged = true;
            }
            return dataChanged;
        }
    }
}
