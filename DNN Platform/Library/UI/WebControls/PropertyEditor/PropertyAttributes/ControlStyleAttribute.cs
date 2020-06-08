// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ControlStyleAttribute : Attribute
    {
        private readonly string _CssClass;
        private readonly Unit _Height;
        private readonly Unit _Width;

        /// <summary>
        /// Initializes a new instance of the StyleAttribute class.
        /// </summary>
        /// <param name="cssClass">The css class to apply to the associated property</param>
        public ControlStyleAttribute(string cssClass)
        {
            _CssClass = cssClass;
        }

        /// <summary>
        /// Initializes a new instance of the StyleAttribute class.
        /// </summary>
        /// <param name="cssClass">The css class to apply to the associated property</param>
        /// <param name="width">control width.</param>
        public ControlStyleAttribute(string cssClass, string width)
        {
            _CssClass = cssClass;
            _Width = Unit.Parse(width);
        }

        /// <summary>
        /// Initializes a new instance of the StyleAttribute class.
        /// </summary>
        /// <param name="cssClass">The css class to apply to the associated property</param>
		/// <param name="width">control width.</param>
		/// <param name="height">control height.</param>
        public ControlStyleAttribute(string cssClass, string width, string height)
        {
            _CssClass = cssClass;
            _Height = Unit.Parse(height);
            _Width = Unit.Parse(width);
        }

        public string CssClass
        {
            get
            {
                return _CssClass;
            }
        }

        public Unit Height
        {
            get
            {
                return _Height;
            }
        }

        public Unit Width
        {
            get
            {
                return _Width;
            }
        }
    }
}
