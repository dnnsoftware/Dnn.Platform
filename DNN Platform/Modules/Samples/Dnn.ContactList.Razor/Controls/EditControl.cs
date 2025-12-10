using Dnn.ContactList.Api;
using Dnn.ContactList.Razor.Models;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.ContactList.Razor
{

    public class EditControl : RazorModuleControlBase, IPageContributor
    {
        private readonly IContactRepository _repository;

        public EditControl() : this(ContactRepository.Instance)
        {

        }
        public EditControl(IContactRepository repository)
        {
            Requires.NotNull(repository);

            _repository = repository;
            LocalResourceFile = "~/DesktopModules/Dnn/RazorContactList/App_LocalResources/Contact.resx";
        }

        public override string ControlName => "Edit";

        public void ConfigurePage(PageConfigurationContext context)
        {
            context.ServicesFramework.RequestAjaxAntiForgerySupport();
            context.ServicesFramework.RequestAjaxScriptSupport();
            context.ClientResourceController.CreateScript("~/DesktopModules/Dnn/RazorContactList/js/edit.js").Register();
            context.PageService.SetTitle("Razor - Edit Contact");
        }

        public override IRazorModuleResult Invoke()
        {
            var returnUrl = ModuleContext.NavigateUrl(this.TabId, string.Empty, false);
            if (int.TryParse(Request.QueryString["ContactId"], out int contactId))
            {
                var c = _repository.GetContact(contactId, PortalSettings.PortalId);

                if (c == null)
                {
                    return Error("ContactList error", "contact dous not exist : "+ contactId);
                }

                return View(new ContactModel()
                {
                    ContactId = c.ContactId,
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Social = c.Social,
                    ReturnUrl = returnUrl
                });
            }
            else
            {
                return View(new ContactModel()
                {
                    ReturnUrl = returnUrl
                });
            }
        }
    }
}
