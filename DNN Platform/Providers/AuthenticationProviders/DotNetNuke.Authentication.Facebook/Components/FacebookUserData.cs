// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Facebook.Components;

using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Authentication.OAuth;

/// <inheritdoc/>
[DataContract]
public class FacebookUserData : UserData
{
    /// <inheritdoc/>
    public override string FirstName
    {
        get { return this.FacebookFirstName; }
        set { }
    }

    /// <inheritdoc/>
    public override string LastName
    {
        get { return this.FacebookLastName; }
        set { }
    }

    /// <summary>Gets or sets the birthday.</summary>
    [DataMember(Name = "birthday")]
    public string Birthday { get; set; }

    /// <summary>Gets or sets the link URL.</summary>
    [DataMember(Name = "link")]
    public Uri Link { get; set; }

    /// <summary>Gets or sets the first name.</summary>
    [DataMember(Name = "first_name")]
    public string FacebookFirstName { get; set; }

    /// <summary>Gets or sets the last name.</summary>
    [DataMember(Name = "last_name")]
    public string FacebookLastName { get; set; }
}
