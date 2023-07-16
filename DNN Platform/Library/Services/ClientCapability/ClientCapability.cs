// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientCapability
{
    using System;

    /// <summary>Default Implementation of IClientCapability.</summary>
    public class ClientCapability : IClientCapability
    {
        /// <inheritdoc />
        public string ID { get; set; }

        /// <inheritdoc />
        public string UserAgent { get; set; }

        /// <inheritdoc />
        public bool IsMobile { get; set; }

        /// <inheritdoc />
        public bool IsTablet { get; set; }

        /// <inheritdoc />
        public bool IsTouchScreen { get; set; }

        /// <inheritdoc />
        public FacebookRequest FacebookRequest { get; set; }

        /// <inheritdoc />
        public int ScreenResolutionWidthInPixels { get; set; }

        /// <inheritdoc />
        public int ScreenResolutionHeightInPixels { get; set; }

        /// <inheritdoc />
        public bool SupportsFlash { get; set; }

        /// <inheritdoc />
        public string BrowserName { get; set; }

        /// <inheritdoc />
        public string HtmlPreferedDTD { get; set; }

        /// <inheritdoc />
        public string SSLOffload { get; set; }

        /// <inheritdoc />
        public virtual string this[string name]
        {
            get
            {
                throw new NotImplementedException(string.Empty);
            }
        }
    }
}
