// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter.Components;

using System.Runtime.Serialization;

using DotNetNuke.Services.Authentication.OAuth;

/// <inheritdoc/>
[DataContract]
public class TwitterUserData : UserData
{
    /// <inheritdoc/>
    public override string DisplayName
    {
        get { return this.ScreenName; }
        set { }
    }

    /// <inheritdoc/>
    public override string Locale
    {
        get { return this.LanguageCode; }
        set { }
    }

    /// <inheritdoc/>
    public override string ProfileImage
    {
        get { return this.ProfileImageUrl; }
        set { }
    }

    /// <inheritdoc/>
    public override string Website
    {
        get { return this.Url; }
        set { }
    }

    /// <summary>Gets or sets the screen name.</summary>
    [DataMember(Name = "screen_name")]
    public string ScreenName { get; set; }

    /// <summary>Gets or sets the language code.</summary>
    [DataMember(Name = "lang")]
    public string LanguageCode { get; set; }

    /// <summary>Gets or sets the URL of the profile image.</summary>
    [DataMember(Name = "profile_image_url")]
    public string ProfileImageUrl { get; set; }

    /// <summary>Gets or sets the URL.</summary>
    [DataMember(Name = "url")]
    public string Url { get; set; }
}
