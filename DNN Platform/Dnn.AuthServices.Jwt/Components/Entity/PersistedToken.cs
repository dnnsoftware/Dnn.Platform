// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Entity;

using System;

/// <summary>Represents a persisted token.</summary>
[Serializable]
public class PersistedToken
{
    /// <summary>Gets or sets the ID for the token.</summary>
    public string TokenId { get; set; }

    /// <summary>Gets or sets the id of the user.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the renewal count.</summary>
    public int RenewCount { get; set; }

    /// <summary>Gets or sets a value indicating when the token expires.</summary>
    public DateTime TokenExpiry { get; set; }

    /// <summary>Gets or sets when the renewal token expires.</summary>
    public DateTime RenewalExpiry { get; set; }

    /// <summary>Gets or sets the token hash value.</summary>
    public string TokenHash { get; set; }

    /// <summary>Gets or sets the renewal token hash value.</summary>
    public string RenewalHash { get; set; }
}
