import { Contact } from '../types/Contact';
import { ModuleContext } from '../types/Module';
import { PagedList } from '../types/PagedList';

// DNN ServicesFramework interface
interface ServicesFramework {
  getServiceRoot(moduleName: string): string;
  getAntiForgeryValue(): string;
}

declare global {
  interface Window {
    $: any;
  }
}

// Get DNN Services Framework
function getServicesFramework(moduleId: number): ServicesFramework {
  if (window.$ && window.$.ServicesFramework) {
    return window.$.ServicesFramework(moduleId);
  }
  throw new Error('DNN ServicesFramework not available');
}

// Build API headers with anti-forgery token
function getHeaders(moduleContext: ModuleContext): HeadersInit {
  const sf = getServicesFramework(moduleContext.ModuleId);
  return {
    'Content-Type': 'application/json',
    'ModuleId': moduleContext.ModuleId.toString(),
    'TabId': moduleContext.TabId.toString(),
    'RequestVerificationToken': sf.getAntiForgeryValue()
  };
}

// Get base API URL
function getApiUrl(moduleContext: ModuleContext): string {
  const sf = getServicesFramework(moduleContext.ModuleId);
  // The API is from ContactList.Spa module
  return sf.getServiceRoot('Dnn/ContactListSpaReact') + 'Contacts/';
}

export interface GetContactsResponse extends PagedList<Contact> {}

export interface DeleteContactResponse {
  success: boolean;
}

// Get paginated contacts
export async function getContacts(
  moduleContext: ModuleContext,
  pageIndex: number,
  pageSize: number,
  searchTerm: string = ''
): Promise<GetContactsResponse> {
  const url = `${getApiUrl(moduleContext)}Page?searchTerm=${encodeURIComponent(searchTerm)}&pageSize=${pageSize}&pageIndex=${pageIndex}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: getHeaders(moduleContext)
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
}

// Get single contact
export async function getContact(
  moduleContext: ModuleContext,
  contactId: number
): Promise<Contact> {
  const url = `${getApiUrl(moduleContext)}Contact/${contactId}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: getHeaders(moduleContext)
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
}

// Save contact (create or update)
export async function saveContact(
  moduleContext: ModuleContext,
  contact: Contact
): Promise<Contact> {
  const url = `${getApiUrl(moduleContext)}Contact/${contact.ContactId}`;

  const response = await fetch(url, {
    method: 'POST',
    headers: getHeaders(moduleContext),
    body: JSON.stringify(contact)
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
}

// Delete contact
export async function deleteContact(
  moduleContext: ModuleContext,
  contact: Contact
): Promise<DeleteContactResponse> {
  const url = `${getApiUrl(moduleContext)}Delete/${contact.ContactId}`;

  const response = await fetch(url, {
    method: 'POST',
    headers: getHeaders(moduleContext),
    body: JSON.stringify(contact)
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
}

