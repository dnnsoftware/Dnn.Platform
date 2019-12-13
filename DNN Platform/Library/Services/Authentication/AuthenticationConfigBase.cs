// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.ComponentModel;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

#endregion

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
            DependencyProvider = Globals.DependencyProvider;
        }

        protected AuthenticationConfigBase(int portalID)
            : this()
        {
            PortalID = portalID;
        }

        [Browsable(false)]
        public int PortalID { get; set; }
    }
}
