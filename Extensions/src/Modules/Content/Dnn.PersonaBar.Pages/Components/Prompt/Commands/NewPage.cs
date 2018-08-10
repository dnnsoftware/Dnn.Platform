using System.Collections.Generic;
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
    [ConsoleCommand("new-page", Constants.PagesCategory, "Prompt_NewPage_Description")]
    public class NewPage : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourceFile;

        [FlagParameter("parentid", "Prompt_NewPage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        [FlagParameter("title", "Prompt_NewPage_FlagTitle", "String")]
        private const string FlagTitle = "title";

        [FlagParameter("name", "Prompt_NewPage_FlagName", "String", true)]
        private const string FlagName = "name";

        [FlagParameter("url", "Prompt_NewPage_FlagUrl", "String")]
        private const string FlagUrl = "url";

        [FlagParameter("description", "Prompt_NewPage_FlagDescription", "String")]
        private const string FlagDescription = "description";

        [FlagParameter("keywords", "Prompt_NewPage_FlagKeywords", "String")]
        private const string FlagKeywords = "keywords";

        [FlagParameter("visible", "Prompt_NewPage_FlagVisible", "Boolean", "true")]
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
            
            ParentId = GetFlagValue<int?>(FlagParentId, "Parent Id", null, false, false, true);
            Title = GetFlagValue(FlagTitle, "Title", string.Empty);
            Name = GetFlagValue(FlagName, "Page Name", string.Empty, true, true);
            Url = GetFlagValue(FlagUrl, "Url", string.Empty);
            Description = GetFlagValue(FlagDescription, "Description", string.Empty);
            Keywords = GetFlagValue(FlagKeywords, "Keywords", string.Empty);
            Visible = GetFlagValue(FlagVisible, "Visible", true);

            // validate that parent ID is a valid ID, if it has been passed
            if (!ParentId.HasValue) return;
            var testTab = TabController.Instance.GetTab((int)ParentId, PortalId);
            if (testTab == null)
            {
                AddMessage(string.Format(LocalizeString("Prompt_UnableToFindSpecified"), FlagParentId, ParentId));
            }
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
                    return new ConsoleErrorResultModel(LocalizeString("MethodPermissionDenied"));
                }
                var newTab = PagesController.Instance.SavePageDetails(PortalSettings, pageSettings);

                // create the tab
                var lstResults = new List<PageModel>();
                if (newTab != null && newTab.TabID > 0)
                {
                    lstResults.Add(new PageModel(newTab));
                }
                else
                {
                    return new ConsoleErrorResultModel(LocalizeString("Prompt_PageCreateFailed"));
                }

                return new ConsoleResultModel(LocalizeString("Prompt_PageCreated")) { Data = lstResults, Records = lstResults.Count };
            }
            catch (PageNotFoundException)
            {
                return new ConsoleErrorResultModel(LocalizeString("PageNotFound"));
            }
            catch (PageValidationException ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}