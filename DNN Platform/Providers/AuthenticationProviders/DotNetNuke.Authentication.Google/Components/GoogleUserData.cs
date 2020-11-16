// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Google.Components
{
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Authentication.OAuth;

    [DataContract]
    public class GoogleUserData : UserData
    {
        public override string FirstName
        {
            get { return this.GivenName; }
            set { }
        }

        public override string LastName
        {
            get { return this.FamilyName; }
            set { }
        }

        public override string ProfileImage
        {
            get { return this.Picture; }
            set { }
        }

        [DataMember(Name = "given_name")]
        public string GivenName { get; set; }

        [DataMember(Name = "family_name")]
        public string FamilyName { get; set; }

        [DataMember(Name = "picture")]
        public string Picture { get; set; }
    }
}
