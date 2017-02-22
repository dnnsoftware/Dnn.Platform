using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserModel : UserModelBase
    {
        public string DisplayName;
        public string FirstName;
        public string LastName;
        public DateTime LastActivity;
        public DateTime LastLockout;
        public DateTime LastPasswordChange;
        public DateTime Created;

        // provide a default field order for use of callers
        public static new string[] FieldOrder = 
        {
            "UserId",
            "Username",
            "DisplayName",
            "FirstName",
            "LastName",
            "Email",
            "LastActivity",
            "LastLogin",
            "LastLockout",
            "LastPasswordChange",
            "IsDeleted",
            "IsAuthorized",
            "IsLockedOut",
            "Created"
        };

        #region Constructors
        public UserModel()
        {
        }
        public UserModel(UserInfo user): base(user)
        {
            DisplayName = user.DisplayName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            LastActivity = user.Membership.LastActivityDate;
            LastLockout = user.Membership.LastLockoutDate;
            LastPasswordChange = user.Membership.LastPasswordChangeDate;
            Created = user.CreatedOnDate;
        }
        #endregion

        #region Command Links
        #endregion
    }
}