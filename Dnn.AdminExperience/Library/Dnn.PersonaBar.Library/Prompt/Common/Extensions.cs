// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.PersonaBar.Library.Prompt.Common
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
