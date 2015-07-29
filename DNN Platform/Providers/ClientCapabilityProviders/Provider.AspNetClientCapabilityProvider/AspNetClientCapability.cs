
﻿#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Instrumentation;
using DotNetNuke.Providers.AspNetClientCapabilityProvider.Properties;

namespace DotNetNuke.Providers.AspNetClientCapabilityProvider
{
    /// <summary>
    /// AspNet Browser Implementation of IClientCapability
    /// </summary>
    public class AspNetClientCapability : Services.ClientCapability.ClientCapability
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AspNetClientCapability));

        #region Constructor

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(HttpBrowserCapabilities browserCaps)
        {
            if (browserCaps != null)
            {
                ID = browserCaps.Id;
                IsMobile = browserCaps.IsMobileDevice;
                ScreenResolutionWidthInPixels = browserCaps.ScreenPixelsWidth;
                ScreenResolutionHeightInPixels = browserCaps.ScreenPixelsHeight;
                IsTablet = false;
                IsTouchScreen = false;
                BrowserName = browserCaps.Browser;
                Capabilities = browserCaps.Capabilities as IDictionary<string, string>;
                SupportsFlash = false;
                HtmlPreferedDTD = null;

                try
                {
                    var d = new Dictionary<string, string>();
                    var e = browserCaps.Capabilities.GetEnumerator();
                    while (e.MoveNext())
                    {
                        var entry = (DictionaryEntry)e.Current;
                        d[entry.Key.ToString()] = entry.Value.ToString();
                    }
                    Capabilities = d;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(IDictionary<string, string> properties)
        {
            Initialise(properties);
        }

        private void Initialise(IDictionary<string, string> properties)
        {
            if (properties != null)
            {
                // Set Lite properties
                ID = GetStringValue(properties, "Id");
                IsMobile = GetBoolValue(properties, "IsMobile");
                ScreenResolutionWidthInPixels = GetIntValue(properties, "ScreenPixelsWidth");
                ScreenResolutionHeightInPixels = GetIntValue(properties, "ScreenPixelsHeight");
                // Set Premium properties
                IsTablet = GetBoolValue(properties, "IsTablet");
                IsTouchScreen = GetBoolValue(properties, "HasTouchScreen");
                BrowserName = GetStringValue(properties, "BrowserName");
                Capabilities = GetCapabilities(properties);

                SupportsFlash = false;
                HtmlPreferedDTD = null;

                //set IsMobile to false when IsTablet is true.
                if (IsTablet)
                    IsMobile = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a dictionary of capability names and values as strings based on the object
        /// keys and values held in the browser capabilities provided. The value string may
        /// contains pipe (|) seperated lists of values.
        /// </summary>
        /// <param name="properties">A collection of device related capabilities.</param>
        /// <returns>Device related capabilities with property names and values converted to strings.</returns>
        private static IDictionary<string, string> GetCapabilities(IDictionary<string, string> properties)
        {
            return properties.Keys.ToDictionary(key => key, key => String.Join(Constants.ValueSeperator, properties[key].ToArray()));
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection
        /// as an boolean.
        /// </summary>
        /// <param name="properties">A collection of device related capabilities.</param>
        /// <param name="property">The name of the property to return as a boolean.</param>
        /// <returns>The boolean value of the property, or false if the property is not found or it's value is not an boolean.</returns>
        private static bool GetBoolValue(IDictionary<string, string> properties, string property)
        {
            bool value;
            if (properties.ContainsKey(property) &&
                bool.TryParse(properties[property], out value))
                return value;
            return false;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection
        /// as an integer.
        /// </summary>
        /// <param name="properties">A collection of device related capabilities.</param>
        /// <param name="property">The name of the property to return as a integer.</param>
        /// <returns>The integer value of the property, or 0 if the property is not found or it's value is not an integer.</returns>
        private static int GetIntValue(IDictionary<string, string> properties, string property)
        {
            int value;
            if (properties.ContainsKey(property) &&
                int.TryParse(properties[property], out value))
                return value;
            return 0;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection
        /// as a string.
        /// </summary>
        /// <param name="properties">A collection of device related properties.</param>
        /// <param name="property">The name of the property to return as a string.</param>
        /// <returns>The string value of the property, or null if the property is not found.</returns>
        private static string GetStringValue(IDictionary<string, string> properties, string property)
        {
            return properties.ContainsKey(property) ? properties[property] : null;
        }

        #endregion
    }
}