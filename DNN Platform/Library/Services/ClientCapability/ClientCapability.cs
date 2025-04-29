// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientCapability;

using System;
using System.Collections.Generic;

/// <summary>Default Implementation of IClientCapability.</summary>
public class ClientCapability : IClientCapability
{
    private IDictionary<string, string> capabilities;

    /// <summary>Initializes a new instance of the <see cref="ClientCapability"/> class.</summary>
    public ClientCapability()
    {
        this.capabilities = new Dictionary<string, string>();
    }

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
    [Obsolete("Deprecated in DotNetNuke 8.0.0. This method is not memory efficient and should be avoided as the Match class now exposes an accessor keyed on property name. Scheduled for removal in v10.0.0.")]
    public IDictionary<string, string> Capabilities
    {
        get
        {
            return this.capabilities;
        }

        set
        {
            this.capabilities = value;
        }
    }

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
