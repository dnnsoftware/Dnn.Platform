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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
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
        private readonly IDictionary<string, string> _properties;

        public override string this[string name]
        {
            get
            {
                if (_properties != null && _properties.ContainsKey(name))
                {
                    return _properties[name];
                }

                return (Capabilities != null && Capabilities.ContainsKey(name)) ? Capabilities[name] : string.Empty;
            }
        }

        #region Constructor

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(string userAgent, HttpCapabilitiesBase browserCaps)
        {
            UserAgent = userAgent;

            if (browserCaps != null)
            {
                ID = browserCaps.Id;
                ScreenResolutionWidthInPixels = browserCaps.ScreenPixelsWidth;
                ScreenResolutionHeightInPixels = browserCaps.ScreenPixelsHeight;
                IsTouchScreen = false;
                BrowserName = browserCaps.Browser;
                if(browserCaps.Capabilities != null)
                { 
                    Capabilities = browserCaps.Capabilities.Cast<DictionaryEntry>()
                        .ToDictionary(kvp => Convert.ToString(kvp.Key), kvp => Convert.ToString(kvp.Value));
                }
                else
                {
                    Capabilities = new Dictionary<string, string>();
                }
                SupportsFlash = false;
                HtmlPreferedDTD = null;

                if (UserAgent.Length < 4)
                {
                    return;
                }

                var lowerAgent = UserAgent.ToLowerInvariant();
                IsMobile = browserCaps.IsMobileDevice || GetIfMobile(lowerAgent);
                IsTablet = GetIfTablet(lowerAgent);

                try
                {
                    DetectOperatingSystem(lowerAgent, Capabilities);
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
        public AspNetClientCapability(HttpRequest request) : this(request.UserAgent ?? "", request.Browser)
        {
            
        }

        /// <summary>
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(IDictionary<string, string> properties)
        {
            _properties = properties;

            if (_properties != null)
            {
                // Set Lite properties
                ID = GetStringValue(_properties, "Id");
                IsMobile = GetBoolValue(_properties, "IsMobile");
                ScreenResolutionWidthInPixels = GetIntValue(_properties, "ScreenPixelsWidth");
                ScreenResolutionHeightInPixels = GetIntValue(_properties, "ScreenPixelsHeight");
                // Set Premium properties
                IsTablet = GetBoolValue(_properties, "IsTablet");
                IsTouchScreen = GetBoolValue(_properties, "HasTouchScreen");
                BrowserName = GetStringValue(_properties, "BrowserName");
                Capabilities = GetCapabilities(_properties);

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

        private static readonly Regex MobileCheck =
            new Regex(
                @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));

        private static readonly Regex TabletRegex = new Regex("ipad|xoom|sch-i800|playbook|tablet|kindle|nexus", RegexOptions.Compiled, TimeSpan.FromSeconds(2));

        private static bool GetIfMobile(string userAgent)
        {
            return MobileCheck.IsMatch(userAgent);
        }

        private static bool GetIfTablet(string userAgent)
        {
            return TabletRegex.IsMatch(userAgent);
        }

        private static void DetectOperatingSystem(string userAgent, IDictionary<string, string> properties)
        {
            var platformVendor = string.Empty;
            string platformName;
            var platformVersion = string.Empty;

            if (CheckAgentAndVersion(WindowsPcAgent, userAgent, ref platformVersion))
            {
                platformVendor = "Microsoft";
                platformName = "Windows";
                platformVersion = MapAgentVersionToWinVersion(platformVersion);
            }
            else if (CheckAgentAndVersion(AndroidAgent, userAgent, ref platformVersion))
            {
                platformName = "Android";
            }
            else if (CheckAgentAndVersion(IphoneAgent, userAgent, ref platformVersion))
            {
                platformVendor = "Apple";
                platformName = "iPhone";
                platformVersion = platformVersion.Replace('_', '.');
            }
            else if (CheckAgentAndVersion(IpadAgent, userAgent, ref platformVersion))
            {
                platformVendor = "Apple";
                platformName = "iPad";
                platformVersion = platformVersion.Replace('_', '.');
            }
            else if (CheckAgentAndVersion(MacOsxAgent, userAgent, ref platformVersion))
            {
                platformVendor = "Apple";
                platformName = "Mac OS";
                platformVersion = platformVersion.Replace('_', '.');
            }
            else if (CheckAgentAndVersion(WindowsPhoneAgent, userAgent, ref platformVersion))
            {
                platformVendor = "Microsoft";
                platformName = "Windows Phone";
            }
            else if (CheckAgentAndVersion(LinuxAgent, userAgent, ref platformVersion))
            {
                platformName = "Linux";
            }
            else if (CheckAgentAndVersion(UnixAgent, userAgent, ref platformVersion) ||
                     CheckAgentAndVersion(UnixAgent2, userAgent, ref platformVersion) ||
                     CheckAgentAndVersion(UnixAgent3, userAgent, ref platformVersion) ||
                     CheckAgentAndVersion(X11Agent, userAgent, ref platformVersion))
            {
                platformName = "Unix";
            }
            else
            {
                //TODO: detect others. maybe through an external service such as:
                // http://www.useragentstring.com/pages/api.php
                // or see this thread: http://www.geekpedia.com/code47_Detect-operating-system-from-user-agent-string.html
                // or port this open source PHP project to C# from https://github.com/piwik/device-detector
                platformName = "Other/Unknown";
            }

            if (!properties.ContainsKey("PlatformVendor"))
            {
                properties.Add("PlatformVendor", platformVendor);
            }
            if (!properties.ContainsKey("PlatformName"))
            {
                properties.Add("PlatformName", platformName);
            }
            if (!properties.ContainsKey("PlatformVersion"))
            {
                properties.Add("PlatformVersion", platformVersion);
            }
        }

        private static string MapAgentVersionToWinVersion(string version)
        {
            switch (version)
            {
                case "10.0":
                    return "10";
                case "6.3":
                    return "8.1";
                case "6.2":
                    return "8";
                case "6.1":
                    return "7";
                case "6.0":
                    return "Vista";
                case "5.2":
                    //platformName = "Windows Server 2003; Windows XP x64 Edition";
                    return "XP x64 Edition";
                case "5.1":
                    return "XP";
                case "5.01":
                    return "2000, Service Pack 1 (SP1)";
                case "5.0":
                    return "2000";
                default:
                    // fallback for all older than above or newer versions
                    return "NT " + version;
            }
        }

        // set all agent identifiers are in lowercase for faster comparison
        private const string WindowsPcAgent = "windows nt";
        private const string WindowsPhoneAgent = "windows phone";
        private const string AndroidAgent = "android";
        private const string IphoneAgent = "iphone";
        private const string IpadAgent = "ipad";
        private const string MacOsxAgent = "mac os x";
        private const string LinuxAgent = "linux ";
        private const string UnixAgent = "i686";
        private const string UnixAgent2 = "i586";
        private const string UnixAgent3 = "i386";
        private const string X11Agent = "x11";
        private static readonly char[] Separators = {';', ')'};

        private static bool CheckAgentAndVersion(string queryAgent, string userAgent, ref string version)
        {
            // user agent must be passed in lowercase
            var index = userAgent.IndexOf(queryAgent, StringComparison.InvariantCulture);
            if (index < 0)
            {
                return false;
            }

            if (queryAgent != X11Agent)
            {
                var v = userAgent.Substring(index + queryAgent.Length).TrimStart(Separators);
                index = v.IndexOfAny(Separators);
                if (index > 0)
                {
                    version = v.Substring(0, index);
                }
            }

            return true;
        }

        #endregion
    }
}