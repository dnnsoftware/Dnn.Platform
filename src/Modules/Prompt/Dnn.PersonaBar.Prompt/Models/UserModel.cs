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
        public DateTime Created;

        #region Constructors
        public UserModel()
        {
        }
        public UserModel(UserInfo user): base(user)
        {
            DisplayName = user.DisplayName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Created = user.CreatedOnDate;
        }
        #endregion

        #region Command Links
        #endregion
    }
}