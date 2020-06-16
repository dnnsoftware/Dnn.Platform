// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.Console.Components
{
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

    /// <summary>
    /// Controls the Console.
    /// </summary>
    public class ConsoleController
    {
        /// <summary>
        /// Gets the size values.
        /// </summary>
        /// <returns>A list with different icon types.</returns>
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
        /// <returns>Show or Hide.</returns>
        public static IList<string> GetViewValues()
        {
            IList<string> returnValue = new List<string>();
            returnValue.Add("Hide");
            returnValue.Add("Show");
            return returnValue;
        }
    }
}
