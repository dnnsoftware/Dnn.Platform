using Dnn.PersonaBar.Library.Prompt.Common;

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    public class UserRoleModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsPublic { get; set; }
        public int PortalId { get; set; }
        public int UserRoleId { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public static UserRoleModel FromDnnUserRoleInfo(DotNetNuke.Entities.Users.UserRoleInfo userRoleInfo)
        {
            var userRoleModel = new UserRoleModel
            {
                RoleId = userRoleInfo.RoleID,
                RoleName = userRoleInfo.RoleName,
                IsPublic = userRoleInfo.IsPublic,
                PortalId = userRoleInfo.PortalID,
                Start = userRoleInfo.EffectiveDate.ToPromptShortDateString(),
                End = userRoleInfo.ExpiryDate.ToPromptShortDateString()
            };
            return userRoleModel;
        }
    }
}