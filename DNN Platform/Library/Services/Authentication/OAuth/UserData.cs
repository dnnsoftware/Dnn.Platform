#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

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