

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.ComponentModel;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationConfigBase class provides base configuration class for the
    /// Authentication providers
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public abstract class AuthenticationConfigBase
    {
        /// <summary>
        /// Gets or sets the Dependency Provider to resolve registered
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected IServiceProvider DependencyProvider { get; }

        public AuthenticationConfigBase()
        {
            this.DependencyProvider = Globals.DependencyProvider;
        }

        protected AuthenticationConfigBase(int portalID)
            : this()
        {
            this.PortalID = portalID;
        }

        [Browsable(false)]
        public int PortalID { get; set; }
    }
}
