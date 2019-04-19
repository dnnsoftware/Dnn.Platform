using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Models
{
    public class RoleModelBase
    {
        public int RoleId { get; set; }
        public int RoleGroupId { get; set; }
        public string RoleName { get; set; }
        public bool IsPublic { get; set; }
        public bool AutoAssign { get; set; }
        public int UserCount { get; set; }
        public string ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }

        #region Constructors
        public RoleModelBase()
        {
        }

        public RoleModelBase(RoleInfo role)
        {
            AutoAssign = role.AutoAssignment;
            ModifiedDate = role.LastModifiedOnDate.ToPromptShortDateString();
            ModifiedBy = role.LastModifiedByUserID;
            IsPublic = role.IsPublic;
            RoleGroupId = role.RoleGroupID;
            RoleId = role.RoleID;
            RoleName = role.RoleName;
            UserCount = role.UserCount;
        }
        #endregion

        #region Command Links
        public string __ModifiedBy => $"get-user {ModifiedBy}";

        public string __RoleId => $"get-role {RoleId}";
        public string __UserCount => $"list-users --role '{RoleName}'";

        #endregion
    }
}