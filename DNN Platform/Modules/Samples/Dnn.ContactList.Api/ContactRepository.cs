// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace Dnn.ContactList.Api
{
    /// <summary>
    /// ContactRepository provides a concrete implemetation of the IContactRepository interface for interacting with the Contact Repository(Database)
    /// </summary>
    public class ContactRepository : ServiceLocator<IContactRepository, ContactRepository>, IContactRepository
    {
        protected override Func<IContactRepository> GetFactory()
        {
            return () => new ContactRepository();
        }

        /// <summary>
        /// AddContact adds a contact to the repository
        /// </summary>
        /// <param name="contact">The contact to add</param>
        /// <returns>The Id of the contact</returns>
        public int AddContact(Contact contact)
        {
            Requires.NotNull(contact);
            Requires.PropertyNotNegative(contact, "PortalId");

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();

                rep.Insert(contact);
            }

            return contact.ContactId;
        }

        /// <summary>
        /// DeleteContact deletes a contact from the repository
        /// </summary>
        /// <param name="contact">The contact to delete</param>
        public void DeleteContact(Contact contact)
        {
            Requires.NotNull(contact);
            Requires.PropertyNotNegative(contact, "ContactId");

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();

                rep.Delete(contact);
            }
        }

        public Contact GetContact(int contactId, int portalId)
        {
            Requires.NotNegative("contactId", contactId);
            Requires.NotNegative("portalId", portalId);

            return GetContacts(portalId).SingleOrDefault(c => c.ContactId == contactId);
        }

        /// <summary>
        /// This GetContacts overload retrieves all the Contacts for a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="portalId">The Id of the portal</param>
        /// <returns>A collection of contacts</returns>
        public IQueryable<Contact> GetContacts(int portalId)
        {
            Requires.NotNegative("portalId", portalId);

            IQueryable<Contact> contacts = null;

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();

                contacts = rep.Get(portalId).AsQueryable();
            }

            return contacts;
        }

        /// <summary>
        /// This GetContacts overload retrieves a page of Contacts for a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="pageIndex">The page Index to fetch - this is 0 based so the first page is when pageIndex = 0</param>
        /// <param name="pageSize">The size of the page to fetch from the database</param>
        /// <returns>A paged collection of contacts</returns>

        public IPagedList<Contact> GetContacts(string searchTerm, int portalId, int pageIndex, int pageSize)
        {
            Requires.NotNegative("portalId", portalId);

            if (string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = "";
            }
            var contacts = GetContacts(portalId).Where(c => c.FirstName.Contains(searchTerm)
                                                                || c.LastName.Contains(searchTerm) ||
                                                                c.Email.Contains(searchTerm));


            return new PagedList<Contact>(contacts, pageIndex, pageSize);
        }

        /// <summary>
        /// UpdateContact updates a contact in the repository
        /// </summary>
        /// <param name="contact">The contact to update</param>
        public void UpdateContact(Contact contact)
        {
            Requires.NotNull(contact);
            Requires.PropertyNotNegative(contact, "ContactId");

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();

                rep.Update(contact);
            }
        }
    }
}
