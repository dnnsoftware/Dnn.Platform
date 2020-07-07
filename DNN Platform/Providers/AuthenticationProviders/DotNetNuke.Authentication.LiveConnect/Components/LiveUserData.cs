// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.LiveConnect.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Authentication.OAuth;

    [DataContract]
    public class LiveUserData : UserData
    {
        public override string FirstName
        {
            get { return this.LiveFirstName; }
            set { }
        }

        public override string LastName
        {
            get { return this.LiveLastName; }
            set { }
        }

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "first_name")]
        public string LiveFirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LiveLastName { get; set; }
    }
}
