using Dnn.PersonaBar.Library.Prompt.Common;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class UserRoleModel
    {
        public int RoleId;
        public string RoleName;
        public bool IsPublic;
        public int PortalId;
        public int UserRoleId;
        public string Start;

        public string End;
        public static UserRoleModel FromDnnUserRoleInfo(DotNetNuke.Entities.Users.UserRoleInfo ri)
        {
            var urm = new UserRoleModel
            {
                RoleId = ri.RoleID,
                RoleName = ri.RoleName,
                IsPublic = ri.IsPublic,
                PortalId = ri.PortalID,
                Start = ri.EffectiveDate.ToPromptShortDateString(),
                End = ri.ExpiryDate.ToPromptShortDateString()
            };
            return urm;
        }
    }
}