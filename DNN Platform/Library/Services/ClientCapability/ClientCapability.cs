// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ClientCapability
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Services.ClientCapability;

    /// <summary>
    /// Default Implementation of IClientCapability.
    /// </summary>
    public class ClientCapability : IClientCapability
    {
        private IDictionary<string, string> _capabilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCapability"/> class.
        ///   Default Constructor.
        /// </summary>
        public ClientCapability()
        {
            this._capabilities = new Dictionary<string, string>();
        }

        /// <summary>
        ///   Gets or sets unique ID of the client making request.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        ///   Gets or sets user Agent of the client making request.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether is request coming from a mobile device.
        /// </summary>
        public bool IsMobile { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether is request coming from a tablet device.
        /// </summary>
        public bool IsTablet { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether does the requesting device supports touch screen.
        /// </summary>
        public bool IsTouchScreen { get; set; }

        /// <summary>
        ///   Gets or sets facebookRequest property is filled when request is coming though Facebook iFrame (e.g. fan pages).
        /// </summary>
        /// <remarks>
        ///   FacebookRequest property is populated based on data in "signed_request" headers coming from Facebook.
        ///   In order to ensure request is coming from Facebook, FacebookRequest.IsValidSignature method should be called with the secrety key provided by Facebook.
        /// </remarks>
        public FacebookRequest FacebookRequest { get; set; }

        /// <summary>
        ///   Gets or sets screenResolution Width of the requester in Pixels.
        /// </summary>
        public int ScreenResolutionWidthInPixels { get; set; }

        /// <summary>
        ///   Gets or sets screenResolution Height of the requester in Pixels.
        /// </summary>
        public int ScreenResolutionHeightInPixels { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether does requester support Flash.
        /// </summary>
        public bool SupportsFlash { get; set; }

        /// <summary>
        /// Gets or sets a key-value collection containing all capabilities supported by requester.
        /// </summary>
        [Obsolete("This method is not memory efficient and should be avoided as the Match class now exposes an accessor keyed on property name. Scheduled removal in v10.0.0.")]
        public IDictionary<string, string> Capabilities
        {
            get
            {
                return this._capabilities;
            }

            set
            {
                this._capabilities = value;
            }
        }

        /// <summary>
        /// Gets or sets represents the name of the broweser in the request.
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// Gets or sets the request prefered HTML DTD.
        /// </summary>
        public string HtmlPreferedDTD { get; set; }

        /// <summary>
        ///   Gets or sets http server variable used for SSL offloading - if this value is empty offloading is not enabled.
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
                throw new NotImplementedException(string.Empty);
            }
        }
    }
}
