// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Facebook.Components
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Authentication.OAuth;

    [DataContract]
    public class FacebookUserData : UserData
    {
        public override string FirstName
        {
            get { return this.FacebookFirstName; }
            set { }
        }

        public override string LastName
        {
            get { return this.FacebookLastName; }
            set { }
        }

        [DataMember(Name = "birthday")]
        public string Birthday { get; set; }

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "first_name")]
        public string FacebookFirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string FacebookLastName { get; set; }
    }
}
