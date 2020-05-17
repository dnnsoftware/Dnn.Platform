﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.FileSystem
{
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        private const string FileSizeFormat = "fs";
        private const decimal OneKiloByte = 1024;
        private const decimal OneMegaByte = OneKiloByte * 1024;
        private const decimal OneGigaByte = OneMegaByte * 1024;

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(FileSizeFormat))
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            decimal size;

            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size >= OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = Localization.Localization.GetString("SizeGb");
            }
            else if (size >= OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = Localization.Localization.GetString("SizeMb");
            }
            else if (size >= OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = Localization.Localization.GetString("SizeKb");
            }
            else
            {
                suffix = Localization.Localization.GetString("SizeB");
            }

            return string.Format("{0:N1} {1}", size, suffix);
        }

        private static string DefaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            var formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }

            return arg.ToString();
        }
    }
}
