// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization.Persian;

using System;

public class PersianCalendar : System.Globalization.PersianCalendar
{
    /// <inheritdoc/>
    public override int GetYear(DateTime time)
    {
        try
        {
            return base.GetYear(time);
        }
        catch
        {
            // ignore
        }

        return time.Year;
    }

    /// <inheritdoc/>
    public override int GetMonth(DateTime time)
    {
        try
        {
            return base.GetMonth(time);
        }
        catch
        {
            // ignore
        }

        return time.Month;
    }

    /// <inheritdoc/>
    public override int GetDayOfMonth(DateTime time)
    {
        try
        {
            return base.GetDayOfMonth(time);
        }
        catch
        {
            // ignore
        }

        return time.Day;
    }

    /// <inheritdoc/>
    public override int GetDayOfYear(DateTime time)
    {
        try
        {
            return base.GetDayOfYear(time);
        }
        catch
        {
            // ignore
        }

        return time.DayOfYear;
    }

    /// <inheritdoc/>
    public override DayOfWeek GetDayOfWeek(DateTime time)
    {
        try
        {
            return base.GetDayOfWeek(time);
        }
        catch
        {
            // ignore
        }

        return time.DayOfWeek;
    }
}
