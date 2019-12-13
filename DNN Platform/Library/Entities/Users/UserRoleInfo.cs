using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Roles;


namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserRoleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserRoleInfo class provides Business Layer model for a User/Role
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserRoleInfo : RoleInfo
    {
        #region Public Properties

        public int UserRoleID { get; set; }

        public int UserID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsOwner { get; set; }
        
        public bool IsTrialUsed { get; set; }

        public bool Subscribed { get; set; }

        #endregion

        public override void Fill(IDataReader dr)
        {
			//Fill base class properties
            base.Fill(dr);

			//Fill this class properties
            UserRoleID = Null.SetNullInteger(dr["UserRoleID"]);
            UserID = Null.SetNullInteger(dr["UserID"]);
            FullName = Null.SetNullString(dr["DisplayName"]);
            Email = Null.SetNullString(dr["Email"]);
            EffectiveDate = Null.SetNullDateTime(dr["EffectiveDate"]);
            ExpiryDate = Null.SetNullDateTime(dr["ExpiryDate"]);
            IsOwner = Null.SetNullBoolean(dr["IsOwner"]);
            IsTrialUsed = Null.SetNullBoolean(dr["IsTrialUsed"]);
            if (UserRoleID > Null.NullInteger)
            {
                Subscribed = true;
            }
        }
    }
}
