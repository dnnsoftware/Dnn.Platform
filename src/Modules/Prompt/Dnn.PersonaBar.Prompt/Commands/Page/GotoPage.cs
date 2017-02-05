using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("goto", "Navigates to a specified page within the DNN site", new string[]{
        "id",
        "name"
    })]
    public class Goto : BaseConsoleCommand, IConsoleCommand
    {

        public string ValidationMessage { get; private set; }
        public int? PageId { get; private set; }
        public string PageName { get; private set; }
        public int? ParentId { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            // default usage: return current page if nothing else specified
            if (args.Length == 1)
            {
                PageId = base.TabId;
            }
            else if (args.Length == 2)
            {
                int tmpId = 0;
                if (!int.TryParse(args[1], out tmpId))
                {
                    sbErrors.Append("No valid Page ID specified; ");
                }
                else
                {
                    PageId = tmpId;
                }
            }
            else
            {
                int tmpId = 0;
                if (HasFlag("id"))
                {
                    if (!int.TryParse(Flag("id"), out tmpId))
                    {
                        sbErrors.Append("You must specify a valid number for Page ID; ");
                    }
                    else
                    {
                        PageId = tmpId;
                    }
                }
            }

            if (HasFlag("parentid"))
            {
                int tmpId = 0;
                if (int.TryParse(Flag("parentid"), out tmpId))
                    ParentId = tmpId;
            }

            PageName = Flag("name");

            if (!PageId.HasValue && string.IsNullOrEmpty(PageName))
            {
                sbErrors.Append("You must specify either a Page ID (number) or a Page Name (string)");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            TabController tc = new TabController();
            List<PageModel> lst = new List<PageModel>();

            if (PageId.HasValue)
            {
                var tab = tc.GetTab((int)PageId, PortalId);
                if (tab != null)
                {
                    return new ConsoleResultModel(tab.FullUrl);
                }
                else
                {
                    return new ConsoleResultModel("Page Not Found");
                }

            }
            else
            {
                return new ConsoleResultModel("Page Not Found");

            }

        }


    }
}