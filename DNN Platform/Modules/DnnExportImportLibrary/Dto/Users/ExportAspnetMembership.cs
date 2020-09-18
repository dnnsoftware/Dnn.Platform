// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Users
{
    using System;

    public class ExportAspnetMembership : BasicExportImportDto
    {
        public Guid ApplicationId { get; set; }

        public Guid UserId { get; set; }

        public string Password { get; set; }

        public int PasswordFormat { get; set; }

        public string PasswordSalt { get; set; }

        public virtual string MobilePin { get; set; }

        public string Email { get; set; }

        public string LoweredEmail { get; set; }

        public string PasswordQuestion { get; set; }

        public string PasswordAnswer { get; set; }

        public bool IsApproved { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        public int FailedPasswordAnswerAttemptCount { get; set; }

        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        public string Comment { get; set; }
    }
}
