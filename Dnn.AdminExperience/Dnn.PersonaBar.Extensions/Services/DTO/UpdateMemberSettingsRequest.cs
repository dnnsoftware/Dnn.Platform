// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Host;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class UpdateMemberSettingsRequest
    {
        public int MembershipResetLinkValidity { get; set; }

        public int AdminMembershipResetLinkValidity { get; set; }

        public bool EnablePasswordHistory { get; set; }

        public int MembershipNumberPasswords { get; set; }

        public int MembershipDaysBeforePasswordReuse { get; set; }

        public bool EnableBannedList { get; set; }

        public bool EnableStrengthMeter { get; set; }

        public bool EnableIPChecking { get; set; }

        public int PasswordExpiry { get; set; }

        public int PasswordExpiryReminder { get; set; }

        public bool ForceLogoutAfterPasswordChanged { get; set; }
    }
}
