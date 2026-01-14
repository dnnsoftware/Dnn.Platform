// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.AspNetClientCapabilityProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web;
    using System.Web.Configuration;

    using DotNetNuke.Common;
    using DotNetNuke.Services.ClientCapability;

    /// <summary>AspNet.BrowserDetector implementation of <see cref="DotNetNuke.Services.ClientCapability.IClientCapabilityProvider"/>.</summary>
    public class AspNetClientCapabilityProvider : ClientCapabilityProvider
    {
        private static readonly object AllCapabilitiesLock = new object();
        private static readonly object AllClientCapabilityValuesLock = new object();
        private static IQueryable<IClientCapability> allCapabilities;
        private static Dictionary<string, List<string>> allClientCapabilityValues;
        private static IDictionary<string, int> highPiorityCapabilityValues;
        private static IDictionary<string, string> sampleProperies;

        /// <inheritdoc/>
        public override bool SupportsTabletDetection
        {
            get { return false; }
        }

        private static IQueryable<IClientCapability> AllCapabilities
        {
            get
            {
                if (allCapabilities == null)
                {
                    lock (AllCapabilitiesLock)
                    {
                        if (allCapabilities == null)
                        {
                            var capabilities = new List<IClientCapability> { new AspNetClientCapability(HttpContext.Current.Request) };
                            allCapabilities = capabilities.AsQueryable();
                        }
                    }
                }

                return allCapabilities;
            }
        }

        private static IDictionary<string, List<string>> ClientCapabilityValues
        {
            get
            {
                if (allClientCapabilityValues == null)
                {
                    lock (AllClientCapabilityValuesLock)
                    {
                        if (allClientCapabilityValues == null)
                        {
                            allClientCapabilityValues = new Dictionary<string, List<string>>();

                            // TODO :Implement
                            // foreach (var property in DataProvider.Properties)
                            // {
                            //    var values = property.Values.Select(value => value.Name).ToList();
                            //    _allClientCapabilityValues.Add(property.Name, values);
                            // }
                        }
                    }

                    var props = HighPiorityCapabilityValues;
                    allClientCapabilityValues = allClientCapabilityValues.OrderByDescending(
                        kvp =>
                        {
                            if (props.ContainsKey(kvp.Key))
                            {
                                return HighPiorityCapabilityValues[kvp.Key];
                            }

                            return 0;
                        }).ThenBy(kvp => kvp.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                return allClientCapabilityValues;
            }
        }

        private static IDictionary<string, int> HighPiorityCapabilityValues
        {
            get
            {
                return highPiorityCapabilityValues ?? (highPiorityCapabilityValues = new Dictionary<string, int>
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

        private static IDictionary<string, string> SampleProperties
        {
            get
            {
                return sampleProperies ??
                       (sampleProperies = new Dictionary<string, string>
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

        /// <summary>Gets the browser capabilities.</summary>
        /// <param name="headers">The headers collection.</param>
        /// <param name="userAgent">The request's user agent.</param>
        /// <returns>An <see cref="HttpBrowserCapabilities"/> instance.</returns>
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

        /// <inheritdoc/>
        public override IClientCapability GetClientCapability(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return new AspNetClientCapability(SampleProperties);
            }

            return new AspNetClientCapability(userAgent, GetHttpBrowserCapabilities(new NameValueCollection(), userAgent));
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override IClientCapability GetClientCapabilityById(string deviceId)
        {
            Requires.NotNullOrEmpty("deviceId", deviceId);

            throw new NotImplementedException($"Can't get device capability for the id '{deviceId}'");
        }

        /// <inheritdoc/>
        public override IDictionary<string, List<string>> GetAllClientCapabilityValues()
        {
            return ClientCapabilityValues;
        }

        /// <inheritdoc/>
        public override IQueryable<IClientCapability> GetAllClientCapabilities()
        {
            return AllCapabilities;
        }
    }
}
