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
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnDatePicker : TextBox
    {
        protected virtual string Format => "yyyy-MM-dd";
        protected virtual string ClientFormat => "YYYY-MM-DD";

        public DateTime? SelectedDate {
            get
            {
                DateTime value;
                if (!string.IsNullOrEmpty(Text) && DateTime.TryParse(Text, out value))
                {
                    return value;
                }

                return null;
            }
            set
            {
                Text = value?.ToString(Format) ?? string.Empty;
                
            }
        }

        public DateTime MinDate { get; set; } = DateTime.MinValue;

        public DateTime MaxDate { get; set; } = DateTime.MaxValue;


        protected override void OnInit(EventArgs e)
        {
            //if (CultureInfo.CurrentCulture.Name == "ar-SA")
            //{
            //    Culture.DateTimeFormat.Calendar = new GregorianCalendar();
            //}

            base.OnInit(e);
            //base.EnableEmbeddedBaseStylesheet = false;
            //Utilities.ApplySkin(this);
            //this.Calendar.ClientEvents.OnLoad = "$.dnnRadPickerHack";
            //var specialDay = new RadCalendarDay();
            //specialDay.Repeatable = Telerik.Web.UI.Calendar.RecurringEvents.Today;
            //specialDay.ItemStyle.CssClass = "dnnCalendarToday";
            //this.Calendar.SpecialDays.Add(specialDay);
            //this.Calendar.RangeMinDate = (DateTime)SqlDateTime.MinValue;
            //this.MinDate = (DateTime)SqlDateTime.MinValue;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            JavaScript.RequestRegistration(CommonJs.jQuery);

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/components/DatePicker/moment.min.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/components/DatePicker/pikaday.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/components/DatePicker/pikaday.jquery.js");

            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/components/DatePicker/pikaday.css");

            RegisterClientResources();
        }

        protected virtual IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>
            {
                {"minDate", MinDate > DateTime.MinValue ? $"$new Date('{MinDate.ToString(Format)}')$" : ""},
                {"maxDate", MaxDate > DateTime.MinValue ? $"$new Date('{MaxDate.ToString(Format)}')$" : ""},
                {"format", ClientFormat }
            };
        } 

        private void RegisterClientResources()
        {
            var settings = Json.Serialize(GetSettings()).Replace("\"$", "").Replace("$\"", "");
            var script = $"$('#{ClientID}').pikaday({settings});";

            Page.ClientScript.RegisterStartupScript(Page.GetType(), "DnnDatePicker" + ClientID, script, true);
        }
    }
}