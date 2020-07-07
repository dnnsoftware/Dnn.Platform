// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatAttribute : Attribute
    {
        private readonly string _Format;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatAttribute"/> class.
        /// </summary>
        public FormatAttribute(string format)
        {
            this._Format = format;
        }

        public string Format
        {
            get
            {
                return this._Format;
            }
        }
    }
}
