// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.AspNetClientCapabilityProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;

    using DotNetNuke.Instrumentation;

    /// <summary>AspNet Browser Implementation of <see cref="DotNetNuke.Services.ClientCapability.IClientCapability"/>.</summary>
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

        private static readonly Regex MobileCheck = new Regex(
            @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(2));

        private static readonly Regex TabletRegex = new Regex("ipad|xoom|sch-i800|playbook|tablet|kindle|nexus", RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        private static readonly char[] Separators = { ';', ')' };

        private readonly IDictionary<string, string> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetClientCapability"/> class.
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="browserCaps">The browser capabilities.</param>
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
                this.properties = browserCaps.Capabilities?.Cast<DictionaryEntry>().ToDictionary(kvp => Convert.ToString(kvp.Key, CultureInfo.InvariantCulture), kvp => Convert.ToString(kvp.Value, CultureInfo.InvariantCulture)) ?? new Dictionary<string, string>(0);
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
                    DetectOperatingSystem(lowerAgent, this.properties);
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
        /// <param name="request">The HTTP request.</param>
        public AspNetClientCapability(HttpRequest request)
            : this(request.UserAgent ?? string.Empty, request.Browser)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetClientCapability"/> class.
        /// Constructs a new instance of ClientCapability.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public AspNetClientCapability(IDictionary<string, string> properties)
        {
            this.properties = properties;

            if (this.properties != null)
            {
                // Set Lite properties
                this.ID = GetStringValue(this.properties, "Id");
                this.IsMobile = GetBoolValue(this.properties, "IsMobile");
                this.ScreenResolutionWidthInPixels = GetIntValue(this.properties, "ScreenPixelsWidth");
                this.ScreenResolutionHeightInPixels = GetIntValue(this.properties, "ScreenPixelsHeight");

                // Set Premium properties
                this.IsTablet = GetBoolValue(this.properties, "IsTablet");
                this.IsTouchScreen = GetBoolValue(this.properties, "HasTouchScreen");
                this.BrowserName = GetStringValue(this.properties, "BrowserName");

                this.SupportsFlash = false;
                this.HtmlPreferedDTD = null;

                // set IsMobile to false when IsTablet is true.
                if (this.IsTablet)
                {
                    this.IsMobile = false;
                }
            }
        }

        /// <inheritdoc/>
        public override string this[string name]
        {
            get
            {
                if (this.properties?.TryGetValue(name, out var propertyValue) == true)
                {
                    return propertyValue;
                }

                return string.Empty;
            }
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
            if (properties.TryGetValue(property, out var stringValue) &&
                bool.TryParse(stringValue, out var value))
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
            if (properties.TryGetValue(property, out var stringValue) &&
                int.TryParse(stringValue, out var value))
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
            return properties.TryGetValue(property, out var propertyValue) ? propertyValue : null;
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
