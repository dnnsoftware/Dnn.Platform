using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserInfoModel
    {
        public int UserId;
        public string __UserId; // command link
        public string Username;
        public string Email;
        public string DisplayName;
        public string FirstName;
        public string LastName;
        public DateTime LastLogin;
        public DateTime Created;

        public bool IsDeleted;
        public static UserInfoModel FromDnnUserInfo(DotNetNuke.Entities.Users.UserInfo user)
        {
            UserInfoModel uim = new UserInfoModel
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LastLogin = user.Membership.LastLoginDate,
                UserId = user.UserID,
                Username = user.Username,
                Created = user.CreatedOnDate,
                IsDeleted = user.IsDeleted
            };

            uim.__UserId = string.Format("get-user {0}", uim.UserId);

            return uim;
        }
    }
}