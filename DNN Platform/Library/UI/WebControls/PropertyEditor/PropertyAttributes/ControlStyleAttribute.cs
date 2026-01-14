// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI.WebControls;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ControlStyleAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.</summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        public ControlStyleAttribute(string cssClass)
        {
            this.CssClass = cssClass;
        }

        /// <summary>Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.</summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        /// <param name="width">control width.</param>
        public ControlStyleAttribute(string cssClass, string width)
        {
            this.CssClass = cssClass;
            this.Width = Unit.Parse(width, CultureInfo.InvariantCulture);
        }

        /// <summary>Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.</summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        /// <param name="width">control width.</param>
        /// <param name="height">control height.</param>
        public ControlStyleAttribute(string cssClass, string width, string height)
        {
            this.CssClass = cssClass;
            this.Height = Unit.Parse(height, CultureInfo.InvariantCulture);
            this.Width = Unit.Parse(width, CultureInfo.InvariantCulture);
        }

        public string CssClass { get; }

        public Unit Height { get; }

        public Unit Width { get; }
    }
}
