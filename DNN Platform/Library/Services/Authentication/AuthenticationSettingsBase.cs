// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
