// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
