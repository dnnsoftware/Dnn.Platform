// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
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

    [ConsoleCommand("list-roles", Constants.RolesCategory, "Prompt_ListRoles_Description")]
    public class ListRoles : ConsoleCommandBase
    {
        [FlagParameter("page", "Prompt_ListRoles_FlagPage", "Integer", "1")]
        private const string FlagPage = "page";

        [FlagParameter("max", "Prompt_ListRoles_FlagMax", "Integer", "10")]
        private const string FlagMax = "max";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListRoles));

        public override string LocalResourceFile => Constants.LocalResourcesFile;
        public int Page { get; set; }
        public int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.Page = this.GetFlagValue(FlagPage, "Page", 1);
            this.Max = this.GetFlagValue(FlagMax, "Max", 10);
        }

        public override ConsoleResultModel Run()
        {
            var max = this.Max <= 0 ? 10 : (this.Max > 500 ? 500 : this.Max);

            var roles = new List<RoleModelBase>();
            try
            {
                int total;
                var results = RolesController.Instance.GetRoles(this.PortalSettings, -1, string.Empty, out total,
                    (this.Page > 0 ? this.Page - 1 : 0) * max, max);
                roles.AddRange(results.Select(role => new RoleModelBase(role)));
                var totalPages = total / max + (total % max == 0 ? 0 : 1);
                var pageNo = this.Page > 0 ? this.Page : 1;
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
                    Output = roles.Count == 0 ? this.LocalizeString("Prompt_NoRoles") : ""
                };

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_ListRolesFailed"));
            }
        }
    }
}
