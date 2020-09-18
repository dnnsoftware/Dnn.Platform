// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
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

    [ConsoleCommand("set-page", Constants.PagesCategory, "Prompt_SetPage_Description")]
    public class SetPage : ConsoleCommandBase
    {
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

        public override string LocalResourceFile => Constants.LocalResourceFile;

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

            this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, true, true, true);
            this.ParentId = this.GetFlagValue<int?>(FlagParentId, "Parent Id", null);
            this.Title = this.GetFlagValue(FlagTitle, "Title", string.Empty);
            this.Name = this.GetFlagValue(FlagName, "Page Name", string.Empty);
            this.Url = this.GetFlagValue(FlagUrl, "Url", string.Empty);
            this.Description = this.GetFlagValue(FlagDescription, "Description", string.Empty);
            this.Keywords = this.GetFlagValue(FlagKeywords, "Keywords", string.Empty);
            this.Visible = this.GetFlagValue<bool?>(FlagVisible, "Visible", null);
            if (string.IsNullOrEmpty(this.Title) && string.IsNullOrEmpty(this.Name) && string.IsNullOrEmpty(this.Description) && string.IsNullOrEmpty(this.Keywords) && string.IsNullOrEmpty(this.Url) && !this.ParentId.HasValue && !this.Visible.HasValue)
            {
                this.AddMessage(string.Format(this.LocalizeString("Prompt_NothingToUpdate"), FlagTitle, FlagDescription, FlagName, FlagVisible));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var pageSettings = PagesController.Instance.GetPageSettings(this.PageId, this.PortalSettings);
                if (pageSettings == null)
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("PageNotFound"));
                }
                pageSettings.Name = !string.IsNullOrEmpty(this.Name) ? this.Name : pageSettings.Name;
                pageSettings.Title = !string.IsNullOrEmpty(this.Title) ? this.Title : pageSettings.Title;
                pageSettings.Url = !string.IsNullOrEmpty(this.Url) ? this.Url : pageSettings.Url;
                pageSettings.Description = !string.IsNullOrEmpty(this.Description) ? this.Description : pageSettings.Description;
                pageSettings.Keywords = !string.IsNullOrEmpty(this.Keywords) ? this.Keywords : pageSettings.Keywords;
                pageSettings.ParentId = this.ParentId.HasValue ? this.ParentId : pageSettings.ParentId;
                pageSettings.IncludeInMenu = this.Visible ?? pageSettings.IncludeInMenu;
                if (!SecurityService.Instance.CanSavePageDetails(pageSettings))
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("MethodPermissionDenied"));
                }
                var updatedTab = PagesController.Instance.SavePageDetails(this.PortalSettings, pageSettings);

                var lstResults = new List<PageModel> { new PageModel(updatedTab) };
                return new ConsoleResultModel(this.LocalizeString("PageUpdatedMessage")) { Data = lstResults, Records = lstResults.Count };
            }
            catch (PageNotFoundException)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("PageNotFound"));
            }
            catch (PageValidationException ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}
