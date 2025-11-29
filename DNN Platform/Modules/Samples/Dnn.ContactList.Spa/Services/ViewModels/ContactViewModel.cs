// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Dnn.ContactList.Api;
using Newtonsoft.Json;

namespace Dnn.ContactList.Spa.Services.ViewModels
{
    /// <summary>
    /// ContactViewModel represents a Contact object within the Contact Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContactViewModel
    {
        /// <summary>
        /// Constructs a ContactViewModel
        /// </summary>
        public ContactViewModel()
        {
        }

        /// <summary>
        /// Constructs a ContactViewModel from a Contact object
        /// </summary>
        /// <param name="contact"></param>
        public ContactViewModel(Contact contact)
        {
            ContactId = contact.ContactId;
            Email = contact.Email;
            FirstName = contact.FirstName;
            LastName = contact.LastName;
            Phone = contact.Phone;
            Twitter = contact.Twitter;
        }

        /// <summary>
        /// The Id of the contact
        /// </summary>
        [JsonProperty("contactId")]
        public int ContactId { get; set; }

        /// <summary>
        /// The Email of the contact
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The First Name of the contact
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// The Last Name of the contact
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// The Phone of the contact
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// The Twitter of the contact
        /// </summary>
        [JsonProperty("twitter")]
        public string Twitter { get; set; }
    }
}
