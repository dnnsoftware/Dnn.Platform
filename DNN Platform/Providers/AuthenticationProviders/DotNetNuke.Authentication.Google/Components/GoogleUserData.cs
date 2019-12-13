// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Runtime.Serialization;

using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.Google.Components
{
    [DataContract]
    public class GoogleUserData : UserData
    {
        #region Overrides

        public override string FirstName
        {
            get { return GivenName; }
            set { }
        }

        public override string LastName
        {
            get { return FamilyName; }
            set { }
        }

        public override string ProfileImage
        {
            get { return Picture; }
            set { }
        }

        #endregion

        [DataMember(Name = "given_name")]
        public string GivenName { get; set; }

        [DataMember(Name = "family_name")]
        public string FamilyName { get; set; }

        [DataMember(Name = "picture")]
        public string Picture { get; set; }
    }
}
