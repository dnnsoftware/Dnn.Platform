// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Common
{
    using System;

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
