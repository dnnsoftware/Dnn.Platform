// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities;

using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

public class Calendar
{
    /// <summary>Opens a popup Calendar.</summary>
    /// <param name="field">TextBox to return the date value.</param>
    /// <returns>A JavaScript URL.</returns>
    public static string InvokePopupCal(TextBox field)
    {
        // Define character array to trim from language strings
        char[] trimChars = { ',', ' ' };

        // Get culture array of month names and convert to string for
        // passing to the popup calendar
        var monthBuilder = new StringBuilder();
        foreach (string month in DateTimeFormatInfo.CurrentInfo.MonthNames)
        {
            monthBuilder.AppendFormat("{0},", month);
        }

        var monthNameString = monthBuilder.ToString().TrimEnd(trimChars);

        // Get culture array of day names and convert to string for
        // passing to the popup calendar
        var dayBuilder = new StringBuilder();
        foreach (string day in DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames)
        {
            dayBuilder.AppendFormat("{0},", day);
        }

        var dayNameString = dayBuilder.ToString().TrimEnd(trimChars);

        // Get the short date pattern for the culture
        string formatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
        if (!field.Page.ClientScript.IsClientScriptIncludeRegistered("PopupCalendar.js"))
        {
            ScriptManager.RegisterClientScriptInclude(field.Page, field.Page.GetType(), "PopupCalendar.js", ClientAPI.ScriptPath + "PopupCalendar.js");
        }

        string strToday = ClientAPI.GetSafeJSString(Localization.GetString("Today"));
        string strClose = ClientAPI.GetSafeJSString(Localization.GetString("Close"));
        string strCalendar = ClientAPI.GetSafeJSString(Localization.GetString("Calendar"));
        return
            string.Format(
                "javascript:popupCal('Cal','{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7});",
                HttpUtility.JavaScriptStringEncode(field.ClientID),
                HttpUtility.JavaScriptStringEncode(formatString),
                HttpUtility.JavaScriptStringEncode(monthNameString),
                HttpUtility.JavaScriptStringEncode(dayNameString),
                HttpUtility.JavaScriptStringEncode(strToday),
                HttpUtility.JavaScriptStringEncode(strClose),
                HttpUtility.JavaScriptStringEncode(strCalendar),
                (int)DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
    }
}
