using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    public class UserModelBase
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string LastLogin { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsLockedOut { get; set; }
        // provide a default field order for use of callers
        public static string[] FieldOrder = {
            "UserId",
            "Username",
            "Email",
            "LastLogin",
            "IsDeleted",
            "IsAuthorized",
            "IsLockedOut"
        };

        #region Constructors
        public UserModelBase()
        {
        }
        public UserModelBase(UserInfo userInfo)
        {
            Email = userInfo.Email;
            LastLogin = userInfo.Membership.LastLoginDate.ToPromptShortDateString();
            UserId = userInfo.UserID;
            Username = userInfo.Username;
            IsDeleted = userInfo.IsDeleted;
            IsAuthorized = userInfo.Membership.Approved;
            IsLockedOut = userInfo.Membership.LockedOut;
        }
        #endregion

        #region Command Links
        public string __Email => $"get-user '{Email}'";
        public string __UserId => $"get-user {UserId}";
        public string __Username => $"get-user '{Username}'";

        #endregion

    }
}