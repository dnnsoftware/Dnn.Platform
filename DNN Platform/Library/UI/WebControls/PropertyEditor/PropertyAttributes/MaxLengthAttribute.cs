

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

using DotNetNuke.Common.Utilities;

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MaxLengthAttribute : Attribute
    {
        private readonly int _Length = Null.NullInteger;

        /// <summary>
        /// Initializes a new instance of the MaxLengthAttribute class.
        /// </summary>
        public MaxLengthAttribute(int length)
        {
            this._Length = length;
        }

        public int Length
        {
            get
            {
                return this._Length;
            }
        }
    }
}
