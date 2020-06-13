// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper
{
    using System;

    using Dnn.PersonaBar.Library.Dto;

    public class TimeZoneHelper
    {
        public static TimeZoneDto GetPreferredTimeZone(TimeZoneInfo userTimeZone)
        {
            return new TimeZoneDto()
            {
                Id = userTimeZone.Id,
                DisplayName = userTimeZone.DisplayName,
                BaseUtcOffset = FormatOffset(userTimeZone.BaseUtcOffset),
                CurrentUtcOffset = FormatOffset(userTimeZone.GetUtcOffset(DateTime.UtcNow)),
            };
        }

        private static string FormatOffset(TimeSpan time)
        {
            return ((time < TimeSpan.Zero) ? "-" : "+") + time.ToString(@"hh\:mm");
        }
    }
}
