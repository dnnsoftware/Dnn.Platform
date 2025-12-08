# ContactList SpaReact Module

A modern React TypeScript SPA module for DNN Platform that provides contact management functionality.

## Features

- Display contacts in a responsive card grid layout
- Pagination support (6 contacts per page)
- Search support
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

Watch mode for development (with sourcemaps, unminified):
```bash
yarn run watch --scope dnn.contactlist.spareact
```

