using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Models
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
        public static string[] FieldOrder = new string[]
        {
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
        public UserModelBase(UserInfo user)
        {
            Email = user.Email;
            LastLogin = user.Membership.LastLoginDate.ToPromptShortDateString();
            UserId = user.UserID;
            Username = user.Username;
            IsDeleted = user.IsDeleted;
            IsAuthorized = user.Membership.Approved;
            IsLockedOut = user.Membership.LockedOut;
        }
        #endregion

        #region Command Links
        public string __Email
        {
            get
            {
                return $"get-user '{Email}'";
            }
        }
        public string __UserId
        {
            get
            {
                return $"get-user {UserId}";
            }
        }
        public string __Username
        {
            get
            {
                return $"get-user '{Username}'";
            }
        }
        #endregion

    }
}