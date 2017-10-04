#region Copyright
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

#region Usings

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;

using FiftyOne.Foundation.Mobile.Detection;

using System.Web;

using FiftyOne.Foundation.Mobile;

using DotNetNuke.Services.ClientCapability;

using FiftyOne.Foundation.UI;
using FiftyOne.Foundation.Mobile.Detection.Entities;

#endregion

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider
{
    /// <summary>
    /// 51Degrees.mobi implementation of ClientCapabilityProvider
    /// </summary>
    public class FiftyOneClientCapabilityProvider : ClientCapabilityProvider
    {
        #region Static Methods

        static readonly bool _51DegreesEnabled = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled;
        static readonly object _allCapabilitiesLock = new object();
        static IQueryable<IClientCapability> _allCapabilities;

        static readonly object _allClientCapabilityValuesLock = new object();
        static Dictionary<string, List<string>> _allClientCapabilityValues;

        private static IDictionary<string, int> _highPiorityCapabilityValues;

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
                            var capabilities = WebProvider.ActiveProvider.DataSet.Hardware.Profiles.Select(profile =>
                                    new FiftyOneClientCapability(new Profile[] { profile })).Cast<IClientCapability>().ToList();

                            _allCapabilities = capabilities.AsQueryable();
                        }
                    }
                }
                return _allCapabilities;
            }
        }

        private static Dictionary<string, List<string>> ClientCapabilityValues
        {
            get
            {
                if (_allClientCapabilityValues == null)
                {
                    lock (_allClientCapabilityValuesLock)
                    {
                        if (_allClientCapabilityValues == null &&
                            WebProvider.ActiveProvider != null)
                        {
                            _allClientCapabilityValues = new Dictionary<string, List<string>>();

                            foreach (var property in WebProvider.ActiveProvider.DataSet.Properties)
                            {
                                var values = property.Values.Select(value => value.Name).ToList();
                                _allClientCapabilityValues.Add(property.Name, values);
                            }
                        }
                    }

                    _allClientCapabilityValues = _allClientCapabilityValues.OrderByDescending(kvp =>
                                                    {
                                                        if (HighPiorityCapabilityValues.ContainsKey(kvp.Key))
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
                if (_highPiorityCapabilityValues == null)
                {
                    //add some common capability as high piority capability values, it will appear at the top of capability values list.
                    _highPiorityCapabilityValues = new Dictionary<string, int>();

                    _highPiorityCapabilityValues.Add("IsMobile", 100);
                    _highPiorityCapabilityValues.Add("IsTablet", 95);
                    _highPiorityCapabilityValues.Add("PlatformName", 90);
                    _highPiorityCapabilityValues.Add("BrowserName", 85);
                    _highPiorityCapabilityValues.Add("BrowserVersion", 80);
                    _highPiorityCapabilityValues.Add("HasTouchScreen", 75);
                    _highPiorityCapabilityValues.Add("PlatformVersion", 70);
                    _highPiorityCapabilityValues.Add("ScreenPixelsWidth", 65);
                    _highPiorityCapabilityValues.Add("ScreenPixelsHeight", 60);
                    _highPiorityCapabilityValues.Add("HardwareVendor", 55);
                }

                return _highPiorityCapabilityValues;
            }
        }

        #endregion

        #region ClientCapabilityProvider Override Methods

        /// <summary>
        /// Returns ClientCapability based on the user agent provided. If 51Degrees
        /// is enabled then the results are returned from 51Degrees. If not then
        /// the standard capabilities provider is used with the user agent of the
        /// current request. 
        /// </summary>
        /// <remarks>
        /// The above behaviour is as implemented in prior versions with the 
        /// modification to check that 51Degrees is enabled. The default provider
        /// in .NET will not be able to use the provided userAgent which is a 
        /// problem where the current requests useragent and the provided useragent
        /// are different. This should be looked at in the future.
        /// TODO - determine default behaviour when 51Degrees disabled.
        /// </remarks>
        public override IClientCapability GetClientCapability(string userAgent)
        {
            if (_51DegreesEnabled &&
                WebProvider.ActiveProvider != null)
            {
                var match = WebProvider.ActiveProvider.Match(userAgent);
                if (match != null)
                {
                    return new FiftyOneClientCapability(match);
                }
            }
            return new FiftyOneClientCapability(HttpContext.Current.Request.Browser);
        }

        /// <summary>
        /// Returns ClientCapability based on device Id provided.
        /// </summary>
        public override IClientCapability GetClientCapabilityById(string deviceId)
        {
            Requires.NotNullOrEmpty("deviceId", deviceId);
            if (_51DegreesEnabled &&
                WebProvider.ActiveProvider != null)
            {
                var profiles = deviceId.Split(new[] { '-' }, System.StringSplitOptions.RemoveEmptyEntries).Select(i =>
                    WebProvider.ActiveProvider.DataSet.FindProfile(int.Parse(i))).Where(i => i != null).ToArray();
                if (profiles == null || profiles.Length == 0)
                {
                    throw new MobileException(string.Format("Can't get device capability for the id '{0}'", deviceId));
                }
                return new FiftyOneClientCapability(profiles);
            }
            return null;
        }

        /// <summary>
        /// Returns available Capability Values for every Capability Name
        /// </summary>
        /// <returns>
        /// Dictionary of Capability Name along with List of possible values of the Capability
        /// </returns>
        /// <example>Capability Name = mobile_browser, value = Safari, Andriod Webkit </example>
        public override IDictionary<string, List<string>> GetAllClientCapabilityValues()
        {
            return ClientCapabilityValues;
        }

        /// <summary>
        /// Returns All available Client Capabilities present
        /// </summary>
        /// <returns>
        /// List of IClientCapability present
        /// </returns>        
        public override IQueryable<IClientCapability> GetAllClientCapabilities()
        {
            return AllCapabilities;
        }

        #endregion

        #region Override Properties

        /// <summary>
        /// Indicates whether tablet detection is supported in the available data set.
        /// </summary>
        public override bool SupportsTabletDetection
        {
            get
            {
                return _51DegreesEnabled && WebProvider.ActiveProvider != null && WebProvider.ActiveProvider.DataSet.Properties["IsTablet"] != null;
            }
        }

        #endregion
    }
}

