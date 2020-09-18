// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.AspNetClientCapabilityProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;
    using System.Web.Configuration;

    using DotNetNuke.Common;
    using DotNetNuke.Services.ClientCapability;

    /// <summary>
    /// AspNet.BrowserDetector implementation of ClientCapabilityProvider.
    /// </summary>
    public class AspNetClientCapabilityProvider : ClientCapabilityProvider
    {
        private static readonly object _allCapabilitiesLock = new object();

        private static readonly object _allClientCapabilityValuesLock = new object();
        private static IQueryable<IClientCapability> _allCapabilities;
        private static Dictionary<string, List<string>> _allClientCapabilityValues;

        private static IDictionary<string, int> _highPiorityCapabilityValues;

        private static IDictionary<string, string> _dummyProperies;

        /// <summary>
        /// Gets a value indicating whether indicates whether tablet detection is supported in the available data set.
        /// </summary>
        public override bool SupportsTabletDetection
        {
            get { return false; }
        }

        private static IQueryable<IClientCapability> AllCapabilities
        {
            get
            {
                if (_allCapabilities == null)
                {
                    lock (_allCapabilitiesLock)
                    {
                        if (_allCapabilities == null)
                        {
                            var capabilities = new List<IClientCapability> { new AspNetClientCapability(HttpContext.Current.Request) };
                            _allCapabilities = capabilities.AsQueryable();
                        }
                    }
                }

                return _allCapabilities;
            }
        }

        private static IDictionary<string, List<string>> ClientCapabilityValues
        {
            get
            {
                if (_allClientCapabilityValues == null)
                {
                    lock (_allClientCapabilityValuesLock)
                    {
                        if (_allClientCapabilityValues == null)
                        {
                            _allClientCapabilityValues = new Dictionary<string, List<string>>();

                            // TODO :Implement
                            // foreach (var property in DataProvider.Properties)
                            // {
                            //    var values = property.Values.Select(value => value.Name).ToList();
                            //    _allClientCapabilityValues.Add(property.Name, values);
                            // }
                        }
                    }

                    var props = HighPiorityCapabilityValues;
                    _allClientCapabilityValues = _allClientCapabilityValues.OrderByDescending(
                        kvp =>
                        {
                            if (props.ContainsKey(kvp.Key))
                            {
                                return HighPiorityCapabilityValues[kvp.Key];
                            }

                            return 0;
                        }).ThenBy(kvp => kvp.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                return _allClientCapabilityValues;
            }
        }

        private static IDictionary<string, int> HighPiorityCapabilityValues
        {
            get
            {
                return _highPiorityCapabilityValues ?? (_highPiorityCapabilityValues = new Dictionary<string, int>
                {
                    { "IsMobile", 100 },
                    { "IsTablet", 95 },
                    { "PlatformName", 90 },
                    { "BrowserName", 85 },
                    { "BrowserVersion", 80 },
                    { "HasTouchScreen", 75 },
                    { "PlatformVersion", 70 },
                    { "ScreenPixelsWidth", 65 },
                    { "ScreenPixelsHeight", 60 },
                    { "HardwareVendor", 55 },
                });
            }
        }

        private static IDictionary<string, string> DummyProperties
        {
            get
            {
                return _dummyProperies ??
                       (_dummyProperies = new Dictionary<string, string>
                       {
                           { "Id", "UNKNOWN" },
                           { "IsMobile", "false" },
                           { "ScreenPixelsWidth", "600" },
                           { "ScreenPixelsHeight", "800" },
                           { "IsTablet", "false" },
                           { "HasTouchScreen", "false" },
                           { "BrowserName", "???" },
                           { "SupportsFlash", "false" },
                       });
            }
        }

        public static HttpBrowserCapabilities GetHttpBrowserCapabilities(NameValueCollection headers, string userAgent)
        {
            var factory = new BrowserCapabilitiesFactory();
            var browserCaps = new HttpBrowserCapabilities();
            var hashtable = new Hashtable(180, StringComparer.OrdinalIgnoreCase);
            hashtable[string.Empty] = userAgent;
            browserCaps.Capabilities = hashtable;
            factory.ConfigureBrowserCapabilities(headers, browserCaps);
            factory.ConfigureCustomCapabilities(headers, browserCaps);
            return browserCaps;
        }

        /// <summary>
        /// Returns ClientCapability based on the user agent provided.
        /// </summary>
        /// <returns></returns>
        public override IClientCapability GetClientCapability(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return new AspNetClientCapability(DummyProperties);
            }

            return new AspNetClientCapability(userAgent, GetHttpBrowserCapabilities(new NameValueCollection(), userAgent));
        }

        /// <summary>
        /// Returns ClientCapability based on device Id provided.
        /// </summary>
        /// <returns></returns>
        public override IClientCapability GetClientCapabilityById(string deviceId)
        {
            Requires.NotNullOrEmpty("deviceId", deviceId);

            throw new NotImplementedException($"Can't get device capability for the id '{deviceId}'");
        }

        /// <summary>
        /// Returns available Capability Values for every Capability Name.
        /// </summary>
        /// <returns>
        /// Dictionary of Capability Name along with List of possible values of the Capability.
        /// </returns>
        /// <example>Capability Name = mobile_browser, value = Safari, Andriod Webkit. </example>
        public override IDictionary<string, List<string>> GetAllClientCapabilityValues()
        {
            return ClientCapabilityValues;
        }

        /// <summary>
        /// Returns All available Client Capabilities present.
        /// </summary>
        /// <returns>
        /// List of IClientCapability present.
        /// </returns>
        public override IQueryable<IClientCapability> GetAllClientCapabilities()
        {
            return AllCapabilities;
        }
    }
}
