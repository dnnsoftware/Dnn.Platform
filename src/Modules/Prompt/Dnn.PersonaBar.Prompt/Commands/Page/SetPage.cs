using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("set-page", "Update page with new data", new string[]{
        "id",
        "title",
        "name",
        "url",
        "parentid",
        "description",
        "keywords",
        "visible"
    })]
    public class SetPage : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_ID = "id";
        private const string FLAG_TITLE = "title";
        private const string FLAG_NAME = "name";
        private const string FLAG_DESCRIPTION = "description";
        private const string FLAG_KEYWORDS = "keywords";
        private const string FLAG_VISIBLE = "visible";
        private const string FLAG_URL = "url";
        private const string FLAG_PARENTID = "parentid";

        public string ValidationMessage { get; private set; }
        public int? PageId { get; private set; }
        public int? ParentId { get; private set; }
        public string Title { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public string Keywords { get; private set; }
        public bool? Visible { get; private set; }


        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);

            StringBuilder sbErrors = new StringBuilder();
            if (HasFlag(FLAG_ID))
            {
                int tempId = 0;
                if (!int.TryParse(Flag(FLAG_ID), out tempId))
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid Page (Tab) ID; ", FLAG_ID);
                }
                else
                {
                    PageId = tempId;
                }
            }
            else
            {
                int tempId = 0;
                if (!int.TryParse(args[1], out tempId))
                {
                    PageId = base.TabId;
                }
                else
                {
                    PageId = tempId;
                }
            }
            if (HasFlag(FLAG_PARENTID))
            {
                int tempId = 0;
                if (!int.TryParse(Flag(FLAG_PARENTID), out tempId))
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid Page ID; ", FLAG_PARENTID);
                }
                else
                {
                    if (tempId == PageId)
                    {
                        sbErrors.AppendFormat("The --{0} flag value cannot be the same as the page you are trying to update; ", FLAG_PARENTID);
                    }
                    else
                    {
                        ParentId = tempId;
                    }
                }
            }
            if (HasFlag(FLAG_TITLE))
                Title = Flag(FLAG_TITLE);
            if (HasFlag(FLAG_NAME))
                Name = Flag(FLAG_NAME);
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

            if (Title == null && Name == null && Description == null && Keywords == null && Url == null && (!ParentId.HasValue) && (!Visible.HasValue))
            {
                sbErrors.AppendFormat("Nothing to Update! Tell me what to update with flags like --{0} --{1} --{2} --{3}, etc.", FLAG_TITLE, FLAG_DESCRIPTION, FLAG_NAME, FLAG_VISIBLE);
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
                var tab = TabController.Instance.GetTab((int)PageId, PortalId);

                if (tab == null)
                {
                    return new ConsoleErrorResultModel("No tab found to update.");
                }
                // Only check whether value has been set. This allows user to clear out 
                // a value with an empty string (except page Name which is always required)
                if (Title != null)
                    tab.Title = Title;
                if (!string.IsNullOrEmpty(Name))
                    tab.TabName = Name;
                if (Description != null)
                    tab.Description = Description;
                if (Keywords != null)
                    tab.KeyWords = Keywords;
                if (Url != null)
                    tab.Url = Url;

                if (ParentId.HasValue)
                    tab.ParentId = (int)ParentId;
                if (Visible.HasValue)
                    tab.IsVisible = (bool)Visible;

                // update the tab
                TabController.Instance.UpdateTab(tab);

                var lstResults = new List<PageModel>();
                lstResults.Add(new PageModel(TabController.Instance.GetTab(tab.TabID, tab.PortalID)));
                return new ConsoleResultModel("Page has been updated") { Data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return new ConsoleErrorResultModel("An unexpected error has occurred, please see the Event Viewer for more details");
        }


    }
}