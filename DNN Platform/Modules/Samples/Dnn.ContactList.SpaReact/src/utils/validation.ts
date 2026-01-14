export interface ValidationResult {
  isValid: boolean;
  message: string;
}

export function validateRequired(value: string, fieldName: string): ValidationResult {
  if (!value || value.trim() === '') {
    return { isValid: false, message: `${fieldName} is required` };
  }
  return { isValid: true, message: '' };
}

export function validateEmail(email: string): ValidationResult {
  if (!email || email.trim() === '') {
    return { isValid: false, message: 'Please enter a valid email address' };
  }

  // Email regex from the ContactList.Spa module
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(email)) {
    return { isValid: false, message: 'Please enter a valid email address' };
  }

  return { isValid: true, message: '' };
}

export function validatePhone(phone: string): ValidationResult {
  if (!phone || phone.trim() === '') {
    return { isValid: false, message: 'Please enter a valid phone number (international formats accepted: +1 234 567 8900, 123-456-7890, etc.)' };
  }

  // Phone regex from the ContactList.Spa module
  const phoneRegex = /^(\+?\d{1,3}[\s.-]?)?[\d\s().-]{6,}$/;

  if (!phoneRegex.test(phone)) {
    return { isValid: false, message: 'Please enter a valid phone number (international formats accepted: +1 234 567 8900, 123-456-7890, etc.)' };
  }

  return { isValid: true, message: '' };
}

export function validateSocial(social: string): ValidationResult {
  // Social is optional
  if (!social || social.trim() === '') {
    return { isValid: true, message: '' };
  }

  // Social must start with @
  if (!social.startsWith('@')) {
    return { isValid: false, message: 'Social handle must start with @ symbol' };
  }

  return { isValid: true, message: '' };
}

