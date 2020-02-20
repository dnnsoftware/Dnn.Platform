// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.Facebook.Components
{
    [DataContract]
    public class FacebookUserData : UserData
    {
        #region Overrides

        public override string FirstName
        {
            get { return FacebookFirstName; }
            set { }
        }

        public override string LastName
        {
            get { return FacebookLastName; }
            set { }
        }

        #endregion

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
