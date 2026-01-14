// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using Dnn.ContactList.Api;
using DotNetNuke.Collections;
using DotNetNuke.Framework;

namespace Dnn.ContactList.Spa.Components
{

    public class ContactService : ServiceLocator<IContactService, ContactService>, IContactService
    {
        private readonly IContactRepository _repository;

        /// <summary>
        /// Default Constructor constructs a new ContactService
        /// </summary>
        public ContactService()
        {
            _repository = ContactRepository.Instance;
        }


        protected override Func<IContactService> GetFactory()
        {
            return () => new ContactService();
        }

        public int AddContact(Contact contact, int userId)
        {
            return _repository.AddContact(contact, userId);
        }

        public void DeleteContact(Contact contact)
        {
            _repository.DeleteContact(contact);
        }

        public Contact GetContact(int contactId, int portalId)
        {
            return _repository.GetContact(contactId, portalId);
        }

        public IQueryable<Contact> GetContacts(int portalId)
        {
            return _repository.GetContacts(portalId);
        }

        public IPagedList<Contact> GetContacts(string searchTerm, int portalId, int pageIndex, int pageSize)
        {
            return _repository.GetContacts(searchTerm, portalId, pageIndex, pageSize);
        }

        public void UpdateContact(Contact contact, int userId)
        {
            _repository.UpdateContact(contact, userId);
        }
    }
}
