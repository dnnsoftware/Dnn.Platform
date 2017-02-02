using DotNetNuke.Entities.Users;
using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserModelBase
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime LastLogin { get; set; }

        public bool IsDeleted { get; set; }

        #region Constructors
        public UserModelBase()
        {
        }
        public UserModelBase(UserInfo user)
        {
            Email = user.Email;
            LastLogin = user.Membership.LastLoginDate;
            UserId = user.UserID;
            Username = user.Username;
            IsDeleted = user.IsDeleted;
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