using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Components
{
    public class Utilities
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Utilities));

        public static RoleInfo CreateRole(string roleName, int portalId, RoleStatus status, string description = "", bool isPublic = false, bool autoAssign = false, int roleGroupId = -1)
        {
            var ri = new RoleInfo
            {
                RoleName = roleName,
                Description = description,
                PortalID = portalId,
                IsPublic = isPublic,
                AutoAssignment = autoAssign,
                Status = status,
                RoleGroupID = roleGroupId,
                SecurityMode = SecurityMode.SecurityRole
            };

            var newRoleId = RoleController.Instance.AddRole(ri);

            return RoleController.Instance.GetRoleById(portalId, newRoleId);
        }

        public static RoleInfo UpdateRole(RoleInfo role)
        {
            RoleController.Instance.UpdateRole(role);
            return GetRoleById(role.RoleID, role.PortalID);
        }

        public static RoleInfo GetRoleById(int roleId, int portalId)
        {
            return RoleController.Instance.GetRoleById(portalId, roleId);
        }

        public static bool RoleExists(string roleName, int portalId)
        {
            var ri = RoleController.Instance.GetRoleByName(portalId, roleName);
            return ri != null;
        }

        public static IList<UserInfo> GetUsersInRole(string roleName, int portalId)
        {
            return RoleController.Instance.GetUsersByRole(portalId, roleName);
        }

        public static string FormatSkinName(string skin)
        {
            if (string.IsNullOrEmpty(skin))
                return skin;
            var match = Regex.Match(skin, "skins\\/(\\w*)\\/(.*)\\.ascx", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"{match.Groups[1].Value} ({match.Groups[2].Value})";
            }
            else
            {
                return skin;
                // unable to find a match
            }
        }

        public static string FormatContainerName(string container)
        {
            if (string.IsNullOrEmpty(container))
                return container;
            var r = new Regex("containers\\/(\\w*)\\/(.*)\\.ascx", RegexOptions.IgnoreCase);
            var match = r.Match(container);
            if (match.Success)
            {
                return $"{match.Groups[1].Value} ({match.Groups[2].Value})";
            }
            else
            {
                return container;
                // unable to find a match
            }
        }

        internal static string GetSuggestedCommand(string cmdName)
        {

            var match = Regex.Match(cmdName, "(\\w+)\\-(\\w+)");
            if (match.Success)
            {
                var verb = match.Groups[1].Value;
                var component = match.Groups[2].Value;
                switch (verb)
                {
                    case "CREATE":
                    case "ADD":
                        switch (component)
                        {
                            case "USER":
                                return "new-user";
                            case "PAGE":
                                return "new-page";
                            case "ROLE":
                                return "new-role";
                        }
                        break;
                    case "LOAD":
                    case "FIND":
                        switch (component)
                        {
                            case "USER":
                            case "USERS":
                                return TranslateToCommandOptionText("get-user", "list-users");
                            case "PAGE":
                            case "PAGES":
                                return TranslateToCommandOptionText("get-page", "list-pages");
                            case "ROLE":
                            case "ROLES":
                                return TranslateToCommandOptionText("get-role", "list-roles");
                        }
                        break;
                    case "UPDATE":
                    case "CHANGE":
                        switch (component)
                        {
                            case "USER":
                                return "set-user";
                            case "PAGE":
                                return "set-page";
                            case "PASSWORD":
                                return "reset-password";
                        }
                        break;
                    case "SET":
                        switch (component)
                        {
                            case "PASSWORD":
                                return "reset-password";
                        }
                        break;
                    case "GET":
                        switch (component)
                        {
                            case "ROLES":
                                return TranslateToCommandOptionText("get-role", "list-roles");
                            case "USERS":
                                return TranslateToCommandOptionText("get-user", "list-users");
                            case "PAGES":
                                return TranslateToCommandOptionText("get-page", "list-pages");
                            case "MODULES":
                                return TranslateToCommandOptionText("get-module", "list-modules");
                        }
                        break;
                    case "LIST":
                        switch (component)
                        {
                            case "USER":
                                return "list-users";
                            case "ROLE":
                                return "list-roles";
                            case "PAGE":
                                return "list-pages";
                            case "Module":
                                return "list-modules";
                        }
                        break;
                    case "RECOVER":
                        switch (component)
                        {
                            case "USER":
                                return "restore-user";
                        }
                        break;
                    case "REMOVE":
                        switch (component)
                        {
                            case "USER":
                                return TranslateToCommandOptionText("delete-user", "purge-user");
                        }
                        break;
                }
            }

            return string.Empty;
        }

        private static string TranslateToCommandOptionText(string string1, string string2)
        {
            return string.Format(Localization.GetString("CommandOptionText", Constants.LocalResourcesFile), string1, string2);
        }
    }
}


