using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Page
{
    [ConsoleCommand("goto", "Navigates to a specified page within the DNN site", new[]{
        "id",
        "name"
    })]
    public class Goto : ConsoleCommandBase
    {


        public int? PageId { get; private set; }
        public string PageName { get; private set; }
        public int? ParentId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            // default usage: return current page if nothing else specified
            if (args.Length == 1)
            {
                PageId = TabId;
            }
            else if (args.Length == 2)
            {
                var tmpId = 0;
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
                var tmpId = 0;
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
                var tmpId = 0;
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

        public override ConsoleResultModel Run()
        {
            var tc = new TabController();
            var lst = new List<PageModel>();

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