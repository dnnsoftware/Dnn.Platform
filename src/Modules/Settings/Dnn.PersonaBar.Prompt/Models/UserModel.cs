using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserModel : UserModelBase
    {
        public string DisplayName;
        public string FirstName;
        public string LastName;
        public string LastActivity;
        public string LastLockout;
        public string LastPasswordChange;
        public string Created;

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
            LastLogin = user.Membership.LastLoginDate.ToPromptLongDateString();
            DisplayName = user.DisplayName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            LastActivity = user.Membership.LastActivityDate.ToPromptLongDateString();
            LastLockout = user.Membership.LastLockoutDate.ToPromptLongDateString();
            LastPasswordChange = user.Membership.LastPasswordChangeDate.ToPromptLongDateString();
            Created = user.CreatedOnDate.ToPromptLongDateString();
        }
        #endregion

        #region Command Links
        #endregion
    }
}