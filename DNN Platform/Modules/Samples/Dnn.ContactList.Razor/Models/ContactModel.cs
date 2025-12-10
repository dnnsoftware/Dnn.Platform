namespace Dnn.ContactList.Razor.Models
{
    public class ContactModel
    {
        public ContactModel()
        {
        }

        public int ContactId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Social { get; set; }
        public bool IsEditable { get; set; }
        public string EditUrl { get; internal set; }
        public object ReturnUrl { get; internal set; }
    }
}