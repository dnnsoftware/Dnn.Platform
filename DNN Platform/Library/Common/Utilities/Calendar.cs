// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System.Globalization;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    public class Calendar
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Opens a popup Calendar.
        /// </summary>
        /// <param name="Field">TextBox to return the date value.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string InvokePopupCal(TextBox Field)
        {
            // Define character array to trim from language strings
            char[] TrimChars = { ',', ' ' };

            // Get culture array of month names and convert to string for
            // passing to the popup calendar
            var monthBuilder = new StringBuilder();
            foreach (string Month in DateTimeFormatInfo.CurrentInfo.MonthNames)
            {
                monthBuilder.AppendFormat("{0},", Month);
            }

            var MonthNameString = monthBuilder.ToString().TrimEnd(TrimChars);

            // Get culture array of day names and convert to string for
            // passing to the popup calendar
            var dayBuilder = new StringBuilder();
            foreach (string Day in DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames)
            {
                dayBuilder.AppendFormat("{0},", Day);
            }

            var DayNameString = dayBuilder.ToString().TrimEnd(TrimChars);

            // Get the short date pattern for the culture
            string FormatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            //START dnnsoftware.ir
            if (System.Globalization.CultureInfo.CurrentCulture.ToString() == "fa-IR")
            {
                if (!Field.Page.ClientScript.IsClientScriptIncludeRegistered("PersianCalendar.js"))
                {
                    ClientAPI.RegisterClientScriptBlock(Field.Page, "PersianCalendar.js", ("<script src=\"" + ClientAPI.ScriptPath + "PersianCalendar.js\"></script>"));
                    ClientAPI.RegisterClientScriptBlock(Field.Page, "PersianCalendar.css", ("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ClientAPI.ScriptPath + "PersianCalendar.css\" />"));
                }
            }
            else
            {
                if (!Field.Page.ClientScript.IsClientScriptIncludeRegistered("PopupCalendar.js"))
                {
                    ScriptManager.RegisterClientScriptInclude(Field.Page, Field.Page.GetType(), "PopupCalendar.js", ClientAPI.ScriptPath + "PopupCalendar.js");
                }
            }
            //END dnnsoftware.ir

            string strToday = ClientAPI.GetSafeJSString(Localization.GetString("Today"));
            string strClose = ClientAPI.GetSafeJSString(Localization.GetString("Close"));
            string strCalendar = ClientAPI.GetSafeJSString(Localization.GetString("Calendar"));

            //START dnnsoftware.ir
            if (System.Globalization.CultureInfo.CurrentCulture.ToString() == "fa-IR")
            {
                return "javascript:displayDatePicker('" + Field.ClientID + "');";
            }
            //END dnnsoftware.ir

            return string.Concat("javascript:popupCal('Cal','", Field.ClientID, "','", FormatString, "','",
                MonthNameString, "','", DayNameString, "','", strToday, "','", strClose, "','", strCalendar, "',",
                (int)DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek, ");");
        }
    }
}
