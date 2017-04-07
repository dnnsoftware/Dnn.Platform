#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using System;
using Dnn.ExportImport.Dto.Users;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [Serializable]
    [TableName("Membership")]
    [PrimaryKey("UserId", AutoIncrement = false)]
    public class ImportAspnetMembership : ExportAspnetMembership
    {
        [JsonProperty(PropertyName = "MobilePIN")]
        [ColumnName("MobilePIN")]
        public override string MobilePin { get; set; }

        [IgnoreColumn]
        public override int Id { get; set; }

        [IgnoreColumn]
        public override int? ReferenceId { get; set; }

        [IgnoreColumn]
        public override int? LocalId { get; set; }

        public ImportAspnetMembership(ExportAspnetMembership aspnetMembership)
        {
            if (aspnetMembership == null) return;

            ApplicationId = aspnetMembership.ApplicationId;
            UserId = aspnetMembership.UserId;
            Password = aspnetMembership.Password;
            PasswordFormat = aspnetMembership.PasswordFormat;
            PasswordSalt = aspnetMembership.PasswordSalt;
            Email = aspnetMembership.Email;
            LoweredEmail = aspnetMembership.LoweredEmail;
            PasswordQuestion = aspnetMembership.PasswordQuestion;
            PasswordAnswer = aspnetMembership.PasswordAnswer;
            IsApproved = aspnetMembership.IsApproved;
            IsLockedOut = aspnetMembership.IsLockedOut;
            CreateDate = aspnetMembership.CreateDate;
            LastLoginDate = aspnetMembership.LastLoginDate;
            LastPasswordChangedDate = aspnetMembership.LastPasswordChangedDate;
            LastLockoutDate = aspnetMembership.LastLockoutDate;
            FailedPasswordAttemptCount = aspnetMembership.FailedPasswordAttemptCount;
            FailedPasswordAttemptWindowStart = aspnetMembership.FailedPasswordAttemptWindowStart;
            FailedPasswordAnswerAttemptCount = aspnetMembership.FailedPasswordAttemptCount;
            FailedPasswordAnswerAttemptWindowStart = aspnetMembership.FailedPasswordAttemptWindowStart;
            Comment = aspnetMembership.Comment;
        }
    }
}
