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
        private Contact _contact = null;

        public EditControl(IContactRepository repository)
        {
            Requires.NotNull(repository);
            _repository = repository;
        }

        public void ConfigurePage(PageConfigurationContext context)
        {
            // Enable Anti-Forgery support for AJAX calls
            context.ServicesFramework.RequestAjaxAntiForgerySupport();
            // Enable Service Framework support for AJAX calls
            context.ServicesFramework.RequestAjaxScriptSupport();
            // Register JavaScript file
            context.ClientResourceController.CreateScript("~/DesktopModules/Dnn/RazorContactList/js/edit.js").Register();
            // Set the page title
            if (TryGetContact(out Contact contact))
            {
                if (contact != null)
                {
                    context.PageService.SetTitle("Razor - Edit Contact: " + contact.FirstName + " " + contact.LastName);
                }
                else
                {
                    context.PageService.SetTitle("Razor - Edit Contact");
                }
            }
        }

        public override IRazorModuleResult Invoke()
        {
            var returnUrl = ModuleContext.NavigateUrl(this.TabId, string.Empty, false);
            // Check if we are editing an existing contact
            if (TryGetContact(out Contact c))
            {
                if (c == null)
                {
                    return View(new ContactModel()
                    {
                        ReturnUrl = returnUrl
                    });
                }
                else
                {
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
            }
            else
            {
                // Contact does not exist
                return Error("ContactList error", "contact dous not exist");
            }
        }

        private bool TryGetContact(out Contact contact)
        {
            if (int.TryParse(Request.QueryString["ContactId"], out int contactId))
            {
                if (_contact == null)
                {
                    _contact = _repository.GetContact(contactId, PortalSettings.PortalId);
                    if (_contact == null)
                    {
                        // Contact does not exist
                        contact = null;
                        return false;
                    }
                }
            }
            contact = _contact;
            return true;
        }
    }
}
