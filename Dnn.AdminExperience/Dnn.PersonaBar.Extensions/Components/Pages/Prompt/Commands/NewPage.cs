// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
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

    [ConsoleCommand("new-page", Constants.PagesCategory, "Prompt_NewPage_Description")]
    public class NewPage : ConsoleCommandBase
    {
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

        public override string LocalResourceFile => Constants.LocalResourceFile;

        private string Title { get; set; }
        private string Name { get; set; }
        private string Url { get; set; }
        private int? ParentId { get; set; }
        private string Description { get; set; }
        private string Keywords { get; set; }
        private bool? Visible { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.ParentId = this.GetFlagValue<int?>(FlagParentId, "Parent Id", null, false, false, true);
            this.Title = this.GetFlagValue(FlagTitle, "Title", string.Empty);
            this.Name = this.GetFlagValue(FlagName, "Page Name", string.Empty, true, true);
            this.Url = this.GetFlagValue(FlagUrl, "Url", string.Empty);
            this.Description = this.GetFlagValue(FlagDescription, "Description", string.Empty);
            this.Keywords = this.GetFlagValue(FlagKeywords, "Keywords", string.Empty);
            this.Visible = this.GetFlagValue(FlagVisible, "Visible", true);

            // validate that parent ID is a valid ID, if it has been passed
            if (!this.ParentId.HasValue) return;
            var testTab = TabController.Instance.GetTab((int)this.ParentId, this.PortalId);
            if (testTab == null)
            {
                this.AddMessage(string.Format(this.LocalizeString("Prompt_UnableToFindSpecified"), FlagParentId, this.ParentId));
            }
        }

        public override ConsoleResultModel Run()
        {

            try
            {
                var pageSettings = PagesController.Instance.GetDefaultSettings();
                pageSettings.Name = !string.IsNullOrEmpty(this.Name) ? this.Name : pageSettings.Name;
                pageSettings.Title = !string.IsNullOrEmpty(this.Title) ? this.Title : pageSettings.Title;
                pageSettings.Url = !string.IsNullOrEmpty(this.Url) ? this.Url : pageSettings.Url;
                pageSettings.Description = !string.IsNullOrEmpty(this.Description) ? this.Description : pageSettings.Description;
                pageSettings.Keywords = !string.IsNullOrEmpty(this.Keywords) ? this.Keywords : pageSettings.Keywords;
                pageSettings.ParentId = this.ParentId.HasValue ? this.ParentId : pageSettings.ParentId;
                pageSettings.HasParent = this.ParentId.HasValue;
                pageSettings.IncludeInMenu = this.Visible ?? pageSettings.IncludeInMenu;
                pageSettings.IncludeInMenu = this.Visible ?? true;
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
                    return new ConsoleErrorResultModel(this.LocalizeString("MethodPermissionDenied"));
                }
                var newTab = PagesController.Instance.SavePageDetails(this.PortalSettings, pageSettings);

                // create the tab
                var lstResults = new List<PageModel>();
                if (newTab != null && newTab.TabID > 0)
                {
                    lstResults.Add(new PageModel(newTab));
                }
                else
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageCreateFailed"));
                }

                return new ConsoleResultModel(this.LocalizeString("Prompt_PageCreated")) { Data = lstResults, Records = lstResults.Count };
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
