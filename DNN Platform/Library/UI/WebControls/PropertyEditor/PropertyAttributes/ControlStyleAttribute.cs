#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
