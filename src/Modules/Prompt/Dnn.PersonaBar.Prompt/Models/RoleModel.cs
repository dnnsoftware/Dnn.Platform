using DotNetNuke.Security.Roles;
using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class RoleModel : RoleModelBase
    {
        public string Description;
        public string CreatedDate;
        public int CreatedBy;

        #region Constructors
        public RoleModel()
        {
        }
        public RoleModel(RoleInfo role): base(role)
        {
            ModifiedDate = role.LastModifiedOnDate.ToString("F");
            CreatedDate = role.CreatedOnDate.ToString("F");
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