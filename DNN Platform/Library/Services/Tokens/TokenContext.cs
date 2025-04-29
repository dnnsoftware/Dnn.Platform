// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens;

using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

/// <summary>The context in which tokenization should happen.</summary>
public class TokenContext
{
    /// <summary>Gets or sets the user object representing the currently accessing user (permission).</summary>
    /// <value>UserInfo object.</value>
    public UserInfo AccessingUser { get; set; }

    /// <summary>Gets or sets the user object to use for 'User:' token replacement.</summary>
    /// <value>UserInfo object.</value>
    public UserInfo User { get; set; }

    /// <summary>Gets or sets the portal settings object to use for 'Portal:' token replacement.</summary>
    /// <value>PortalSettings object.</value>
    public PortalSettings Portal { get; set; }

    /// <summary>Gets or sets the tab settings object to use for 'Tab:' token replacement.</summary>
    public TabInfo Tab { get; set; }

    /// <summary>Gets or sets the module settings object to use for 'Module:' token replacement.</summary>
    public ModuleInfo Module { get; set; }

    /// <summary>Gets or sets the Format provider as Culture info from stored language or current culture.</summary>
    /// <value>An CultureInfo.</value>
    public CultureInfo Language { get; set; } = Thread.CurrentThread.CurrentUICulture;

    /// <summary>Gets or sets the current Access Level controlling access to critical user settings.</summary>
    /// <value>A TokenAccessLevel as defined above.</value>
    public Scope CurrentAccessLevel { get; set; }

    /// <summary>Gets or sets a value indicating whether if DebugMessages are enabled, unknown Tokens are replaced with Error Messages.</summary>
    public bool DebugMessages { get; set; }

    public Dictionary<string, IPropertyAccess> PropertySource { get; private set; } = new Dictionary<string, IPropertyAccess>();
}
