# @dnncommunity/dnn-types

TypeScript type definitions for DNN Platform (DotNetNuke) - providing type safety and IntelliSense for DNN JavaScript APIs.

## Overview

This package provides comprehensive TypeScript definitions for:

- **PersonaBar API** - Modern administrative interface for DNN Platform (Currently Available)
- **DNN Core APIs** - Core DNN JavaScript utilities and plugins

## Installation

```bash
npm install --save-dev @dnncommunity/dnn-types
```

Or with yarn:

```bash
yarn add --dev @dnncommunity/dnn-types
```

## Usage

### Method 1: Triple-Slash Reference (Recommended for Legacy Projects)

If you're working with existing DNN modules that use global window objects, add this to the top of your TypeScript files:

```typescript
/// <reference types="@dnncommunity/dnn-types" />

// Now you have full type safety for PersonaBar APIs
if (window.dnn?.PersonaBar) {
    const utils = window.dnn.PersonaBar.API.Utilities;
    utils.notify("Hello from TypeScript with types!", {
        type: "success",
        duration: 3000
    });
}
```

### Method 2: Modern Import (Recommended for New Projects)

For modern TypeScript projects, you can import the types directly:

```typescript
import type { DnnPersonaBar } from '@dnncommunity/dnn-types/persona-bar';

// Type-safe access to PersonaBar utilities
declare const dnn: {
    PersonaBar?: typeof DnnPersonaBar;
};

// Use with full IntelliSense
const sf = dnn.PersonaBar?.API.ServicesFramework;
if (sf) {
    sf.call<MyResponseType>("GET", "MyController/MyMethod", 
        { id: 123 },
        (data) => {
            // 'data' is typed as MyResponseType
            console.log(data);
        }
    );
}
```

### Method 3: tsconfig.json Configuration (Project-wide)

Add to your `tsconfig.json` to include types across your entire project:

```json
{
  "compilerOptions": {
    "types": ["@dnncommunity/dnn-types"]
  }
}
```

## PersonaBar API Reference

The PersonaBar API is organized into three main namespaces:

### DnnPersonaBar.Models

Data models and interfaces.

### DnnPersonaBar.API

Core PersonaBar APIs.

#### ServicesFramework

Make API calls to PersonaBar controllers.

```typescript
const sf = window.dnn?.PersonaBar?.API.ServicesFramework;

// GET request with typed response
sf?.call<User[]>("GET", "Users/GetUsers", 
    { pageSize: 10, pageIndex: 0 },
    (users) => {
        users.forEach(user => console.log(user.DisplayName));
    },
    (xhr, error) => {
        console.error("API call failed:", error);
    }
);

// POST request
sf?.call("POST", "Settings/UpdateSetting",
    { key: "SiteName", value: "My Site" },
    () => console.log("Setting updated!")
);
```

#### Utilities

Helpful utility functions:

```typescript
const utils = window.dnn?.PersonaBar?.API.Utilities;

// Show notifications
utils?.notify("Operation completed successfully!", {
    type: "success",
    duration: 3000
});

utils?.notifyError("Something went wrong!");

// Confirmation dialogs
utils?.confirm(
    "Are you sure you want to delete this?",
    "Confirm Delete",
    () => {
        // User confirmed
        console.log("Deleting...");
    },
    () => {
        // User cancelled
        console.log("Cancelled");
    }
);

// Format numbers
const formatted = utils?.formatCommaSeparate(1234567); // "1,234,567"
const abbreviated = utils?.formatAbbreviateBigNumbers(1500000); // "1.5M"

// Copy to clipboard
const copied = utils?.copyToClipboard("Text to copy");

// Load panels programmatically
utils?.loadPanel("Dnn.Themes", { mode: "edit" });

// Close PersonaBar
utils?.closePersonaBar(() => {
    console.log("PersonaBar closed");
});
```

#### Persistant Storage

Store user-level state:

```typescript
const persistant = window.dnn?.PersonaBar?.API.Utilities?.persistant;

// Save data
persistant?.save({ lastView: "themes", collapsed: false });

// Load data
persistant?.load((data) => {
    console.log("Last view was:", data.lastView);
});
```

### DnnPersonaBar.Validation

Form validation utilities:

```typescript
import type { DnnPersonaBar } from '@dnncommunity/dnn-types/persona-bar';

const validator = window.dnn?.PersonaBar?.Validation.Validator;

// Validate a form
const isValid = validator?.validate(document.getElementById("myForm"));

// Custom validator
const customValidators: DnnPersonaBar.Validation.CustomValidator[] = [
    {
        name: "uniqueUsername",
        validate: (value, input) => {
            // Check if username is unique
            return !existingUsernames.includes(value as string);
        }
    }
];

const isValid = validator?.validate(formElement, customValidators);
```

## Example: Complete PersonaBar Module

```typescript
/// <reference types="@dnncommunity/dnn-types" />

interface MyModuleSettings {
    apiEndpoint: string;
    itemsPerPage: number;
}

class MyPersonaBarModule {
    private sf = window.dnn?.PersonaBar?.API.ServicesFramework;
    private utils = window.dnn?.PersonaBar?.API.Utilities;

    init() {
        // Load module settings
        const settings = this.utils?.findMenuSettings("Dnn.MyModule") as MyModuleSettings;

        if (settings) {
            this.loadData(settings.itemsPerPage);
        }
    }

    loadData(pageSize: number) {
        this.sf?.call<DataItem[]>(
            "GET",
            "MyModule/GetItems",
            { pageSize },
            (items) => {
                this.renderItems(items);
                this.utils?.notify("Data loaded successfully!", {
                    type: "success",
                    duration: 2000
                });
            },
            (xhr, error) => {
                this.utils?.notifyError(`Failed to load data: ${error}`);
            }
        );
    }

    renderItems(items: DataItem[]) {
        // Render your items
    }
}

interface DataItem {
    id: number;
    name: string;
}
```

## Requirements

- TypeScript 4.0 or higher
- jQuery types (`@types/jquery`) - for jQuery-dependent APIs
- dayjs - for date utilities

These are specified as peer dependencies and should be installed in your project.

## Contributing

This is an open-source community project. Contributions are welcome!

### Adding New Type Definitions

To add definitions for additional DNN APIs:

1. Create a new directory under the package root (e.g., `dnn-core/`)
2. Add your type definitions following the namespace pattern
3. Reference them in the main `index.d.ts`
4. Update this README with usage examples

### Testing Your Changes

```bash
# Install dependencies
npm install

# TypeScript will validate the definitions
npx tsc --noEmit
```

## Roadmap

- [x] PersonaBar API type definitions
- [ ] DNN Core ServicesFramework types
- [ ] jQuery plugin extensions (dnnModal, dnnTabs, dnnPanels)
- [ ] DNN Module Settings types
- [ ] DNN Client Resource Manager types
- [ ] Legacy DNN form controls types
