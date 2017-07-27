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
    [ConsoleCommand("list-roles", "Retrieves a list of DNN security roles for this portal", new[] { "page", "max" })]
    public class ListRoles : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListRoles));

        private const string FlagPage = "page";
        private const string FlagMax = "Max";

        public int Page { get; set; }
        public int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            Page = GetFlagValue(FlagPage, "Page", 1);
            Max = GetFlagValue(FlagMax, "Max", 10);
        }

        public override ConsoleResultModel Run()
        {
            var roles = new List<RoleModelBase>();
            try
            {
                int total;
                var results = RolesController.Instance.GetRoles(PortalSettings, -1, string.Empty, out total,
                    (Page > 0 ? Page - 1 : 0) * Max, Max);
                roles.AddRange(results.Select(role => new RoleModelBase(role)));
                var totalPages = total / Max + (total % Max == 0 ? 0 : 1);
                var pageNo = Page > 0 ? Page : 1;
                return new ConsoleResultModel
                {
                    Data = roles,
                    PagingInfo = new PagingInfo
                    {
                        PageNo = pageNo,
                        TotalPages = totalPages,
                        PageSize = Max
                    },
                    Records = roles.Count,
                    Output = pageNo <= totalPages ? LocalizeString("Prompt_ListRolesOutput") : LocalizeString("Prompt_NoRoles")
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