﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationSettingsBase class provides a base class for Authentiication 
    /// Settings controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class AuthenticationSettingsBase : PortalModuleBase
    {
        private string _AuthenticationType = Null.NullString;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Type of Authentication associated with this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType
        {
            get
            {
                return _AuthenticationType;
            }
            set
            {
                _AuthenticationType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings updates the settings in the Data Store
        /// </summary>
        /// <remarks>This method must be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------
        public abstract void UpdateSettings();
    }
}
