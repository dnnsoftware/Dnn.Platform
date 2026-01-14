import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Contact } from '../types/Contact';
import { ModuleContext } from '../types/Module';
import { getContact, saveContact } from '../services/services';
import { validateRequired, validateEmail, validatePhone, validateSocial } from '../utils/validation';

interface ContactFormProps {
  moduleContext: ModuleContext;
}

interface FormErrors {
  FirstName: string;
  LastName: string;
  Email: string;
  Phone: string;
  Social: string;
}

export default function ContactForm({ moduleContext }: ContactFormProps) {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEditMode = id !== undefined;

  const [contact, setContact] = useState<Contact>({
    ContactId: -1,
    FirstName: '',
    LastName: '',
    Email: '',
    Phone: '',
    Social: '',
    CreatedByUserId: 0,
    CreatedOnDate: new Date(),
    LastModifiedByUserId: 0,
    LastModifiedOnDate: new Date()
  });

  const [errors, setErrors] = useState<FormErrors>({
    FirstName: '',
    LastName: '',
    Email: '',
    Phone: '',
    Social: ''
  });

  const [loading, setLoading] = useState(false);
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  useEffect(() => {
    if (isEditMode && id) {
      loadContact(parseInt(id));
    }
  }, [id]);

  const loadContact = async (contactId: number) => {
    try {
      setLoading(true);
      const response = await getContact(moduleContext, contactId);
      setContact(response);
    } catch (err) {
      console.error('Error loading contact:', err);
      alert('Failed to load contact');
      navigate('/');
    } finally {
      setLoading(false);
    }
  };

  const validateField = (name: string, value: string): string => {
    switch (name) {
      case 'FirstName':
        return validateRequired(value, 'First name').message;
      case 'LastName':
        return validateRequired(value, 'Last name').message;
      case 'Email':
        return validateEmail(value).message;
      case 'Phone':
        return validatePhone(value).message;
      case 'Social':
        return validateSocial(value).message;
      default:
        return '';
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setContact({ ...contact, [name]: value });

    // Validate on change if field has been touched
    if (touched[name]) {
      setErrors({ ...errors, [name]: validateField(name, value) });
    }
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setTouched({ ...touched, [name]: true });
    setErrors({ ...errors, [name]: validateField(name, value) });
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {
      FirstName: validateField('FirstName', contact.FirstName),
      LastName: validateField('LastName', contact.LastName),
      Email: validateField('Email', contact.Email),
      Phone: validateField('Phone', contact.Phone),
      Social: validateField('Social', contact.Social)
    };

    setErrors(newErrors);
    setTouched({
      FirstName: true,
      LastName: true,
      Email: true,
      Phone: true,
      Social: true
    });

    return !Object.values(newErrors).some(error => error !== '');
  };

  const handleSave = async () => {
    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);
      const response = await saveContact(moduleContext, contact);

      setContact(response);
      navigate('/');
    } catch (err) {
      console.error('Error saving contact:', err);
      alert('Failed to save contact');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/');
  };

  return (
    <div className="contactList-container">
      <div className="editContact">
        <div className="editContact-header">
          <h3 className="editContact-title">
            <i className="fa fa-user-plus"></i>
            <span>{isEditMode ? 'Edit Contact' : 'Add Contact'}</span>
          </h3>
        </div>
        <div className="editContact-body">
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">
                <i className="fa fa-user"></i>
                First Name
              </label>
              <input
                className={`form-control ${errors.FirstName && touched.FirstName ? 'has-error' : ''}`}
                name="FirstName"
                value={contact.FirstName}
                onChange={handleChange}
                onBlur={handleBlur}
                placeholder="First Name"
                disabled={loading}
              />
              {errors.FirstName && touched.FirstName && (
                <span className="form-error">{errors.FirstName}</span>
              )}
            </div>
            <div className="form-group">
              <label className="form-label">
                <i className="fa fa-user"></i>
                Last Name
              </label>
              <input
                className={`form-control ${errors.LastName && touched.LastName ? 'has-error' : ''}`}
                name="LastName"
                value={contact.LastName}
                onChange={handleChange}
                onBlur={handleBlur}
                placeholder="Last Name"
                disabled={loading}
              />
              {errors.LastName && touched.LastName && (
                <span className="form-error">{errors.LastName}</span>
              )}
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">
              <i className="fa fa-envelope"></i>
              Email
            </label>
            <input
              className={`form-control ${errors.Email && touched.Email ? 'has-error' : ''}`}
              name="Email"
              value={contact.Email}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Email"
              disabled={loading}
            />
            {errors.Email && touched.Email && (
              <span className="form-error">{errors.Email}</span>
            )}
          </div>
          <div className="form-group">
            <label className="form-label">
              <i className="fa fa-phone"></i>
              Phone
            </label>
            <input
              className={`form-control ${errors.Phone && touched.Phone ? 'has-error' : ''}`}
              name="Phone"
              value={contact.Phone}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Phone"
              disabled={loading}
            />
            {errors.Phone && touched.Phone && (
              <span className="form-error">{errors.Phone}</span>
            )}
          </div>
          <div className="form-group">
            <label className="form-label">
              <i className="fa fa-twitter"></i>
              Social
            </label>
            <input
              className={`form-control ${errors.Social && touched.Social ? 'has-error' : ''}`}
              name="Social"
              value={contact.Social}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="@username"
              disabled={loading}
            />
            {errors.Social && touched.Social && (
              <span className="form-error">{errors.Social}</span>
            )}
          </div>
        </div>
        <div className="editContact-footer">
          <a className="btn-cancel" onClick={handleCancel}>
            <i className="fa fa-times"></i>
            Cancel
          </a>
          <a className="btn-save" onClick={handleSave} style={{ cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.6 : 1 }}>
            <i className="fa fa-check"></i>
            {loading ? 'Saving...' : 'Save'}
          </a>
        </div>
      </div>
    </div>
  );
}

