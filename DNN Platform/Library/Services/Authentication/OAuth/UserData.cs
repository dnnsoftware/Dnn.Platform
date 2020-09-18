// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class UserData
    {
        public string PreferredEmail
        {
            get
            {
                if (this.Emails == null)
                {
                    return this.Email;
                }

                return this.Emails.PreferredEmail;
            }
        }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        public virtual string DisplayName
        {
            get
            {
                return this.Name;
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
                return (!string.IsNullOrEmpty(this.Name) && this.Name.IndexOf(" ", StringComparison.Ordinal) > 0) ? this.Name.Substring(0, this.Name.IndexOf(" ", StringComparison.Ordinal)) : string.Empty;
            }

            set { this.Name = value + " " + this.LastName; }
        }

        [DataMember(Name = "gender")]
        public string Gender { get; set; }

        public virtual string LastName
        {
            get
            {
                return (!string.IsNullOrEmpty(this.Name) && this.Name.IndexOf(" ", StringComparison.Ordinal) > 0) ? this.Name.Substring(this.Name.IndexOf(" ", StringComparison.Ordinal) + 1) : this.Name;
            }

            set { this.Name = this.FirstName + " " + value; }
        }

        [DataMember(Name = "locale")]
        public virtual string Locale { get; set; }

        [DataMember(Name = "name")]
        public virtual string Name { get; set; }

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
