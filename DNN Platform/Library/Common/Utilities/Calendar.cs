#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.Common.Utilities
{
    public class Calendar
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Opens a popup Calendar
        /// </summary>
        /// <param name="Field">TextBox to return the date value</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string InvokePopupCal(TextBox Field)
        {
            //Define character array to trim from language strings
            char[] TrimChars = {',', ' '};
            //Get culture array of month names and convert to string for
            //passing to the popup calendar
            var monthBuilder = new StringBuilder();
            foreach (string Month in DateTimeFormatInfo.CurrentInfo.MonthNames)
            {
                monthBuilder.AppendFormat("{0},", Month);
            }
            var MonthNameString = monthBuilder.ToString().TrimEnd(TrimChars);
            //Get culture array of day names and convert to string for
            //passing to the popup calendar
            var dayBuilder = new StringBuilder();
            foreach (string Day in DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames)
            {
                dayBuilder.AppendFormat("{0},", Day);
            }
            var DayNameString = dayBuilder.ToString().TrimEnd(TrimChars);
            //Get the short date pattern for the culture
            string FormatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            if (!Field.Page.ClientScript.IsClientScriptIncludeRegistered("PopupCalendar.js"))
            {
                ScriptManager.RegisterClientScriptInclude(Field.Page, Field.Page.GetType(), "PopupCalendar.js", ClientAPI.ScriptPath + "PopupCalendar.js");
            }
            string strToday = ClientAPI.GetSafeJSString(Localization.GetString("Today"));
            string strClose = ClientAPI.GetSafeJSString(Localization.GetString("Close"));
            string strCalendar = ClientAPI.GetSafeJSString(Localization.GetString("Calendar"));
            return string.Concat("javascript:popupCal('Cal','", Field.ClientID, "','", FormatString, "','",
                MonthNameString, "','", DayNameString, "','", strToday, "','", strClose, "','", strCalendar, "',",
                (int) DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek, ");");
        }
    }
}
