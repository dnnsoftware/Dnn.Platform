// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.LiveConnect.Components
{
    [DataContract]
    public class LiveUserData : UserData
    {
        #region Overrides

        public override string FirstName
        {
            get { return LiveFirstName; }
            set { }
        }

        public override string LastName
        {
            get { return LiveLastName; }
            set { }
        }

        #endregion

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "first_name")]
        public string LiveFirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LiveLastName { get; set; }
    }
}
