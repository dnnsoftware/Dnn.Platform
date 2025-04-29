// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Google.Components;

using System.Runtime.Serialization;

using DotNetNuke.Services.Authentication.OAuth;

/// <inheritdoc/>
[DataContract]
public class GoogleUserData : UserData
{
    /// <inheritdoc/>
    public override string FirstName
    {
        get { return this.GivenName; }
        set { }
    }

    /// <inheritdoc/>
    public override string LastName
    {
        get { return this.FamilyName; }
        set { }
    }

    /// <inheritdoc/>
    public override string ProfileImage
    {
        get { return this.Picture; }
        set { }
    }

    /// <summary>Gets or sets the given name.</summary>
    [DataMember(Name = "given_name")]
    public string GivenName { get; set; }

    /// <summary>Gets or sets the family name.</summary>
    [DataMember(Name = "family_name")]
    public string FamilyName { get; set; }

    /// <summary>Gets or sets the picture URL.</summary>
    [DataMember(Name = "picture")]
    public string Picture { get; set; }
}
