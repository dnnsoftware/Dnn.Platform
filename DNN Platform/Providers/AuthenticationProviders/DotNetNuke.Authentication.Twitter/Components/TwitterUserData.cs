// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter.Components
{
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Authentication.OAuth;

    [DataContract]
    public class TwitterUserData : UserData
    {
        public override string DisplayName
        {
            get { return this.ScreenName; }
            set { }
        }

        public override string Locale
        {
            get { return this.LanguageCode; }
            set { }
        }

        public override string ProfileImage
        {
            get { return this.ProfileImageUrl; }
            set { }
        }

        public override string Website
        {
            get { return this.Url; }
            set { }
        }

        [DataMember(Name = "screen_name")]
        public string ScreenName { get; set; }

        [DataMember(Name = "lang")]
        public string LanguageCode { get; set; }

        [DataMember(Name = "profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
