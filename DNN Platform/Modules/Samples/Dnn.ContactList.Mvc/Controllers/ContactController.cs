// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web.Mvc;
using Dnn.ContactList.Api;
using DotNetNuke.Abstractions.ClientResources;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.ContactList.Mvc.Controllers
{
    /// <summary>
    /// ContactController is the MVC Controller class for managing Contacts in the UI
    /// </summary>
    [DnnHandleError]
    public class ContactController : DnnController
    {
        private readonly IContactRepository _repository;
        private readonly IClientResourceController clientResourceController;
        private readonly IPageService pageService;

        /// <summary>
        /// Constructor constructs a new ContactController with a passed in repository
        /// </summary>
        public ContactController(IClientResourceController clientResourceController,
                                    IPageService pageService)
        {
            //Requires.NotNull(repository);
            Requires.NotNull(clientResourceController);
            Requires.NotNull(pageService);

            this.clientResourceController = clientResourceController;
            this.pageService = pageService;
            _repository = ContactRepository.Instance;

        }

        /// <summary>
        /// The Delete method is used to delete a Contact
        /// </summary>
        /// <param name="contactId">The Id of the Contact to delete</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Delete(int contactId)
        {
            var contact = _repository.GetContact(contactId, PortalSettings.PortalId);

            _repository.DeleteContact(contact);

            return RedirectToDefaultRoute();
        }

        /// <summary>
        /// This Edit method is used to set up editing a Contact
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(int contactId = -1)
        {
            var contact = (contactId == -1)
                        ? new Contact { PortalId = PortalSettings.PortalId }
                        : _repository.GetContact(contactId, PortalSettings.PortalId);

            return View(contact);
        }

        /// <summary>
        /// This Edit method is used to save the posted Contact
        /// </summary>
        /// <param name="contact">The contact to save</param>
        /// <returns></returns>
        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Edit(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.PortalId = PortalSettings.PortalId;

                if (contact.ContactId == -1)
                {
                    _repository.AddContact(contact, User.UserID);
                }
                else
                {
                    var existing = _repository.GetContact(contact.ContactId, PortalSettings.PortalId);
                    existing.FirstName = contact.FirstName;
                    existing.LastName = contact.LastName;
                    existing.Email = contact.Email;
                    existing.Phone = contact.Phone;
                    existing.Social = contact.Social;
                    _repository.UpdateContact(existing, User.UserID);
                }

                return RedirectToDefaultRoute();
            }
            else
            {
                return View(contact);
            }
        }

        /// <summary>
        /// The Index method is the default Action method.
        /// </summary>
        /// <param name="searchTerm">Term to search.</param>
        /// <param name="pageIndex">Index of the current page.</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns></returns>
        [ModuleAction(ControlKey = "Edit", TitleKey = "AddContact")]
        public ActionResult Index(string searchTerm = "", int pageIndex = 0)
        {
            pageService.SetTitle("my page title");
            clientResourceController
                                .CreateScript("~/DesktopModules/MVC/Dnn/ContactList/script.js")
                                .Register();
            clientResourceController
                            .CreateStylesheet("~/DesktopModules/MVC/Dnn/ContactList/stylesheet.css")
                            .Register();


            var contacts = _repository.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("PageSize", 6));

            return View(contacts);
        }

        public JsonResult GetJsonResult()
        {
            return new JsonResult()
            {
                Data = new
                {
                    User.UserID,
                    PortalSettings.PortalId,
                    Alias = PortalSettings.PortalAlias.HTTPAlias,
                    Time = DateTime.Now.ToString("HH:mm:ss ttt")
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDemoPartial()
        {
            TempData["UserID"] = User.UserID;
            ViewData["PortalId"] = PortalSettings.PortalId;
            ViewBag.Alias = PortalSettings.PortalAlias.HTTPAlias;

            return PartialView("_DemoPartial", DateTime.Now);
        }
    }
}
