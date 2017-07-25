using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Components.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
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

        private string Title { get; set; }
        private string Name { get; set; }
        private string Url { get; set; }
        private int? ParentId { get; set; }
        private string Description { get; set; }
        private string Keywords { get; set; }
        private bool? Visible { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();
            if (HasFlag(FlagParentid))
            {
                int tempId;
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
                    sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_InvalidParentId", Constants.LocalResourceFile), FlagParentid);
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
                bool tempVisible;
                if (!bool.TryParse(Flag(FlagVisible), out tempVisible))
                {
                    sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_TrueFalseRequired", Constants.LocalResourceFile), FlagVisible);
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
                sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_FlagRequired", Constants.LocalResourceFile), FlagName);
            }

            // validate that parent ID is a valid ID, if it has been passed
            if (ParentId.HasValue)
            {
                var testTab = TabController.Instance.GetTab((int)ParentId, PortalId);
                if (testTab == null)
                {
                    sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_UnableToFindSpecified", Constants.LocalResourceFile), FlagParentid, ParentId);
                }
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {

            try
            {
                var pageSettings = PagesController.Instance.GetDefaultSettings();
                pageSettings.Name = !string.IsNullOrEmpty(Name) ? Name : pageSettings.Name;
                pageSettings.Title = !string.IsNullOrEmpty(Title) ? Title : pageSettings.Title;
                pageSettings.Url = !string.IsNullOrEmpty(Url) ? Url : pageSettings.Url;
                pageSettings.Description = !string.IsNullOrEmpty(Description) ? Description : pageSettings.Description;
                pageSettings.Keywords = !string.IsNullOrEmpty(Keywords) ? Keywords : pageSettings.Keywords;
                pageSettings.ParentId = ParentId.HasValue ? ParentId : pageSettings.ParentId;
                pageSettings.HasParent = ParentId.HasValue;
                pageSettings.IncludeInMenu = Visible ?? pageSettings.IncludeInMenu;
                pageSettings.IncludeInMenu = Visible ?? true;
                if (pageSettings.ParentId != null)
                {
                    var parentTab = PagesController.Instance.GetPageSettings(pageSettings.ParentId.Value);
                    if (parentTab != null)
                    {
                        pageSettings.Permissions = parentTab.Permissions;
                    }
                }

                if (!SecurityService.Instance.CanSavePageDetails(pageSettings))
                {
                    return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("MethodPermissionDenied", Constants.LocalResourceFile));
                }
                var newTab = PagesController.Instance.SavePageDetails(pageSettings);

                // create the tab
                var lstResults = new List<PageModel>();
                if (newTab != null && newTab.TabID > 0)
                {
                    lstResults.Add(new PageModel(newTab));
                }
                else
                {
                    return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("Prompt_PageCreateFailed", Constants.LocalResourceFile));
                }

                return new ConsoleResultModel(DotNetNuke.Services.Localization.Localization.GetString("Prompt_PageCreated", Constants.LocalResourceFile)) { Data = lstResults, Records = lstResults.Count };
            }
            catch (PageNotFoundException)
            {
                return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Constants.LocalResourceFile));
            }
            catch (PageValidationException ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}