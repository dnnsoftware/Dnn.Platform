using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class RoleInfoModel
    {
        public int RoleId;
        public int RoleGroupId;
        public string RoleName;
        public string Description;
        public bool IsPublic;
        public bool AutoAssign;
        public int UserCount;
        public string __UserCount;  // command link
        public DateTime CreatedDate;
        public DateTime ModifiedDate;
        public int CreatedBy;
        public string __CreatedBy;  // command link
        public int ModifiedBy;
        public string __ModifiedBy; // command link

        public static RoleInfoModel FromDnnRoleInfo(DotNetNuke.Security.Roles.RoleInfo ri)
        {
            RoleInfoModel rim = new RoleInfoModel
            {
                AutoAssign = ri.AutoAssignment,
                CreatedDate = ri.CreatedOnDate,
                CreatedBy = ri.CreatedByUserID,
                Description = ri.Description,
                ModifiedDate = ri.LastModifiedOnDate,
                ModifiedBy = ri.LastModifiedByUserID,
                IsPublic = ri.IsPublic,
                RoleGroupId = ri.RoleGroupID,
                RoleId = ri.RoleID,
                RoleName = ri.RoleName,
                UserCount = ri.UserCount
            };

            rim.__CreatedBy = string.Format("get-user {0}", rim.CreatedBy);
            rim.__ModifiedBy = string.Format("get-user {0}", rim.ModifiedBy);
            rim.__UserCount = string.Format("list-users --role '{0}'", rim.RoleName);

            return rim;
        }

    }
}