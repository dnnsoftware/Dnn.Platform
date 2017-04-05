using System;
using Dnn.PersonaBar.Library.Dto;

namespace Dnn.PersonaBar.Library.Helper
{
    public class TimeZoneHelper
    {
        public static TimeZoneDto GetPreferredTimeZone(TimeZoneInfo userTimeZone)
        {
            return new TimeZoneDto()
            {
                Id = userTimeZone.Id,
                DisplayName = userTimeZone.DisplayName,
                BaseUtcOffset = FormatOffset(userTimeZone.BaseUtcOffset),
                CurrentUtcOffset = FormatOffset(userTimeZone.GetUtcOffset(DateTime.UtcNow))
            };
        }

        private static string FormatOffset(TimeSpan time)
        {
            return ((time < TimeSpan.Zero) ? "-" : "+") + time.ToString(@"hh\:mm");
        }
    }
}