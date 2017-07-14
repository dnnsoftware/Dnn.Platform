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
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Page
{
    [ConsoleCommand("set-page", "Update page with new data", new[]{
        "id",
        "title",
        "name",
        "url",
        "parentid",
        "description",
        "keywords",
        "visible"
    })]
    public class SetPage : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetPage));

        private const string FlagId = "id";
        private const string FlagTitle = "title";
        private const string FlagName = "name";
        private const string FlagDescription = "description";
        private const string FlagKeywords = "keywords";
        private const string FlagVisible = "visible";
        private const string FlagUrl = "url";
        private const string FlagParentid = "parentid";


        public int? PageId { get; private set; }
        public int? ParentId { get; private set; }
        public string Title { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public string Keywords { get; private set; }
        public bool? Visible { get; private set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();
            if (HasFlag(FlagId))
            {
                var tempId = 0;
                if (!int.TryParse(Flag(FlagId), out tempId))
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid Page (Tab) ID; ", FlagId);
                }
                else
                {
                    PageId = tempId;
                }
            }
            else
            {
                var tempId = 0;
                if (!int.TryParse(args[1], out tempId))
                {
                    PageId = TabId;
                }
                else
                {
                    PageId = tempId;
                }
            }
            if (HasFlag(FlagParentid))
            {
                var tempId = 0;
                if (!int.TryParse(Flag(FlagParentid), out tempId))
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid Page ID; ", FlagParentid);
                }
                else
                {
                    if (tempId == PageId)
                    {
                        sbErrors.AppendFormat("The --{0} flag value cannot be the same as the page you are trying to update; ", FlagParentid);
                    }
                    else
                    {
                        ParentId = tempId;
                    }
                }
            }
            if (HasFlag(FlagTitle))
                Title = Flag(FlagTitle);
            if (HasFlag(FlagName))
                Name = Flag(FlagName);
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

            if (Title == null && Name == null && Description == null && Keywords == null && Url == null && !ParentId.HasValue && !Visible.HasValue)
            {
                sbErrors.AppendFormat("Nothing to Update! Tell me what to update with flags like --{0} --{1} --{2} --{3}, etc.", FlagTitle, FlagDescription, FlagName, FlagVisible);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var sbMessage = new StringBuilder();
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
                Logger.Error(ex);
            }

            return new ConsoleErrorResultModel("An unexpected error has occurred, please see the Event Viewer for more details");
        }


    }
}