using System;
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
    [ConsoleCommand("new-page", "Create a new page in the portal", new[]{
        "title",
        "name",
        "url",
        "parentid",
        "description",
        "keywords",
        "visible"
    })]
    public class NewPage : ConsoleCommandBase
    {
        private const string FlagParentid = "parentid";
        private const string FlagTitle = "title";
        private const string FlagName = "name";
        private const string FlagUrl = "url";
        private const string FlagDescription = "description";
        private const string FlagKeywords = "keywords";
        private const string FlagVisible = "visible";


        public string Title { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }
        public int? ParentId { get; private set; }
        public string Description { get; private set; }
        public string Keywords { get; private set; }
        public bool? Visible { get; private set; }

        private TabInfo CurrentTab { get; set; }
        private TabInfo ParentTab { get; set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            CurrentTab = new TabController().GetTab(activeTabId, portalSettings.PortalId);

            var sbErrors = new StringBuilder();
            if (HasFlag(FlagParentid))
            {
                var tempId = 0;
                if (int.TryParse(Flag(FlagParentid), out tempId))
                {
                    ParentId = tempId;
                }
                else if (Flag(FlagParentid) == "me")
                {
                    ParentId = activeTabId;
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid Page (Tab) ID; ", FlagParentid);
                }
            }

            if (HasFlag(FlagTitle))
                Title = Flag(FlagTitle);
            if (HasFlag(FlagName))
            {
                Name = Flag(FlagName);
            }
            else
            {
                // Let Name be the default argument
                if (args.Length > 1 && !IsFlag(args[1]))
                {
                    Name = args[1];
                }
            }

            if (HasFlag(FlagUrl))
                Url = Flag(FlagUrl);
            if (HasFlag(FlagDescription))
                Description = Flag(FlagDescription);
            if (HasFlag(FlagKeywords))
                Keywords = Flag(FlagKeywords);
            if (HasFlag(FlagVisible))
            {
                var tempVisible = false;
                if (!bool.TryParse(Flag(FlagVisible), out tempVisible))
                {
                    sbErrors.AppendFormat("You must pass True or False for the --{0} flag; ", FlagVisible);
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
                sbErrors.AppendFormat("--{0} is required", FlagName);
            }

            // validate that parent ID is a valid ID, if it has been passed
            if (ParentId.HasValue)
            {
                var testTab = TabController.Instance.GetTab((int)ParentId, PortalId);
                if (testTab == null)
                {
                    sbErrors.AppendFormat("Unable to find page specified for --{0} '{1}'", FlagParentid, ParentId);
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var sbMessage = new StringBuilder();
            try
            {
                var tab = new TabInfo();
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
                ParentTab = new TabController().GetTab(tab.ParentId, PortalId);
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
                        return new ConsoleErrorResultModel(
                            $"Unable to retrieve newly created page with ID of '{newTabId}'");
                    }
                    lstResults.Add(new PageModel(addedTab));
                }
                else
                {
                    return new ConsoleErrorResultModel(string.Format("Unable to create the new page"));
                }

                return new ConsoleResultModel("The page has been created") { Data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return new ConsoleErrorResultModel("An unexpected error has occurred, please see the Event Viewer for more details");
        }


    }
}