using System;

namespace Dnn.PersonaBar.Prompt.Common
{
    public static class Extensions
    {
        public static string ToPromptShortDateString(this DateTime input)
        {
            return input.ToString("yyyy-MM-dd");
        }
        public static string ToPromptShortDateAndTimeString(this DateTime input)
        {
            return input.ToString("yyyy-MM-dd HH:mm");
        }
        public static string ToPromptLongDateString(this DateTime input)
        {
            return input.ToString("F");
        }
    }
}