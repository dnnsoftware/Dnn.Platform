// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.ContactList.Api;
using Dnn.ContactList.Spa.Components;
using Dnn.ContactList.Spa.Services.ViewModels;
using DotNetNuke.Common;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.ContactList.Spa.Services
{
    /// <summary>
    /// ContentTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.ContactList.Spa")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContactController : DnnApiController
    {
        private readonly IContactService _contactService;

        /// <summary>
        /// Default Constructor constructs a new ContactController
        /// </summary>
        public ContactController() : this(ContactService.Instance)
        {

        }

        /// <summary>
        /// Constructor constructs a new ContactController with a passed in repository
        /// </summary>
        public ContactController(IContactService service)
        {
            Requires.NotNull(service);

            _contactService = service;
        }

        /// <summary>
        /// The DeleteContact method deletes a single contact
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeleteContact(ContactViewModel viewModel)
        {
            var contact = _contactService.GetContact(viewModel.ContactId, PortalSettings.PortalId);

            _contactService.DeleteContact(contact);

            var response = new
            {
                success = true
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// The GetContact method gets a single contact
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContact(int contactId)
        {
            var contact = new ContactViewModel(_contactService.GetContact(contactId, PortalSettings.PortalId));

            var response = new
            {
                success = true,
                data = new
                {
                    contact = contact
                }
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// The GetContacts method gets all the contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContacts(string searchTerm, int pageSize, int pageIndex)
        {
            var contactList = _contactService.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, pageSize);
            var contacts = contactList
                                 .Select(contact => new ContactViewModel(contact))
                                 .ToList();

            var response = new
            {
                success = true,
                data = new
                {
                    results = contacts,
                    totalCount = contactList.TotalCount
                }
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// The SaveContact method persists the Contact to the repository
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SaveContact(ContactViewModel viewModel)
        {
            Contact contact;

            if (viewModel.ContactId == -1)
            {
                contact = new Contact
                {
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Social = viewModel.Social,
                    PortalId = PortalSettings.PortalId
                };
                _contactService.AddContact(contact, UserInfo.UserID);
            }
            else
            {
                //Update
                contact = _contactService.GetContact(viewModel.ContactId, PortalSettings.PortalId);

                if (contact != null)
                {
                    contact.FirstName = viewModel.FirstName;
                    contact.LastName = viewModel.LastName;
                    contact.Email = viewModel.Email;
                    contact.Phone = viewModel.Phone;
                    contact.Social = viewModel.Social;
                }
                _contactService.UpdateContact(contact, UserInfo.UserID);
            }
            var response = new
            {
                success = true,
                data = new
                {
                    contactId = contact.ContactId
                }
            };

            return Request.CreateResponse(response);

        }
    }
}
