using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class RoleInfoModelSlim
    {
        public int RoleId;
        
        public string __RoleId;     // command link
        public int RoleGroupId;
        public string RoleName;
        public bool IsPublic;
        public bool AutoAssign;
        public int UserCount;
        public string __UserCount;  // command link
        public System.DateTime ModifiedDate;
        public int ModifiedBy;
        public string __ModifiedBy; // command link

        public static RoleInfoModelSlim FromDnnRoleInfo(DotNetNuke.Security.Roles.RoleInfo ri)
        {
            RoleInfoModelSlim rim = new RoleInfoModelSlim
            {
                AutoAssign = ri.AutoAssignment,
                ModifiedDate = ri.LastModifiedOnDate,
                ModifiedBy = ri.LastModifiedByUserID,
                IsPublic = ri.IsPublic,
                RoleGroupId = ri.RoleGroupID,
                RoleId = ri.RoleID,
                RoleName = ri.RoleName,
                UserCount = ri.UserCount
            };

            rim.__ModifiedBy = string.Format("get-user {0}", rim.ModifiedBy);
            rim.__RoleId = string.Format("get-role {0}", rim.RoleId);
            rim.__UserCount = string.Format("list-users --role '{0}'", rim.RoleName);

            return rim; 
        }

    }
}