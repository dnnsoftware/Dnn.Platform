// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.AspNetClientCapabilityProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Providers.AspNetClientCapabilityProvider.Properties;

    /// <summary>
    /// AspNet Browser Implementation of IClientCapability.
    /// </summary>
    public class AspNetClientCapability : Services.ClientCapability.ClientCapability
    {
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

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AspNetClientCapability));

        private static readonly Regex MobileCheck =
            new Regex(
                @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));

        private static readonly Regex TabletRegex = new Regex("ipad|xoom|sch-i800|playbook|tablet|kindle|nexus", RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        private static readonly char[] Separators = { ';', ')' };

        private readonly IDictionary<string, string> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetClientCapability"/> class.
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(string userAgent, HttpCapabilitiesBase browserCaps)
        {
            this.UserAgent = userAgent;

            if (browserCaps != null)
            {
                this.ID = browserCaps.Id;
                this.ScreenResolutionWidthInPixels = browserCaps.ScreenPixelsWidth;
                this.ScreenResolutionHeightInPixels = browserCaps.ScreenPixelsHeight;
                this.IsTouchScreen = false;
                this.BrowserName = browserCaps.Browser;
                if (browserCaps.Capabilities != null)
                {
                    this.Capabilities = browserCaps.Capabilities.Cast<DictionaryEntry>()
                        .ToDictionary(kvp => Convert.ToString(kvp.Key), kvp => Convert.ToString(kvp.Value));
                }
                else
                {
                    this.Capabilities = new Dictionary<string, string>();
                }

                this.SupportsFlash = false;
                this.HtmlPreferedDTD = null;

                if (this.UserAgent.Length < 4)
                {
                    return;
                }

                var lowerAgent = this.UserAgent.ToLowerInvariant();
                this.IsMobile = browserCaps.IsMobileDevice || GetIfMobile(lowerAgent);
                this.IsTablet = GetIfTablet(lowerAgent);

                try
                {
                    DetectOperatingSystem(lowerAgent, this.Capabilities);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetClientCapability"/> class.
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(HttpRequest request)
            : this(request.UserAgent ?? string.Empty, request.Browser)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetClientCapability"/> class.
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        public AspNetClientCapability(IDictionary<string, string> properties)
        {
            this._properties = properties;

            if (this._properties != null)
            {
                // Set Lite properties
                this.ID = GetStringValue(this._properties, "Id");
                this.IsMobile = GetBoolValue(this._properties, "IsMobile");
                this.ScreenResolutionWidthInPixels = GetIntValue(this._properties, "ScreenPixelsWidth");
                this.ScreenResolutionHeightInPixels = GetIntValue(this._properties, "ScreenPixelsHeight");

                // Set Premium properties
                this.IsTablet = GetBoolValue(this._properties, "IsTablet");
                this.IsTouchScreen = GetBoolValue(this._properties, "HasTouchScreen");
                this.BrowserName = GetStringValue(this._properties, "BrowserName");
                this.Capabilities = GetCapabilities(this._properties);

                this.SupportsFlash = false;
                this.HtmlPreferedDTD = null;

                // set IsMobile to false when IsTablet is true.
                if (this.IsTablet)
                {
                    this.IsMobile = false;
                }
            }
        }

        public override string this[string name]
        {
            get
            {
                if (this._properties != null && this._properties.ContainsKey(name))
                {
                    return this._properties[name];
                }

                return (this.Capabilities != null && this.Capabilities.ContainsKey(name)) ? this.Capabilities[name] : string.Empty;
            }
        }

        /// <summary>
        /// Returns a dictionary of capability names and values as strings based on the object
        /// keys and values held in the browser capabilities provided. The value string may
        /// contains pipe (|) seperated lists of values.
        /// </summary>
        /// <param name="properties">A collection of device related capabilities.</param>
        /// <returns>Device related capabilities with property names and values converted to strings.</returns>
        private static IDictionary<string, string> GetCapabilities(IDictionary<string, string> properties)
        {
            return properties.Keys.ToDictionary(key => key, key => string.Join(Constants.ValueSeperator, properties[key].ToArray()));
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
            {
                return value;
            }

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
            {
                return value;
            }

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
                // TODO: detect others. maybe through an external service such as:
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
                    // platformName = "Windows Server 2003; Windows XP x64 Edition";
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
    }
}
