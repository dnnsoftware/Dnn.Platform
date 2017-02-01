using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserInfoModelSlim
    {
        public int UserId;
        public string __UserId;     // command link
        public string Username;
        public string __Username;   // command link
        public string Email;
        public string __Email;      // command link
        public DateTime LastLogin;

        public bool IsDeleted;
        public static UserInfoModelSlim FromDnnUserInfo(DotNetNuke.Entities.Users.UserInfo user)
        {
            UserInfoModelSlim uim = new UserInfoModelSlim
            {
                Email = user.Email,
                LastLogin = user.Membership.LastLoginDate,
                UserId = user.UserID,
                Username = user.Username,
                IsDeleted = user.IsDeleted
            };

            uim.__Email = string.Format("get-user '{0}'", uim.Email);
            uim.__UserId = string.Format("get-user {0}", uim.UserId);
            uim.__Username = string.Format("get-user '{0}'", uim.Username);

            return uim;
        }
    }
}