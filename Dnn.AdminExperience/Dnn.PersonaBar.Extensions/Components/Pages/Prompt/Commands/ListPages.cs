// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("list-pages", Constants.PagesCategory, "Prompt_ListPages_Description")]
    public class ListPages : ConsoleCommandBase
    {
        [FlagParameter("parentid", "Prompt_ListPages_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        [FlagParameter("deleted", "Prompt_ListPages_FlagDeleted", "Boolean")]
        private const string FlagDeleted = "deleted";

        [FlagParameter("name", "Prompt_ListPages_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("title", "Prompt_ListPages_FlagTitle", "String")]
        private const string FlagTitle = "title";

        [FlagParameter("path", "Prompt_ListPages_FlagPath", "String")]
        private const string FlagPath = "path";

        [FlagParameter("skin", "Prompt_ListPages_FlagSkin", "String")]
        private const string FlagSkin = "skin";

        [FlagParameter("visible", "Prompt_ListRoles_FlagVisible", "Boolean")]
        private const string FlagVisible = "visible";

        [FlagParameter("page", "Prompt_ListRoles_FlagPage", "Integer", "1")]
        private const string FlagPage = "page";

        [FlagParameter("max", "Prompt_ListRoles_FlagMax", "Integer", "10")]
        private const string FlagMax = "max";

        public override string LocalResourceFile => Constants.LocalResourceFile;

        private int? ParentId { get; set; } = -1;
        private bool? Deleted { get; set; }
        private string PageName { get; set; }
        private string PageTitle { get; set; }
        private string PagePath { get; set; }
        private string PageSkin { get; set; }
        private bool? PageVisible { get; set; }
        private int Page { get; set; }
        private int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.ParentId = this.GetFlagValue<int?>(FlagParentId, "Parent Id", null, false, true, true);
            this.Deleted = this.GetFlagValue<bool?>(FlagDeleted, "Deleted", null);
            this.PageVisible = this.GetFlagValue<bool?>(FlagVisible, "Page Visible", null);
            this.PageName = this.GetFlagValue(FlagName, "Page Name", string.Empty);
            this.PageTitle = this.GetFlagValue(FlagTitle, "Page Title", string.Empty);
            this.PagePath = this.GetFlagValue(FlagPath, "Page Path", string.Empty);
            this.PageSkin = this.GetFlagValue(FlagSkin, "Page Skin", string.Empty);
            this.Page = this.GetFlagValue(FlagPage, "Page", 1);
            this.Max = this.GetFlagValue(FlagMax, "Max", 10);
        }

        public override ConsoleResultModel Run()
        {
            var max = this.Max <= 0 ? 10 : (this.Max > 500 ? 500 : this.Max);

            var lstOut = new List<PageModelBase>();

            int total;

            IEnumerable<DotNetNuke.Entities.Tabs.TabInfo> lstTabs;

            lstTabs = PagesController.Instance.GetPageList(this.PortalSettings, this.Deleted, this.PageName, this.PageTitle, this.PagePath, this.PageSkin, this.PageVisible, this.ParentId ?? -1, out total, string.Empty, this.Page > 0 ? this.Page - 1 : 0, max, this.ParentId == null);
            var totalPages = total / max + (total % max == 0 ? 0 : 1);
            var pageNo = this.Page > 0 ? this.Page : 1;
            lstOut.AddRange(lstTabs.Select(tab => new PageModelBase(tab)));
            return new ConsoleResultModel
            {
                Data = lstOut,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    PageSize = max
                },
                Records = lstOut.Count,
                Output = lstOut.Count == 0 ? this.LocalizeString("Prompt_NoPages") : "",
                FieldOrder = new[]
                {
                    "TabId", "ParentId", "Name", "Title", "Skin", "Path", "IncludeInMenu", "IsDeleted"
                }
            };
        }
    }
}
