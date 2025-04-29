// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientCapability;

internal struct FaceBookData
{
    public User User { get; set; }

    public string Algorithm { get; set; }

    public long Issued_at { get; set; }

    public string User_id { get; set; }

    public string Oauth_token { get; set; }

    public long Expires { get; set; }

    public string App_data { get; set; }

    public Page Page { get; set; }

    public long Profile_id { get; set; }
}
