// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Install
{
    /// <summary>Holds information to build the Security tab during upgrade.</summary>
    public class SecurityTabResult
    {
        /// <summary>Gets or sets a value indicating whether the Upgrade Now button will be enabled or not by default.</summary>
        public bool CanProceed { get; set; }

        /// <summary>Gets or sets the HTML markup to render in the Security tab.</summary>
        public string View { get; set; }
    }
}
