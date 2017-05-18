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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotNetNuke.Services.ClientCapability;


namespace DotNetNuke.Services.ClientCapability
{
    /// <summary>
    /// Default Implementation of IClientCapability
    /// </summary>
    public class ClientCapability : IClientCapability
    {
        private IDictionary<string, string> _capabilities;
        /// <summary>
        ///   Default Constructor.
        /// </summary>
        public ClientCapability()
        {
            _capabilities = new Dictionary<string, string>();
        }
    
        #region Implementation of IClientCapability

        /// <summary>
        ///   Unique ID of the client making request.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        ///   User Agent of the client making request
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        ///   Is request coming from a mobile device.
        /// </summary>
        public bool IsMobile { get; set; }

        /// <summary>
        ///   Is request coming from a tablet device.
        /// </summary>
        public bool IsTablet { get; set; }        

        /// <summary>
        ///   Does the requesting device supports touch screen.
        /// </summary>
        public bool IsTouchScreen { get; set; }

        /// <summary>
        ///   FacebookRequest property is filled when request is coming though Facebook iFrame (e.g. fan pages).
        /// </summary>
        /// <remarks>
        ///   FacebookRequest property is populated based on data in "signed_request" headers coming from Facebook.  
        ///   In order to ensure request is coming from Facebook, FacebookRequest.IsValidSignature method should be called with the secrety key provided by Facebook.
        /// </remarks>         
        public FacebookRequest FacebookRequest { get; set; }

        /// <summary>
        ///   ScreenResolution Width of the requester in Pixels.
        /// </summary>
        public int ScreenResolutionWidthInPixels { get; set; }

        /// <summary>
        ///   ScreenResolution Height of the requester in Pixels.
        /// </summary>
        public int ScreenResolutionHeightInPixels { get; set; }

        /// <summary>
        ///   Does requester support Flash.
        /// </summary>
        public bool SupportsFlash { get; set; }

        /// <summary>
        /// A key-value collection containing all capabilities supported by requester
        /// </summary>
        [Obsolete(
            "This method is not memory efficient and should be avoided as the Match class now exposes an accessor keyed on property name."
            )]
        public IDictionary<string, string> Capabilities
        {
            get
            {
                return _capabilities;
                
            }
            set
            {
                _capabilities = value;
                
            }
        }

        /// <summary>
        /// Represents the name of the broweser in the request
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// Returns the request prefered HTML DTD
        /// </summary>
        public string HtmlPreferedDTD { get; set; }

        /// <summary>
        ///   Http server variable used for SSL offloading - if this value is empty offloading is not enabled
        /// </summary>
        public string SSLOffload { get; set; }

        /// <summary>
        /// Get client capability value by property name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual string this[string name] 
        {
            get
            {
                throw new NotImplementedException("");
            }
        }

        #endregion
    }
}
