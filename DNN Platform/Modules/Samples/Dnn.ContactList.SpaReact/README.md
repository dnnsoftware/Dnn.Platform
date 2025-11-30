# ContactList SpaReact Module

A modern React TypeScript SPA module for DNN Platform that provides contact management functionality.

## Features

- Display contacts in a responsive card grid layout
- Pagination support (6 contacts per page)
- Add, edit, and delete contacts
- Role-based security (edit/delete buttons shown based on user permissions)
- Form validation with real-time feedback
- Modern UI with smooth animations

## Technology Stack

- React 18
- TypeScript 5
- React Router 6
- Vite 5 (build tool)
- DNN ServicesFramework for API authentication

## Project Structure

```
src/
├── main.tsx              # Entry point
├── App.tsx               # Root component with router
├── types/                # TypeScript interfaces
│   ├── Contact.ts
│   ├── Security.ts
│   └── Module.ts
├── services/
│   └── services.ts       # API calls to DNN backend
├── pages/
│   ├── ContactList.tsx   # Main list view
│   └── ContactForm.tsx   # Add/Edit form
├── components/
│   ├── ContactCard.tsx   # Contact card component
│   └── Pagination.tsx    # Pagination controls
└── utils/
    └── validation.ts     # Form validation
```

## Development

### Prerequisites

- Node.js 18+ or Yarn 4.10.3+
- DNN Platform installation with ContactList.Spa module (for API backend)

### Setup

1. Install dependencies:
```bash
yarn install
```

2. Start development server:
```bash
yarn dev
```

### Build

Build the production bundle (minified, no sourcemaps):
```bash
yarn build
```

Watch mode for development (with sourcemaps, unminified):
```bash
yarn watch
```

This generates `scripts/contact-list.js` which is loaded by DNN.

#### Custom Output Path for Development

For watch mode, you can set the `DNN_PATH` environment variable to automatically output to your local DNN installation:

```bash
# Windows PowerShell
$env:DNN_PATH="C:\inetpub\wwwroot\dnndev"
yarn watch

# Windows CMD
set DNN_PATH=C:\inetpub\wwwroot\dnndev
yarn watch

# Linux/Mac
export DNN_PATH=/var/www/dnndev
yarn watch
```

The files will be output to `{DNN_PATH}/DesktopModules/Dnn/ContactListSpaReact/scripts/` during watch mode, or to the local `scripts/` folder for production builds.

## API Integration

The module uses the existing ContactList.Spa Web API endpoints:

- `GET /API/Dnn/ContactList/Contact/GetContacts` - List contacts (paginated)
- `GET /API/Dnn/ContactList/Contact/GetContact` - Get single contact
- `POST /API/Dnn/ContactList/Contact/SaveContact` - Create/update contact
- `POST /API/Dnn/ContactList/Contact/DeleteContact` - Delete contact

Authentication is handled via DNN's ServicesFramework.

## Security Context

The module reads security context from data attributes in the HTML:

- `data-security` - Contains user permissions (CanView, CanEdit, IsAdmin)
- `data-module` - Contains module context (ModuleId, TabId, etc.)

These are provided by the `ContextTokens` class in the C# backend.
