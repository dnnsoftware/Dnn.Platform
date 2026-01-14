import { Contact } from '../types/Contact';

interface ContactCardProps {
  contact: Contact;
  canEdit: boolean;
  onEdit: (contact: Contact) => void;
  onDelete: (contact: Contact) => void;
}

export default function ContactCard({ contact, canEdit, onEdit, onDelete }: ContactCardProps) {
  const handleEdit = () => {
    onEdit(contact);
  };

  const handleDelete = () => {
    if (window.confirm(`Are you sure you want to delete ${contact.FirstName} ${contact.LastName}?`)) {
      onDelete(contact);
    }
  };

  return (
    <div className="contactCard">
      <div className="contactCard-header">
        <div className="contactCard-logo">
          <svg viewBox="0 0 100 60" xmlns="http://www.w3.org/2000/svg">
            {/* Left chevron/mountain */}
            <polyline
              points="5,55 20,15 35,55"
              fill="none"
              stroke="white"
              strokeWidth="6"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
            {/* Middle chevron/mountain */}
            <polyline
              points="30,55 50,5 70,55"
              fill="none"
              stroke="white"
              strokeWidth="6"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
            {/* Right chevron/mountain (partial) */}
            <polyline
              points="65,55 80,15 95,55"
              fill="none"
              stroke="white"
              strokeWidth="5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </div>
        <div className="contactCard-company">
          <div className="company-name">ACME CORP</div>
          <div className="company-tagline">INNOVATION &amp; SOLUTIONS</div>
        </div>
        {canEdit && (
          <div className="contactCard-actions">
            <a title="Edit" onClick={handleEdit}>
              <i className="fa fa-pencil"></i>
            </a>
            <a title="Delete" onClick={handleDelete}>
              <i className="fa fa-trash"></i>
            </a>
          </div>
        )}
      </div>
      <div className="contactCard-body">
        <div className="contact-name">
          <span>{contact.FirstName}</span> <span>{contact.LastName}</span>
        </div>
        <div className="contact-title">Contact Person</div>
        <div className="contact-detail">
          <i className="fa fa-envelope"></i>
          <span>{contact.Email}</span>
        </div>
        <div className="contact-detail">
          <i className="fa fa-phone"></i>
          <span>{contact.Phone}</span>
        </div>
        {contact.Social && (
          <div className="contact-detail contact-social">
            <i className="fa fa-twitter"></i>
            <a
              href={`https://social.com/${contact.Social.replace('@', '')}`}
              target="_blank"
              rel="noopener noreferrer"
            >
              <span>{contact.Social}</span>
            </a>
          </div>
        )}
      </div>
    </div>
  );
}

