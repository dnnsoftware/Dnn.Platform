using System;
using System.Runtime.Serialization;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class UserRoleDto
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "roleId")]
        public int RoleId { get; set; }

        [DataMember(Name = "roleName")]
        public string RoleName { get; set; }

        [DataMember(Name = "startTime")]
        public DateTime StartTime { get; set; }

        [DataMember(Name = "expiresTime")]
        public DateTime ExpiresTime { get; set; }

        [DataMember(Name = "allowExpired")]
        public bool AllowExpired { get; set; }

        [DataMember(Name = "allowDelete")]
        public bool AllowDelete { get; set; }

        [DataMember(Name = "allowOwner")]
        public bool AllowOwner { get; set; }

        public static UserRoleDto FromRoleInfo(PortalSettings portalSettings, UserRoleInfo userRole)
        {
            if (userRole == null)
            {
                return null;
            }

            return new UserRoleDto()
            {
                Id = userRole.UserRoleID,
                UserId = userRole.UserID,
                DisplayName = userRole.FullName,
                RoleId = userRole.RoleID,
                RoleName = userRole.RoleName,
                StartTime = userRole.EffectiveDate,
                ExpiresTime = userRole.ExpiryDate,
                AllowExpired = AllowExpiredRole(portalSettings, userRole.UserID, userRole.RoleID),
                AllowDelete = RoleController.CanRemoveUserFromRole(portalSettings, userRole.UserID, userRole.RoleID),
                AllowOwner = (userRole.SecurityMode == SecurityMode.SocialGroup) || (userRole.SecurityMode == SecurityMode.Both)
            };
        }

        public UserRoleInfo ToUserRoleInfo()
        {
            return new UserRoleInfo()
            {
                UserRoleID = Id,
                UserID = UserId,
                FullName = DisplayName,
                RoleID = RoleId,
                RoleName = RoleName,
                EffectiveDate = StartTime,
                ExpiryDate = ExpiresTime,
            };
        }

        internal static bool AllowExpiredRole(PortalSettings portalSettings, int userId, int roleId)
        {
            return userId != portalSettings.AdministratorId || roleId != portalSettings.AdministratorRoleId;
        }
    }
}