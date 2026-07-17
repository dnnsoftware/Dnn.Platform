// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.UI.WebControls;
using Dnn.ContactList.Api;
using Dnn.ContactList.Razor.Models;
using DotNetNuke.Common;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.ContactList.Razor.Services
{
    /// <summary>
    /// ContentTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.ContactList.Razor")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContactController : DnnApiController
    {
        private readonly IContactRepository _repository;

        /// <summary>
        /// Default Constructor constructs a new ContactController
        /// </summary>
        public ContactController() : this(ContactRepository.Instance)
        {

        }

        /// <summary>
        /// Constructor constructs a new ContactController with a passed in repository
        /// </summary>
        public ContactController(IContactRepository service)
        {
            Requires.NotNull(service);

            _repository = service;
        }

        /// <summary>
        /// The DeleteContact method deletes a single contact
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage DeleteContact(ContactModel viewModel)
        {
            var contact = _repository.GetContact(viewModel.ContactId, PortalSettings.PortalId);

            _repository.DeleteContact(contact);

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
            var contact = _repository.GetContact(contactId, PortalSettings.PortalId);

            return Request.CreateResponse(new ContactModel()
            {
                ContactId = contact.ContactId,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone,
                Social = contact.Social
            });
        }

        /// <summary>
        /// The GetContacts method gets all the contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContacts(string searchTerm, int pageSize, int pageIndex)
        {
            var contactList = _repository.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, pageSize);
            var contacts = contactList
                                 .Select(contact => new ContactModel()
                                 {
                                     ContactId = contact.ContactId,
                                     FirstName = contact.FirstName,
                                     LastName = contact.LastName,
                                     Email = contact.Email,
                                     Phone = contact.Phone,
                                     Social = contact.Social
                                 })
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage SaveContact(ContactModel viewModel)
        {
            Contact contact;

            if (viewModel.ContactId <= 0)
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
                _repository.AddContact(contact, UserInfo.UserID);
            }
            else
            {
                //Update
                contact = _repository.GetContact(viewModel.ContactId, PortalSettings.PortalId);

                if (contact != null)
                {
                    contact.FirstName = viewModel.FirstName;
                    contact.LastName = viewModel.LastName;
                    contact.Email = viewModel.Email;
                    contact.Phone = viewModel.Phone;
                    contact.Social = viewModel.Social;
                }
                _repository.UpdateContact(contact, UserInfo.UserID);
            }
            var response = new
            {
                success = true,
                data = new
                {
                    contactId = contact.ContactId,
                }
            };

            return Request.CreateResponse(response);

        }
    }
}
