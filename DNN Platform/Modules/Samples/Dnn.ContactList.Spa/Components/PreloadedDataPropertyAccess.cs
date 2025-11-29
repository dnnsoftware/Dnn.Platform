// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Linq;
using Dnn.ContactList.Spa.Services.ViewModels;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;

namespace Dnn.ContactList.Spa.Components
{
    /// <summary>
    /// Implements the Interface IPropertyAccess. This mechanism is used by the Token Replace
    /// Engine implemented to be used in SPA modules. The method 'GetProperty' allows to access to 
    /// information that will be available as properties in the custom token.
    /// </summary>
    public class PreloadedDataPropertyAccess : IPropertyAccess
    {
        private readonly IContactService _service;
        private readonly int _portalId;
        private readonly string searchTerm;
        private readonly int pageIndex;
        private readonly int pageSize;

        /// <summary>
        /// Default Constructor constructs a new PreloadedDataPropertyAccess
        /// </summary>
        public PreloadedDataPropertyAccess(int portalId) : this(portalId, ContactService.Instance)
        {

        }

        /// <summary>
        /// Constructor constructs a new PreloadedDataPropertyAccess with a passed in service
        /// </summary>
        public PreloadedDataPropertyAccess(int portalId, IContactService service)
        {
            Requires.NotNull(service);

            _service = service;
            _portalId = portalId;
            searchTerm = "";
            pageIndex = 0;
            pageSize = 6;
        }

        /// <summary>
        /// Token Cacheability.
        /// </summary>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <summary>
        /// Get Preloaded Data Property.
        /// </summary>
        /// <param name="propertyName">property name.</param>
        /// <param name="format">format.</param>
        /// <param name="formatProvider">format provider.</param>
        /// <param name="accessingUser">accessing user.</param>
        /// <param name="accessLevel">access level.</param>
        /// <param name="propertyNotFound">Whether found the property value.</param>
        /// <returns></returns>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            var contactList = _service.GetContacts(searchTerm, _portalId, pageIndex, pageSize);
            var contacts = contactList
                                 .Select(contact => new ContactViewModel(contact))
                                 .ToList();
            return "{ results: " + JsonConvert.SerializeObject(contacts) + ", pageCount: " + contactList.TotalCount + "}";
        }
    }
}
