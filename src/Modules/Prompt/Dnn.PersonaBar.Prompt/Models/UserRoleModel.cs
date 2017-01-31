using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserRoleModel
    {
        public int RoleId;
        public string RoleName;
        public bool IsPublic;
        public int PortalId;
        public int UserRoleId;
        public DateTime Start;

        public DateTime End;
        public static UserRoleModel FromDnnUserRoleInfo(DotNetNuke.Entities.Users.UserRoleInfo ri)
        {
            UserRoleModel urm = new UserRoleModel
            {
                RoleId = ri.RoleID,
                RoleName = ri.RoleName,
                IsPublic = ri.IsPublic,
                PortalId = ri.PortalID,
                Start = ri.EffectiveDate,
                End = ri.ExpiryDate
            };
            return urm;
        }
    }
}