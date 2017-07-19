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
        private const string FlagParentid = "parentid";
        private const string FlagDeleted = "deleted";
        private const string FlagName = "name";
        private const string FlagTitle = "title";
        private const string FlagPath = "path";
        private const string FlagSkin = "skin";
        private const string FlagVisible = "visible";
        private const string FlagPage = "page";
        private const string FlagMax = "Max";


        int ParentId { get; set; } = -1;
        bool? Deleted { get; set; }
        string PageName { get; set; }
        string PageTitle { get; set; }
        string PagePath { get; set; }
        string PageSkin { get; set; }
        bool? PageVisible { get; set; }
        int Page { get; set; }
        int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagParentid))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagParentid), out tmpId))
                {
                    ParentId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_ParentIdNotNumeric", Constants.LocalResourceFile), FlagParentid);
                }
            }
            else if (args.Length > 1 && !IsFlag(args[1]))
            {
                int tmpId;
                if (int.TryParse(args[1], out tmpId))
                {
                    ParentId = tmpId;
                }
            }

            if (HasFlag(FlagDeleted))
            {
                // if flag is specified but has no value, default to it being true
                if (string.IsNullOrEmpty(Flag(FlagDeleted)))
                {
                    Deleted = true;
                }
                else
                {
                    bool tmpDeleted;
                    if (bool.TryParse(Flag(FlagDeleted), out tmpDeleted))
                    {
                        Deleted = tmpDeleted;
                    }
                    else
                    {
                        sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_IfSpecifiedMustHaveValue", Constants.LocalResourceFile), FlagDeleted);
                    }
                }
            }

            if (HasFlag(FlagVisible))
            {
                bool tmp;
                if (bool.TryParse(Flag(FlagVisible), out tmp))
                {
                    PageVisible = tmp;
                }
                else if (Flag(FlagVisible, null) == null)
                {
                    // default to true
                    PageVisible = true;
                }
                else
                {
                    sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_IfSpecifiedMustHaveValue", Constants.LocalResourceFile), FlagVisible);
                }
            }

            if (HasFlag(FlagName))
                PageName = Flag(FlagName);
            if (HasFlag(FlagTitle))
                PageTitle = Flag(FlagTitle);
            if (HasFlag(FlagPath))
                PagePath = Flag(FlagPath);
            if (HasFlag(FlagSkin))
                PageSkin = Flag(FlagSkin);
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
                    Max = tmpId > 0 && tmpId < 100 ? tmpId : Max;
            }


            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lstOut = new List<PageModelBase>();
            int total;
            var lstTabs = PagesController.Instance.GetPageList(Deleted, PageName, PageTitle, PagePath, PageSkin, PageVisible, ParentId, out total, string.Empty, Page > 0 ? Page - 1 : 0, Max);
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
                        ? DotNetNuke.Services.Localization.Localization.GetString("Prompt_ListPagesOutput", Constants.LocalResourceFile)
                        : DotNetNuke.Services.Localization.Localization.GetString("Prompt_NoPages", Constants.LocalResourceFile),
                FieldOrder = new[]
                {
                    "TabId", "ParentId", "Name", "Title", "Skin", "Path", "IncludeInMenu", "IsDeleted"
                }
            };
        }
    }
}