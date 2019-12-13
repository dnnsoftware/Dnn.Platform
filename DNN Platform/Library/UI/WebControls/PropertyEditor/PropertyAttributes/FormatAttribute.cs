// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatAttribute : Attribute
    {
        private readonly string _Format;

        /// <summary>
        /// Initializes a new instance of the FormatAttribute class.
        /// </summary>
        public FormatAttribute(string format)
        {
            _Format = format;
        }

        public string Format
        {
            get
            {
                return _Format;
            }
        }
    }
}
