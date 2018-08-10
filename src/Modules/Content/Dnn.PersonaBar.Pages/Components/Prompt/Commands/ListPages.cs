using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("list-pages", Constants.PagesCategory, "Prompt_ListPages_Description")]
    public class ListPages : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourceFile;

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

            ParentId = GetFlagValue<int?>(FlagParentId, "Parent Id", null, false, true, true);
            Deleted = GetFlagValue<bool?>(FlagDeleted, "Deleted", null);
            PageVisible = GetFlagValue<bool?>(FlagVisible, "Page Visible", null);
            PageName = GetFlagValue(FlagName, "Page Name", string.Empty);
            PageTitle = GetFlagValue(FlagTitle, "Page Title", string.Empty);
            PagePath = GetFlagValue(FlagPath, "Page Path", string.Empty);
            PageSkin = GetFlagValue(FlagSkin, "Page Skin", string.Empty);
            Page = GetFlagValue(FlagPage, "Page", 1);
            Max = GetFlagValue(FlagMax, "Max", 10);
        }

        public override ConsoleResultModel Run()
        {
            var max = Max <= 0 ? 10 : (Max > 500 ? 500 : Max);

            var lstOut = new List<PageModelBase>();

            int total;

            IEnumerable<DotNetNuke.Entities.Tabs.TabInfo> lstTabs;

            lstTabs = PagesController.Instance.GetPageList(PortalSettings, Deleted, PageName, PageTitle, PagePath, PageSkin, PageVisible, ParentId ?? -1, out total, string.Empty, Page > 0 ? Page - 1 : 0, max, ParentId == null);
            var totalPages = total / max + (total % max == 0 ? 0 : 1);
            var pageNo = Page > 0 ? Page : 1;
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
                Output = lstOut.Count == 0 ? LocalizeString("Prompt_NoPages") : "",
                FieldOrder = new[]
                {
                    "TabId", "ParentId", "Name", "Title", "Skin", "Path", "IncludeInMenu", "IsDeleted"
                }
            };
        }
    }
}
