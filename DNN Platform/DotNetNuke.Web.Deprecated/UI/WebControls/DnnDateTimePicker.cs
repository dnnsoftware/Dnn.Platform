// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data.SqlTypes;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnDateTimePicker : RadDateTimePicker
    {
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			base.EnableEmbeddedBaseStylesheet = true;
			Utilities.ApplySkin(this, string.Empty, "DatePicker");
			this.Calendar.ClientEvents.OnLoad = "$.dnnRadPickerHack";
			var specialDay = new RadCalendarDay();
			specialDay.Repeatable = Telerik.Web.UI.Calendar.RecurringEvents.Today;
			specialDay.ItemStyle.CssClass = "dnnCalendarToday";
			this.Calendar.SpecialDays.Add(specialDay);
            this.Calendar.RangeMinDate = (DateTime)SqlDateTime.MinValue;
            this.Calendar.RangeMaxDate = (DateTime)SqlDateTime.MaxValue;
            this.MinDate = (DateTime)SqlDateTime.MinValue;
            this.MaxDate = (DateTime)SqlDateTime.MaxValue;
        }
    }
}
