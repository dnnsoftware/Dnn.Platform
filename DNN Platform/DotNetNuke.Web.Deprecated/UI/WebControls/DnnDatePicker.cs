// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Data.SqlTypes;
    using System.Globalization;

    using Telerik.Web.UI;

    public class DnnDatePicker : RadDatePicker
    {
        protected override void OnInit(EventArgs e)
        {
            if (CultureInfo.CurrentCulture.Name == "ar-SA")
            {
                this.Culture.DateTimeFormat.Calendar = new GregorianCalendar();
            }

            base.OnInit(e);
            this.EnableEmbeddedBaseStylesheet = false;
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
