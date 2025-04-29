// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services.Dto;

/// <summary>Data-transfer object to save an IP filter.</summary>
public class UpdateIpFilterRequest
{
    /// <summary>Gets or sets the IP Address for the filter.</summary>
    public string IPAddress { get; set; }

    /// <summary>Gets or sets the subnet mask for an IP range.</summary>
    public string SubnetMask { get; set; }

    /// <summary>
    /// Gets or sets the type of rule.
    /// Allow = 1, Deny = 2.
    /// </summary>
    public int RuleType { get; set; }

    /// <summary>Gets or sets the ID of the filter.</summary>
    public int IPFilterID { get; set; }

    /// <summary>Gets or sets notes about this filter.</summary>
    public string Notes { get; set; }
}
