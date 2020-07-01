// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LabelModeAttribute : Attribute
    {
        private readonly LabelMode _Mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelModeAttribute"/> class.
        /// </summary>
        /// <param name="mode">The label mode to apply to the associated property.</param>
        public LabelModeAttribute(LabelMode mode)
        {
            this._Mode = mode;
        }

        public LabelMode Mode
        {
            get
            {
                return this._Mode;
            }
        }
    }
}
