namespace Dnn.ContactList.SpaReact.Dto
{
    public class ContactDto
    {
        public ContactDto()
        {
        }
        public ContactDto(Dnn.ContactList.Api.Contact contact)
        {
            this.ContactId = contact.ContactId;
            this.PortalId = contact.PortalId;
            this.FirstName = contact.FirstName;
            this.LastName = contact.LastName;
            this.Email = contact.Email;
            this.Phone = contact.Phone;
            this.Social = contact.Social;
        }
        public int ContactId { get; set; }
        public int PortalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Social { get; set; }
    }
}
