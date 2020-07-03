// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Roles;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserRoleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserRoleInfo class provides Business Layer model for a User/Role.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserRoleInfo : RoleInfo
    {
        public int UserRoleID { get; set; }

        public int UserID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsOwner { get; set; }

        public bool IsTrialUsed { get; set; }

        public bool Subscribed { get; set; }

        public override void Fill(IDataReader dr)
        {
            // Fill base class properties
            base.Fill(dr);

            // Fill this class properties
            this.UserRoleID = Null.SetNullInteger(dr["UserRoleID"]);
            this.UserID = Null.SetNullInteger(dr["UserID"]);
            this.FullName = Null.SetNullString(dr["DisplayName"]);
            this.Email = Null.SetNullString(dr["Email"]);
            this.EffectiveDate = Null.SetNullDateTime(dr["EffectiveDate"]);
            this.ExpiryDate = Null.SetNullDateTime(dr["ExpiryDate"]);
            this.IsOwner = Null.SetNullBoolean(dr["IsOwner"]);
            this.IsTrialUsed = Null.SetNullBoolean(dr["IsTrialUsed"]);
            if (this.UserRoleID > Null.NullInteger)
            {
                this.Subscribed = true;
            }
        }
    }
}
