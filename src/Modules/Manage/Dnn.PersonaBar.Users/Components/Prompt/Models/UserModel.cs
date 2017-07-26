using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    public class UserModel : UserModelBase
    {
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastActivity { get; set; }
        public string LastLockout { get; set; }
        public string LastPasswordChange { get; set; }
        public string Created { get; set; }

        // provide a default field order for use of callers
        public new static string[] FieldOrder =
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
        public UserModel(UserInfo user) : base(user)
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