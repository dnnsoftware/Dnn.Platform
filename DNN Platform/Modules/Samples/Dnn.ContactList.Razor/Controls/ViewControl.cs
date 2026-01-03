using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using Dnn.ContactList.Api;
using Dnn.ContactList.Razor.Models;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Pages;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

namespace Dnn.ContactList.Razor
{

    public class ViewControl : RazorModuleControlBase, IPageContributor, IActionable
    {
        private readonly IContactRepository _repository;

        // Constructor with dependency injection
        public ViewControl(IContactRepository repository)
        {
            Requires.NotNull(repository);
            _repository = repository;
        }

        // IActionable implementation to add module actions
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();

                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    string.Empty,
                    this.EditUrl(),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                return actions;
            }
        }

        // Configure the page settings (separate from rendering)
        public void ConfigurePage(PageConfigurationContext context)
        {
            var contacts = _repository.GetContacts(PortalSettings.PortalId);

            context.PageService.SetTitle("Contact List - " + contacts.Count());
            context.PageService.SetDescription("Contact List description - " + contacts.Count());
            context.PageService.SetKeyWords("keywords1");

            context.PageService.AddInfoMessage("", "This is a simple contact list module built using Razor and DNN's MVC Pipeline");
        }

        // Render the html for module control
        public override IRazorModuleResult Invoke()
        {
            var contacts = _repository.GetContacts(PortalSettings.PortalId);

            return View(new ContactsModel()
            {
                IsEditable = ModuleContext.IsEditable,
                EditUrl = ModuleContext.EditUrl(),
                Contacts = contacts.Select(c => new ContactModel()
                {
                    ContactId = c.ContactId,
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Social = c.Social,
                    IsEditable = ModuleContext.IsEditable,
                    EditUrl = ModuleContext.IsEditable ? this.EditUrl("ContactId", c.ContactId.ToString(), "Edit") : string.Empty
                }).ToList()
            });
        }
    }
}
