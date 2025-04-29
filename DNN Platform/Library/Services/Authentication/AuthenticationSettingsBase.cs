// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

/// <summary>
/// The AuthenticationSettingsBase class provides a base class for Authentication
/// Settings controls.
/// </summary>
public abstract class AuthenticationSettingsBase : PortalModuleBase
{
    private string authenticationType = Null.NullString;

    /// <summary>Gets or sets the Type of Authentication associated with this control.</summary>
    public string AuthenticationType
    {
        get
        {
            return this.authenticationType;
        }

        set
        {
            this.authenticationType = value;
        }
    }

    /// <summary>UpdateSettings updates the settings in the Data Store.</summary>
    /// <remarks>This method must be overriden in the inherited class.</remarks>
    public abstract void UpdateSettings();
}
