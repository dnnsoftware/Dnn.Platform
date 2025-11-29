// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Linq;
using DotNetNuke.Collections;

namespace Dnn.ContactList.Api
{
    /// <summary>
    /// IContactRepository provides an interface for interacting with the Contact Repository(Database)
    /// </summary>
    public interface IContactRepository
    {
        /// <summary>
        /// AddContact adds a contact to the repository
        /// </summary>
        /// <param name="contact">The contact to add</param>
        /// <returns>The Id of the contact</returns>
        int AddContact(Contact contact);

        /// <summary>
        /// DeleteContact deletes a contact from the repository
        /// </summary>
        /// <param name="contact">The contact to delete</param>
        void DeleteContact(Contact contact);

        /// <summary>
        /// This GetContact  method retrieves a specific Contact in a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="contactId">The Id of the contact</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <returns>A single of contact</returns>
        Contact GetContact(int contactId, int portalId);

        /// <summary>
        /// This GetContacts overload retrieves all the Contacts for a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="portalId">The Id of the portal</param>
        /// <returns>A collection of contacts</returns>
        IQueryable<Contact> GetContacts(int portalId);

        /// <summary>
        /// This GetContacts overload retrieves a page of Contacts for a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="pageIndex">The page Index to fetch - this is 0 based so the first page is when pageIndex = 0</param>
        /// <param name="pageSize">The size of the page to fetch from the database</param>
        /// <returns>A paged collection of contacts</returns>
        IPagedList<Contact> GetContacts(string searchTerm, int portalId, int pageIndex, int pageSize);

        /// <summary>
        /// UpdateContact updates a contact in the repository
        /// </summary>
        /// <param name="contact">The contact to update</param>
        void UpdateContact(Contact contact);
    }
}


