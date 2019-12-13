// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data.SqlTypes;
using System.Globalization;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnDatePicker : RadDatePicker
    {
        protected override void OnInit(EventArgs e)
        {
            if (CultureInfo.CurrentCulture.Name == "ar-SA")
            {
                Culture.DateTimeFormat.Calendar = new GregorianCalendar();
            }

            base.OnInit(e);
            base.EnableEmbeddedBaseStylesheet = false;
            Utilities.ApplySkin(this);
            this.Calendar.ClientEvents.OnLoad = "$.dnnRadPickerHack";
            var specialDay = new RadCalendarDay();
            specialDay.Repeatable = Telerik.Web.UI.Calendar.RecurringEvents.Today;
            specialDay.ItemStyle.CssClass = "dnnCalendarToday";
            this.Calendar.SpecialDays.Add(specialDay);
            this.Calendar.RangeMinDate = (DateTime)SqlDateTime.MinValue;
            this.MinDate = (DateTime)SqlDateTime.MinValue;
        }
    }
}
