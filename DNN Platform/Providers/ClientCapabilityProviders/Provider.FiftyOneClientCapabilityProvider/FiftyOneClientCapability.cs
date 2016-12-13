
﻿#region Copyright
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

using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider
{
    /// <summary>
    /// 51Degrees.mobi Implementation of IClientCapability
    /// </summary>
    public class FiftyOneClientCapability : Services.ClientCapability.ClientCapability
    {
        #region Fields

        private readonly Match _match;
        private readonly Profile[] _profiles;
        private readonly HttpBrowserCapabilities _caps;

        #endregion

        #region Properties

        public override string this[string name]
        {
            get
            {
                if (_match != null)
                {
                    return string.Join(Constants.ValueSeperator, _match[name]);
                }
                else if (_profiles != null && _profiles.Any())
                {
                    return string.Join(Constants.ValueSeperator, _profiles[0][name]);
                }
                else if(_caps != null)
                {
                    var capabilities =
                        _caps.Capabilities[Constants.FiftyOneDegreesProperties] as SortedList<string, string[]>;

                    if (capabilities != null)
                    {
                        return string.Join(Constants.ValueSeperator, capabilities[name]);
                    }
                }

                return string.Empty;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// See http://51degrees.mobi/Products/DeviceData/PropertyDictionary.aspx
        /// for a full list of available properties.
        /// All the properties used are non-lists and therefore the first
        /// item contained in the values list contains the only available value.
        /// </summary>
        /// <param name="caps">Reference to browser capabilities for .NET</param>
        public FiftyOneClientCapability(HttpBrowserCapabilities caps)
        {
            _caps = caps;

            // Set Lite properties
            ID = _caps.Id;
            IsMobile = caps.IsMobileDevice;
            ScreenResolutionWidthInPixels = caps.ScreenPixelsWidth;
            ScreenResolutionHeightInPixels = caps.ScreenPixelsHeight;

            // Set Premium properties which are not available.
            IsTablet = false;
            IsTouchScreen = false;
            BrowserName = _caps.Browser;

            // The following properties are not provided by 51Degrees and
            // are therefore set to default values.
            SupportsFlash = false;
            HtmlPreferedDTD = null;

            // set IsMobile to false when IsTablet is true.
            if (IsTablet)
            {
                IsMobile = false;
            }
        }

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// See http://51degrees.mobi/Products/DeviceData/PropertyDictionary.aspx
        /// for a full list of available properties.
        /// All the properties used are non-lists and therefore the first
        /// item contained in the values list contains the only available value.
        /// </summary>
        /// <param name="profiles">Reference to a profile contained in the dataset</param>
        public FiftyOneClientCapability(Profile[] profiles)
        {
            _profiles = profiles;

            // Set Lite properties
            ID = GetStringValue(_profiles, "Id");
            IsMobile = GetBoolValue(_profiles, "IsMobile");
            ScreenResolutionWidthInPixels = GetIntValue(_profiles, "ScreenPixelsWidth");
            ScreenResolutionHeightInPixels = GetIntValue(_profiles, "ScreenPixelsHeight");

            // Set Premium properties
            IsTablet = GetBoolValue(_profiles, "IsTablet");
            IsTouchScreen = GetBoolValue(_profiles, "HasTouchScreen");
            BrowserName = GetStringValue(_profiles, "BrowserName");

            // The following properties are not provided by 51Degrees and
            // are therefore set to default values.
            SupportsFlash = false;
            HtmlPreferedDTD = null;

            // set IsMobile to false when IsTablet is true.
            if (IsTablet)
            {
                IsMobile = false;
            }
        }

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// See http://51degrees.mobi/Products/DeviceData/PropertyDictionary.aspx
        /// for a full list of available properties.
        /// All the properties used are non-lists and therefore the first
        /// item contained in the values list contains the only available value.
        /// </summary>
        /// <param name="match">Reference to the capabilities returned by the detection</param>
        public FiftyOneClientCapability(Match match)
        {
            _match = match;

            // Set Lite properties
            UserAgent = match.TargetUserAgent;
            ID = GetStringValue(_match, "Id");
            IsMobile = GetBoolValue(_match, "IsMobile");
            ScreenResolutionWidthInPixels = GetIntValue(_match, "ScreenPixelsWidth");
            ScreenResolutionHeightInPixels = GetIntValue(_match, "ScreenPixelsHeight");
            
            // Set Premium properties
            IsTablet = GetBoolValue(_match, "IsTablet");
            IsTouchScreen = GetBoolValue(_match, "HasTouchScreen");
            BrowserName = GetStringValue(_match, "BrowserName");

            // The following properties are not provided by 51Degrees and
            // are therefore set to default values.
            SupportsFlash = false;
            HtmlPreferedDTD = null;

            // set IsMobile to false when IsTablet is true.
            if (IsTablet)
            {
                IsMobile = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as an boolean.
        /// </summary>
        /// <param name="profiles">Profiles from the data set either as a result of a match, or from quering profiles.</param>
        /// <param name="propertyName">The name of the property to return as a boolean.</param>
        /// <returns>The boolean value of the property, or false if the property is not found or it's value is not an boolean.</returns>
        private static bool GetBoolValue(IEnumerable<Profile> profiles, string propertyName)
        {
            Values value;
            var e = profiles.GetEnumerator();
            while (e.MoveNext())
            {
                value = e.Current[propertyName];
                if (value != null && value.Count > 0 &&
                    value[0].Property.ValueType == typeof(bool))
                {
                    return value.ToBool();
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as an integer.
        /// </summary>
        /// <param name="profiles">Profiles from the data set either as a result of a match, or from quering profiles.</param>
        /// <param name="propertyName">The name of the property to return as a integer.</param>
        /// <returns>The integer value of the property, or 0 if the property is not found or it's value is not an integer.</returns>
        private static int GetIntValue(IEnumerable<Profile> profiles, string propertyName)
        {
            Values value;
            var e = profiles.GetEnumerator();
            while (e.MoveNext())
            {
                value = e.Current[propertyName];
                if (value != null && value.Count > 0 &&
                    (value[0].Property.ValueType == typeof(int) ||
                    value[0].Property.ValueType == typeof(double)))
                {
                    return (int)value.ToInt();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as a string.
        /// </summary>
        /// <param name="profiles">Profiles from the data set either as a result of a match, or from quering profiles.</param>
        /// <param name="propertyName">The name of the property to return as a string.</param>
        /// <returns>The string value of the property, or null if the property is not found.</returns>
        private static string GetStringValue(IEnumerable<Profile> profiles, string propertyName)
        {
            Values value;
            var e = profiles.GetEnumerator();
            while (e.MoveNext())
            {
                value = e.Current[propertyName];
                if (value != null)
                {
                    return value.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as an boolean.
        /// </summary>
        /// <param name="match">A collection of device related capabilities.</param>
        /// <param name="property">The name of the property to return as a boolean.</param>
        /// <returns>The boolean value of the property, or false if the property is not found or it's value is not an boolean.</returns>
        private static bool GetBoolValue(Match match, string property)
        {
            var value = match[property];
            if (value != null && value.Count > 0 && 
                value[0].Property.ValueType == typeof(bool))
            {
                return value.ToBool();
            }
            return false;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as an integer.
        /// </summary>
        /// <param name="match">A collection of device related capabilities.</param>
        /// <param name="property">The name of the property to return as a integer.</param>
        /// <returns>The integer value of the property, or 0 if the property is not found or it's value is not an integer.</returns>
        private static int GetIntValue(Match match, string property)
        {
            var value = match[property];
            if (value != null && value.Count > 0 &&
                (value[0].Property.ValueType == typeof(int) ||
                value[0].Property.ValueType == typeof(double)))
            {
                return (int)value.ToInt();
            }
            return 0;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as a string.
        /// </summary>
        /// <param name="match">A collection of device related properties.</param>
        /// <param name="property">The name of the property to return as a string.</param>
        /// <returns>The string value of the property, or null if the property is not found.</returns>
        private static string GetStringValue(Match match, string property)
        {
            var value = match[property];
            if (value != null)
            {
                return value.ToString();
            }
            return null;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as an boolean.
        /// </summary>
        /// <param name="caps">A collection of device related properties.</param>
        /// <param name="property">The name of the property to return as a boolean.</param>
        /// <returns>The boolean value of the property, or false if the property is not found or it's value is not an boolean.</returns>
        private static bool GetBoolValue(HttpBrowserCapabilities caps, string property)
        {
            bool value = false;
            bool.TryParse(caps[property], out value);
            return value;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as an integer.
        /// </summary>
        /// <param name="caps">A collection of device related properties.</param>
        /// <param name="property">The name of the property to return as a integer.</param>
        /// <returns>The integer value of the property, or 0 if the property is not found or it's value is not an integer.</returns>
        private static int GetIntValue(HttpBrowserCapabilities caps, string property)
        {
            int value;
            int.TryParse(caps[property], out value);
            return value;
        }

        /// <summary>
        /// Returns the property of the HttpBrowserCapabilities collection 
        /// as a string.
        /// </summary>
        /// <param name="caps">A collection of device related properties.</param>
        /// <param name="property">The name of the property to return as a string.</param>
        /// <returns>The string value of the property, or null if the property is not found.</returns>
        private static string GetStringValue(HttpBrowserCapabilities caps, string property)
        {
            return caps[property];
        }

        #endregion
    }
}
