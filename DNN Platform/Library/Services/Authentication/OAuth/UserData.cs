// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace DotNetNuke.Services.Authentication.OAuth
{
    [DataContract]
    public class UserData
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        public virtual string DisplayName
        {
            get
            {
                return Name;
            }
            set { }
        }

        [DataMember(Name = "email")]
        public virtual string Email { get; set; }

        [DataMember(Name = "emails")]
        public EmailData Emails { get; set; }

        public virtual string FirstName
        {
            get
            {
                return (!String.IsNullOrEmpty(Name) && Name.IndexOf(" ", StringComparison.Ordinal) > 0) ? Name.Substring(0, Name.IndexOf(" ", StringComparison.Ordinal)) : String.Empty;
            }
            set { Name = value + " " + LastName; }
        }

        [DataMember(Name = "gender")]
        public string Gender { get; set; }

        public virtual string LastName
        {
            get
            {
                return (!String.IsNullOrEmpty(Name) && Name.IndexOf(" ", StringComparison.Ordinal) > 0) ? Name.Substring(Name.IndexOf(" ", StringComparison.Ordinal) + 1) : Name;
            }
            set { Name = FirstName + " " + value; }

        }

        [DataMember(Name = "locale")]
        public virtual string Locale { get; set; }

        [DataMember(Name = "name")]
        public virtual string Name { get; set; }

        public string PreferredEmail 
        { 
            get
            {
                if (Emails == null)
                {
                    return Email;
                }
                return Emails.PreferredEmail;
            }
        }

        public virtual string ProfileImage { get; set; }

        [DataMember(Name = "timezone")]
        public string Timezone { get; set; }

        [DataMember(Name = "time_zone")]
        public string TimeZoneInfo { get; set; }

        [DataMember(Name = "username")]
        public virtual string UserName { get; set; }

        [DataMember(Name = "website")]
        public virtual string Website { get; set; }
    }

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
}
