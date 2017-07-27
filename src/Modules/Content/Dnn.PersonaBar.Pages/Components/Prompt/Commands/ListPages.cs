using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("list-pages", "Retrieves a list of pages based on the specified criteria", new[]{
        "parentid",
        "deleted",
        "name",
        "title",
        "path",
        "skin",
        "page",
        "max"
    })]
    public class ListPages : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourceFile;

        private const string FlagParentId = "parentid";
        private const string FlagDeleted = "deleted";
        private const string FlagName = "name";
        private const string FlagTitle = "title";
        private const string FlagPath = "path";
        private const string FlagSkin = "skin";
        private const string FlagVisible = "visible";
        private const string FlagPage = "page";
        private const string FlagMax = "Max";

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
            base.Init(args, portalSettings, userInfo, activeTabId);
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
            var lstOut = new List<PageModelBase>();
            int total;
            var lstTabs = PagesController.Instance.GetPageList(Deleted, PageName, PageTitle, PagePath, PageSkin, PageVisible, ParentId ?? -1, out total, string.Empty, Page > 0 ? Page - 1 : 0, Max);
            var totalPages = total / Max + (total % Max == 0 ? 0 : 1);
            var pageNo = Page > 0 ? Page : 1;
            lstOut.AddRange(lstTabs.Select(tab => new PageModelBase(tab)));
            return new ConsoleResultModel
            {
                Data = lstOut,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    PageSize = Max
                },
                Records = lstOut.Count,
                Output = pageNo <= totalPages
                        ? LocalizeString("Prompt_ListPagesOutput")
                        : LocalizeString("Prompt_NoPages"),
                FieldOrder = new[]
                {
                    "TabId", "ParentId", "Name", "Title", "Skin", "Path", "IncludeInMenu", "IsDeleted"
                }
            };
        }
    }
}