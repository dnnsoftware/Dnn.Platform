#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;

#endregion

namespace Dnn.Modules.Console.Components
{
    /// <summary>
    /// Controls the Console
    /// </summary>
    public class ConsoleController
    {
        /// <summary>
        /// Gets the size values.
        /// </summary>
        /// <returns>A list with different icon types</returns>
        public static IList<string> GetSizeValues()
        {
            IList<string> returnValue = new List<string>();
            returnValue.Add("IconFile");
            returnValue.Add("IconFileLarge");
            returnValue.Add("IconNone");
            return returnValue;
        }

        /// <summary>
        /// Gets the view values.
        /// </summary>
        /// <returns>Show or Hide</returns>
        public static IList<string> GetViewValues()
        {
            IList<string> returnValue = new List<string>();
            returnValue.Add("Hide");
            returnValue.Add("Show");
            return returnValue;
        }
    }
}