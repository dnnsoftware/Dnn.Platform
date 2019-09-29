using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("list-roles", Constants.RolesCategory, "Prompt_ListRoles_Description")]
    public class ListRoles : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListRoles));

        [FlagParameter("page", "Prompt_ListRoles_FlagPage", "Integer", "1")]
        private const string FlagPage = "page";
        [FlagParameter("max", "Prompt_ListRoles_FlagMax", "Integer", "10")]
        private const string FlagMax = "max";

        public int Page { get; set; }
        public int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            Page = GetFlagValue(FlagPage, "Page", 1);
            Max = GetFlagValue(FlagMax, "Max", 10);
        }

        public override ConsoleResultModel Run()
        {
            var max = Max <= 0 ? 10 : (Max > 500 ? 500 : Max);

            var roles = new List<RoleModelBase>();
            try
            {
                int total;
                var results = RolesController.Instance.GetRoles(PortalSettings, -1, string.Empty, out total,
                    (Page > 0 ? Page - 1 : 0) * max, max);
                roles.AddRange(results.Select(role => new RoleModelBase(role)));
                var totalPages = total / max + (total % max == 0 ? 0 : 1);
                var pageNo = Page > 0 ? Page : 1;
                return new ConsoleResultModel
                {
                    Data = roles,
                    PagingInfo = new PagingInfo
                    {
                        PageNo = pageNo,
                        TotalPages = totalPages,
                        PageSize = max
                    },
                    Records = roles.Count,
                    Output = roles.Count == 0 ? LocalizeString("Prompt_NoRoles") : ""
                };

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("Prompt_ListRolesFailed"));
            }
        }
    }
}