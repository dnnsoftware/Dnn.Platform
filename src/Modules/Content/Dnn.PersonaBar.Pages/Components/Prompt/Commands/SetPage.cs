using System;
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Components.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("set-page", Constants.PagesCategory, "Prompt_SetPage_Description")]
    public class SetPage : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourceFile;

        [FlagParameter("id", "Prompt_SetPage_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("title", "Prompt_SetPage_FlagTitle", "String")]
        private const string FlagTitle = "title";

        [FlagParameter("name", "Prompt_SetPage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("description", "Prompt_SetPage_FlagDescription", "String")]
        private const string FlagDescription = "description";

        [FlagParameter("keywords", "Prompt_SetPage_FlagKeywords", "String")]
        private const string FlagKeywords = "keywords";

        [FlagParameter("visible", "Prompt_SetPage_FlagVisible", "Boolean")]
        private const string FlagVisible = "visible";

        [FlagParameter("url", "Prompt_SetPage_FlagUrl", "String")]
        private const string FlagUrl = "url";

        [FlagParameter("parentid", "Prompt_SetPage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        private int PageId { get; set; }
        private int? ParentId { get; set; }
        private string Title { get; set; }
        private string Name { get; set; }
        private string Url { get; set; }
        private string Description { get; set; }
        private string Keywords { get; set; }
        private bool? Visible { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            PageId = GetFlagValue(FlagId, "Page Id", -1, true, true, true);
            ParentId = GetFlagValue<int?>(FlagParentId, "Parent Id", null);
            Title = GetFlagValue(FlagTitle, "Title", string.Empty);
            Name = GetFlagValue(FlagName, "Page Name", string.Empty);
            Url = GetFlagValue(FlagUrl, "Url", string.Empty);
            Description = GetFlagValue(FlagDescription, "Description", string.Empty);
            Keywords = GetFlagValue(FlagKeywords, "Keywords", string.Empty);
            Visible = GetFlagValue<bool?>(FlagVisible, "Visible", null);
            if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(Keywords) && string.IsNullOrEmpty(Url) && !ParentId.HasValue && !Visible.HasValue)
            {
                AddMessage(string.Format(LocalizeString("Prompt_NothingToUpdate"), FlagTitle, FlagDescription, FlagName, FlagVisible));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var pageSettings = PagesController.Instance.GetPageSettings(PageId, PortalSettings);
                if (pageSettings == null)
                {
                    return new ConsoleErrorResultModel(LocalizeString("PageNotFound"));
                }
                pageSettings.Name = !string.IsNullOrEmpty(Name) ? Name : pageSettings.Name;
                pageSettings.Title = !string.IsNullOrEmpty(Title) ? Title : pageSettings.Title;
                pageSettings.Url = !string.IsNullOrEmpty(Url) ? Url : pageSettings.Url;         
                pageSettings.Description = !string.IsNullOrEmpty(Description) ? Description : pageSettings.Description;
                pageSettings.Keywords = !string.IsNullOrEmpty(Keywords) ? Keywords : pageSettings.Keywords;
                pageSettings.ParentId = ParentId.HasValue ? ParentId : pageSettings.ParentId;
                pageSettings.IncludeInMenu = Visible ?? pageSettings.IncludeInMenu;
                if (!SecurityService.Instance.CanSavePageDetails(pageSettings))
                {
                    return new ConsoleErrorResultModel(LocalizeString("MethodPermissionDenied"));
                }
                var updatedTab = PagesController.Instance.SavePageDetails(PortalSettings, pageSettings);

                var lstResults = new List<PageModel> { new PageModel(updatedTab) };
                return new ConsoleResultModel(LocalizeString("PageUpdatedMessage")) { Data = lstResults, Records = lstResults.Count };
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