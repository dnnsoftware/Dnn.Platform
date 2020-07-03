// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ControlStyleAttribute : Attribute
    {
        private readonly string _CssClass;
        private readonly Unit _Height;
        private readonly Unit _Width;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.
        /// </summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        public ControlStyleAttribute(string cssClass)
        {
            this._CssClass = cssClass;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.
        /// </summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        /// <param name="width">control width.</param>
        public ControlStyleAttribute(string cssClass, string width)
        {
            this._CssClass = cssClass;
            this._Width = Unit.Parse(width);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.
        /// </summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        /// <param name="width">control width.</param>
        /// <param name="height">control height.</param>
        public ControlStyleAttribute(string cssClass, string width, string height)
        {
            this._CssClass = cssClass;
            this._Height = Unit.Parse(height);
            this._Width = Unit.Parse(width);
        }

        public string CssClass
        {
            get
            {
                return this._CssClass;
            }
        }

        public Unit Height
        {
            get
            {
                return this._Height;
            }
        }

        public Unit Width
        {
            get
            {
                return this._Width;
            }
        }
    }
}
