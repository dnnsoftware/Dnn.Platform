using System.Collections.Generic;
using System.Text;
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
        private const string FlagId = "id";
        private const string FlagTitle = "title";
        private const string FlagName = "name";
        private const string FlagDescription = "description";
        private const string FlagKeywords = "keywords";
        private const string FlagVisible = "visible";
        private const string FlagUrl = "url";
        private const string FlagParentid = "parentid";


        int PageId { get; set; }
        int? ParentId { get; set; }
        string Title { get; set; }
        string Name { get; set; }
        string Url { get; set; }
        string Description { get; set; }
        string Keywords { get; set; }
        bool? Visible { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();
            if (HasFlag(FlagId))
            {
                int tempId;
                if (!int.TryParse(Flag(FlagId), out tempId))
                {
                    sbErrors.Append(DotNetNuke.Services.Localization.Localization.GetString("Prompt_NoPageId", Constants.LocalResourceFile));
                }
                else
                {
                    PageId = tempId;
                }
            }
            else
            {
                int tempId;
                if (int.TryParse(args[1], out tempId))
                {
                    PageId = tempId;
                }
                else
                {
                    sbErrors.Append(DotNetNuke.Services.Localization.Localization.GetString("Prompt_NoPageId", Constants.LocalResourceFile));
                }
            }
            if (HasFlag(FlagParentid))
            {
                int tempId;
                if (!int.TryParse(Flag(FlagParentid), out tempId))
                {
                    sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_InvalidParentId", Constants.LocalResourceFile), FlagParentid);
                }
                else
                {
                    if (tempId == PageId)
                    {
                        sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_FlagMatch", Constants.LocalResourceFile), FlagParentid);
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

            if (Title == null && Name == null && Description == null && Keywords == null && Url == null && !ParentId.HasValue && !Visible.HasValue)
            {
                sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_NothingToUpdate", Constants.LocalResourceFile), FlagTitle, FlagDescription, FlagName, FlagVisible);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var pageSettings = PagesController.Instance.GetPageSettings(PageId);
                if (pageSettings == null)
                {
                    return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Constants.LocalResourceFile));
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
                    return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("MethodPermissionDenied", Constants.LocalResourceFile));
                }
                var updatedTab = PagesController.Instance.SavePageDetails(pageSettings);

                var lstResults = new List<PageModel> { new PageModel(updatedTab) };
                return new ConsoleResultModel(DotNetNuke.Services.Localization.Localization.GetString("PageUpdatedMessage", Constants.LocalResourceFile)) { Data = lstResults };
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