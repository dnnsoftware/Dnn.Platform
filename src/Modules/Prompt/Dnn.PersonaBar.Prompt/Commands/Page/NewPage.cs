using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("new-page", "Create a new page in the portal", new string[]{
        "title",
        "name",
        "url",
        "parentid",
        "description",
        "keywords",
        "visible"
    })]
    public class NewPage : BaseConsoleCommand, IConsoleCommand
    {
        private const string FLAG_PARENTID = "parentid";
        private const string FLAG_TITLE = "title";
        private const string FLAG_NAME = "name";
        private const string FLAG_URL = "url";
        private const string FLAG_DESCRIPTION = "description";
        private const string FLAG_KEYWORDS = "keywords";
        private const string FLAG_VISIBLE = "visible";

        public string ValidationMessage { get; private set; }
        public string Title { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }
        public int? ParentId { get; private set; }
        public string Description { get; private set; }
        public string Keywords { get; private set; }
        public bool? Visible { get; private set; }

        private TabInfo CurrentTab { get; set; }
        private TabInfo ParentTab { get; set; }


        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);

            CurrentTab = (new TabController()).GetTab(activeTabId, portalSettings.PortalId);

            StringBuilder sbErrors = new StringBuilder();
            if (HasFlag(FLAG_PARENTID))
            {
                int tempId = 0;
                if (int.TryParse(Flag(FLAG_PARENTID), out tempId))
                {
                    ParentId = tempId;
                }
                else if (Flag(FLAG_PARENTID) == "me")
                {
                    ParentId = activeTabId;
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid Page (Tab) ID; ", FLAG_PARENTID);
                }
            }

            if (HasFlag(FLAG_TITLE))
                Title = Flag(FLAG_TITLE);
            if (HasFlag(FLAG_NAME))
            {
                Name = Flag(FLAG_NAME);
            }
            else
            {
                // Let Name be the default argument
                if (args.Length > 1 && !IsFlag(args[1]))
                {
                    Name = args[1];
                }
            }

            if (HasFlag(FLAG_URL))
                Url = Flag(FLAG_URL);
            if (HasFlag(FLAG_DESCRIPTION))
                Description = Flag(FLAG_DESCRIPTION);
            if (HasFlag(FLAG_KEYWORDS))
                Keywords = Flag(FLAG_KEYWORDS);
            if (HasFlag(FLAG_VISIBLE))
            {
                bool tempVisible = false;
                if (!bool.TryParse(Flag(FLAG_VISIBLE), out tempVisible))
                {
                    sbErrors.AppendFormat("You must pass True or False for the --{0} flag; ", FLAG_VISIBLE);
                }
                else
                {
                    Visible = tempVisible;
                }
            }
            else
            {
                Visible = true; // default
            }

            // Check for required fields here
            if (string.IsNullOrEmpty(Name))
            {
                sbErrors.AppendFormat("--{0} is required", FLAG_NAME);
            }

            // validate that parent ID is a valid ID, if it has been passed
            if (ParentId.HasValue)
            {
                var testTab = TabController.Instance.GetTab((int)ParentId, PortalId);
                if (testTab == null)
                {
                    sbErrors.AppendFormat("Unable to find page specified for --{0} '{1}'", FLAG_PARENTID, ParentId);
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            StringBuilder sbMessage = new StringBuilder();
            try
            {
                TabInfo tab = new TabInfo();
                tab.TabName = Name;
                tab.PortalID = PortalId;
                if (!string.IsNullOrEmpty(Title))
                    tab.Title = Title;
                if (!string.IsNullOrEmpty(Url))
                    tab.Url = Url;
                if (!string.IsNullOrEmpty(Description))
                    tab.Description = Description;
                if (!string.IsNullOrEmpty(Keywords))
                    tab.KeyWords = Keywords;
                if (ParentId.HasValue)
                {
                    tab.ParentId = (int)ParentId;
                }
                else
                {
                    tab.ParentId = CurrentTab.ParentId;
                }
                ParentTab = (new TabController()).GetTab(tab.ParentId, PortalId);
                if (ParentTab != null)
                {
                    tab.TabPermissions.AddRange(ParentTab.TabPermissions);
                }

                tab.IsVisible = (bool)Visible;

                // create the tab
                var lstResults = new List<PageModel>();
                var newTabId = TabController.Instance.AddTab(tab);
                if (newTabId > 0)
                {
                    // success
                    var addedTab = TabController.Instance.GetTab(newTabId, PortalId);
                    if (addedTab == null)
                    {
                        return new ConsoleErrorResultModel(string.Format("Unable to retrieve newly created page with ID of '{0}'", newTabId));
                    }
                    lstResults.Add(new PageModel(addedTab));
                }
                else
                {
                    return new ConsoleErrorResultModel(string.Format("Unable to create the new page"));
                }

                return new ConsoleResultModel("The page has been created") { data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return new ConsoleErrorResultModel("An unexpected error has occurred, please see the Event Viewer for more details");
        }


    }
}