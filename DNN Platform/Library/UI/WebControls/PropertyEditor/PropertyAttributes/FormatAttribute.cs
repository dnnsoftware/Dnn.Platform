﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatAttribute : Attribute
    {
        private readonly string format;

        /// <summary>Initializes a new instance of the <see cref="FormatAttribute"/> class.</summary>
        /// <param name="format">The format string to use to format the corresponding date/time.</param>
        public FormatAttribute(string format)
        {
            this.format = format;
        }

        public string Format
        {
            get
            {
                return this.format;
            }
        }
    }
}
