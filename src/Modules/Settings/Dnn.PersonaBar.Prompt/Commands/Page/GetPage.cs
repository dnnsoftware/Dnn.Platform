using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("get-page", "Retrieves information about the specified or current page", new[]{
        "id",
        "parentid",
        "name"
    })]
    public class GetPage : ConsoleCommandBase
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
                    lst.Add(new PageModel(tab));
                }

            }
            else
            {
                TabInfo tab = null;
                if (ParentId.HasValue)
                {
                    tab = tc.GetTabByName(PageName, PortalId, (int)ParentId);
                }
                else
                {
                    tab = tc.GetTabByName(PageName, PortalId);
                }

                if (tab != null)
                    lst.Add(new PageModel(tab));
            }

            return new ConsoleResultModel($"{lst.Count} page{(lst.Count != 1 ? "s" : "")} found") { Data = lst };
        }


    }
}