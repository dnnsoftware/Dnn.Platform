using DotNetNuke.Security.Roles;
using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class RoleModel : RoleModelBase
    {
        public string Description;
        public DateTime CreatedDate;
        public int CreatedBy;

        #region Constructors
        public RoleModel()
        {
        }
        public RoleModel(RoleInfo role)
        {
            CreatedDate = role.CreatedOnDate;
            CreatedBy = role.CreatedByUserID;
            Description = role.Description;
        }
        #endregion

        #region Command Links
        public string __CreatedBy
        {
            get
            {
                return $"get-user {CreatedBy}";
            }
        }
        #endregion
    }
}