// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication.OAuth;

using System.Runtime.Serialization;

[DataContract]
public class EmailData
{
    [DataMember(Name = "preferred")]
    public string PreferredEmail { get; set; }

    [DataMember(Name = "account")]
    public string AccountEmail { get; set; }

    [DataMember(Name = "personal")]
    public string PersonalEmail { get; set; }

    [DataMember(Name = "business")]
    public string BusinessEmail { get; set; }
}
