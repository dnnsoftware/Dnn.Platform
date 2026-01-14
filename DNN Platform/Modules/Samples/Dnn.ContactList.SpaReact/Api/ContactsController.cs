using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.ContactList.Api;
using Dnn.ContactList.SpaReact.Dto;
using DotNetNuke.Collections;
using DotNetNuke.Web.Api;

namespace Dnn.ContactList.SpaReact.Api
{
    public class ContactsController : DnnApiController
    {
        private readonly IContactRepository contactRepository;

        public ContactsController(IContactRepository contactRepository)
        {
            this.contactRepository = contactRepository;
        }

        /// <summary>
        /// The Page method gets a page list of contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
        public HttpResponseMessage Page(string searchTerm, int pageSize, int pageIndex)
        {
            var contactList = contactRepository.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, pageSize);
            return Request.CreateResponse(HttpStatusCode.OK, contactList.Serialize(x => new ContactDto(x)));
        }

        /// <summary>
        /// The Contact method gets a single contact
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
        public HttpResponseMessage Contact(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ContactDto(contactRepository.GetContact(id, PortalSettings.PortalId)));
        }

        /// <summary>
        /// The Update method persists the Contact to the repository
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.Edit)]
        public HttpResponseMessage Contact(int id, ContactDto viewModel)
        {
            Contact contact;

            if (id == -1)
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
                contact.ContactId = this.contactRepository.AddContact(contact, UserInfo.UserID);
            }
            else
            {
                //Update
                contact = this.contactRepository.GetContact(id, PortalSettings.PortalId);

                if (contact != null)
                {
                    contact.FirstName = viewModel.FirstName;
                    contact.LastName = viewModel.LastName;
                    contact.Email = viewModel.Email;
                    contact.Phone = viewModel.Phone;
                    contact.Social = viewModel.Social;
                }
                this.contactRepository.UpdateContact(contact, UserInfo.UserID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new ContactDto(contactRepository.GetContact(contact.ContactId, PortalSettings.PortalId)));
        }

        /// <summary>
        /// The Delete method deletes a single contact
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.Edit)]
        public HttpResponseMessage Delete(int id)
        {
            var contact = this.contactRepository.GetContact(id, PortalSettings.PortalId);
            this.contactRepository.DeleteContact(contact);

            var response = new
            {
                success = true
            };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
