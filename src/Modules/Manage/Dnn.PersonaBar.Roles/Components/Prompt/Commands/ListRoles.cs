using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("list-roles", "Retrieves a list of DNN security roles for this portal", new[] { "page", "max" })]
    public class ListRoles : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListRoles));

        private const string FlagPage = "page";
        private const string FlagMax = "Max";

        public int Page { get; set; }
        public int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagPage))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPage), out tmpId))
                    Page = tmpId;
            }
            if (HasFlag(FlagMax))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagMax), out tmpId))
                    Max = tmpId > 0 && tmpId < 500 ? tmpId : Max;
            }
            ValidationMessage = sbErrors.ToString();
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
                    Output = pageNo <= totalPages ? Localization.GetString("Prompt_ListRolesOutput", Constants.LocalResourcesFile) : Localization.GetString("Prompt_NoRoles", Constants.LocalResourcesFile)
                };

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_ListRolesFailed", Constants.LocalResourcesFile));
            }
        }
    }
}