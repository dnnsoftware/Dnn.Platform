using Dnn.ContactList.Razor.Models;
using System.Collections.Generic;

namespace Dnn.ContactList.Razor.Models
{
    public class ContactsModel
    {
        public ContactsModel()
        {
        }

        public bool IsEditable { get; set; }
        public string EditUrl { get; set; }
        public List<ContactModel> Contacts { get; set; }
    }
}