#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;

using FiftyOne.Foundation.Mobile.Detection;

using System.Web;

using FiftyOne.Foundation.Mobile;

using DotNetNuke.Services.ClientCapability;

using FiftyOne.Foundation.UI;

#endregion

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider
{
    /// <summary>
    /// 51Degrees.mobi implementation of ClientCapabilityProvider
    /// </summary>
    public class FiftyOneClientCapabilityProvider : DotNetNuke.Services.ClientCapability.ClientCapabilityProvider
    {
        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FiftyOneClientCapabilityProvider()
        {
        }

        #endregion

        #region Static Methods

        static object _allCapabilitiesLock = new object();
        static IQueryable<IClientCapability> _allCapabilities;

        static object _allClientCapabilityValuesLock = new object();
        static Dictionary<string, List<string>> _allClientCapabilityValues;

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
                            var capabilities = new List<IClientCapability>();

                            foreach (var device in Factory.ActiveProvider.Devices)
                            {
                                capabilities.Add(new FiftyOneClientCapability(device));
                            }

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
                        if (_allClientCapabilityValues == null)
                        {
                            _allClientCapabilityValues = new Dictionary<string, List<string>>();

                            foreach (var property in Factory.ActiveProvider.Properties.Values)
                            {
                                var values = new List<string>();
                                foreach (var value in property.Values)
                                    values.Add(value.Name);
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
                                                                  else
                                                                  {
                                                                      return 0;
                                                                  }
                                                              }).ThenBy(kvp => kvp.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                return _allClientCapabilityValues;
            }
        }

        private static IDictionary<string, int> _highPiorityCapabilityValues;
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
        /// Returns ClientCapability based on the user agent provided.
        /// </summary>
        public override IClientCapability GetClientCapability(string userAgent)
        {
            var request = HttpContext.Current != null ? HttpContext.Current.Request : null;
            if (request != null && request.UserAgent == userAgent &&
                request.Browser.Capabilities.Contains(FiftyOne.Foundation.Mobile.Detection.Constants.FiftyOneDegreesProperties))
            {
                // The useragent has already been processed by 51Degrees.mobi when the request
                // was processed by the detector module. Uses the values obtained then.
                var clientCapability = new FiftyOneClientCapability(request.Browser);
                clientCapability.UserAgent = request.UserAgent;
                return clientCapability;
            }
            else
            {
                // The useragent has not already been processed. Therefore process it now
                // and then set the properties.
                var deviceInfo = Factory.ActiveProvider.GetDeviceInfo(userAgent);
                if (deviceInfo != null)
                {
                    return new FiftyOneClientCapability(deviceInfo);
                }
                else
                {
                    return new FiftyOneClientCapability(null as SortedList<string, List<string>>);
                }
            }
        }

        /// <summary>
        /// Returns ClientCapability based on device Id provided.
        /// </summary>
        public override IClientCapability GetClientCapabilityById(string deviceId)
        {
            Requires.NotNullOrEmpty("deviceId", deviceId);

            var device = Factory.ActiveProvider.GetDeviceInfoByID(deviceId);
            
			if(device == null)
			{
                throw new MobileException(string.Format("Can't get device capability for the id '{0}'", deviceId));
			}

            return new FiftyOneClientCapability(device);
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
                return ClientCapabilityValues.ContainsKey("IsTablet");
            }
        }

        #endregion
    }
}
