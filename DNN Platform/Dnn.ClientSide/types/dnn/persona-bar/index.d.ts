/**
 * DNN PersonaBar TypeScript Definitions
 * 
 * Type definitions for the DNN Platform PersonaBar administration interface.
 * PersonaBar is the modern administrative interface introduced in DNN 9.0+
 * 
 * @packageDocumentation
 */

import type { Dayjs } from "dayjs";

/**
 * Main namespace for DNN PersonaBar types
 */
declare namespace DnnPersonaBar {
    // ==================== Models ====================

    /**
     * Data models used throughout PersonaBar
     */
    export namespace Models {
        /**
         * A menu item in the persona bar.
         */
        export interface MenuItem {
            /** The unique identifier for the menu item. */
            id: string;
            /** The localization key for the menu item. */
            resourceKey: string;
            /** The name of the module to display in the panel, use LinkMenu to do a link instead of a module.*/
            moduleName: string;
            /** Not sure on the usage, only found empty string PR welcome if anyone know the purpose. */
            folderName: string;
            /** Not sure on the purpose, usually same as moduleName. */
            path: string;
            /** An optional querystring to pass to a link (when in link mode).*/
            query: string;
            /** If set, the panel will display a site page instead of a persona bar module. */
            link: string;
            /** Css classes that can be added to the menu item. */
            css: string;
            /** The path to an icon to display in the menu item. */
            icon: string;
            /** A user friendly and localized display name. */
            displayName: string;
            /** A json STRING that can contain settings for the persona bar page or module to use with no specific shape (depends on the module). */
            settings: string;
            /** The child menu items contained within this page. */
            menuItems: MenuItem[];
        }

        /**
         * Represents a menu structure item.
         */
        export interface MenuStructureItem {
            /** The menu identifier. */
            menuId: number;
            /** The unique identifier of this menu item. */
            identifier: string;
            /** The controller type. */
            controller: string;
            /** The resource key. */
            resourceKey: string;
            /** The css class for this menu. */
            cssClass: string;
            /** The parent identifier. */
            parentId: string | null;
            /** The sort order. */
            order: number;
            /** The path for this item. */
            path: string;
            /** The link (for link type menus). */
            link: string;
            /** Whether this is the first level. */
            isFirstLevel: boolean;
        }

        /**
         * Represents the complete menu structure.
         */
        export interface MenuStructure {
            /** The menu identifier. */
            MenuId: number;
            /** The identifier. */
            Identifier: string;
            /** The parent menu identifier. */
            ParentId: string | null;
            /** The module identifier. */
            ModuleId: number | null;
            /** The controller type. */
            Controller: string;
            /** The resource key. */
            ResourceKey: string;
            /** The path. */
            Path: string;
            /** The link. */
            Link: string;
            /** The CSS class. */
            CssClass: string;
            /** The icon file. */
            IconFile: string;
            /** The parent identifier. */
            ParentIdentifier: string | null;
            /** The sort order. */
            Order: number;
            /** Whether this is a disabled item. */
            IsDisabled: boolean;
            /** Whether this menu allows hosts only. */
            AllowHost: boolean;
            /** Whether this item is active. */
            Enabled: boolean;
            /** HTML settings. */
            Settings: Record<string, unknown> | null;
            /** The admin page. */
            AdminPage: string | null;
        }

        /**
         * Settings for a menu item/page - can contain any custom properties the module needs.
         */
        export interface MenuSettings {
            /** Can contain any custom settings that a given persona bar module or page may need. */
            [key: string]: unknown;
        }

        /**
         * Parameters for loading a PersonaBar panel.
         */
        export interface LoadPanelParams {
            /** The unique identifier of the page to load. */
            identifier?: string;
            /** Additional parameters to pass to the panel. */
            [key: string]: unknown;
        }

        /**
         * Options for displaying notifications.
         */
        export interface NotificationOptions {
            /** The notification message. */
            message: string;
            /** The type of notification (error, warning, info, success). */
            type?: "error" | "warning" | "info" | "success";
            /** How long to display the notification in milliseconds (0 = permanent). */
            duration?: number;
        }

        /**
         * Data structure for persistant storage.
         */
        export interface PersistantData {
            /** Can contain any data that needs to be persisted. */
            [key: string]: unknown;
        }
    }

    // ==================== API ====================

    /**
     * PersonaBar APIs and utilities
     */
    export namespace API {
        /**
         * Allow easy access to DNN services framework for API calls.
         */
        export interface ServicesFramework {
            /** The anti-forgery token to use for secure API calls. */
            antiForgeryToken: string;

            /**
             * Performs an API call to a PersonaBar controller method.
             * Based on DNN Platform sf.js implementation.
             */
            call(
                /** The HTTP method to use. */
                httpMethod: "GET" | "POST",
                /** The controller action to call (method of the controller class). */
                method: string,
                /** The data to append to the GET query or to send in a POST. */
                params?: Record<string, unknown> | string | FormData | unknown[],
                /** Fires when the call succeeded. */
                success?: (
                    /** The data returned by the API call. */
                    data: unknown
                ) => void,
                /** Fires when the API call fails. */
                failure?: (
                    /** The XHR object that contains the error details. */
                    xhr: JQueryXHR | null,
                    /** The error message. */
                    message: string
                ) => void,
                /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
                loading?: (loading: boolean) => void,
                /** A callback that can be used to customize the request before it is sent.*/
                beforeSend?: (xhr: JQueryXHR) => void,
                /** If true the request will be sent synchronously (defaults to asynchronous). */
                sync?: boolean,
                /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
                silence?: boolean,
                /** Set to true if the data (params) represents a file to be uploaded. */
                postFile?: boolean
            ): JQuery.jqXHR<unknown>;

            /**
             * Performs an API call to a PersonaBar controller method with typed response.
             * Based on DNN Platform sf.js implementation.
             */
            call<T>(
                /** The HTTP method to use. */
                httpMethod: "GET" | "POST",
                /** The controller action to call (method of the controller class). */
                method: string,
                /** The data to append to the GET query or to send in a POST. */
                params?: Record<string, unknown> | string | FormData | unknown[],
                /** Fires when the call succeeded. */
                success?: (
                    /** The data returned by the API call. */
                    data: T
                ) => void,
                /** Fires when the API call fails. */
                failure?: (
                    /** The XHR object that contains the error details. */
                    xhr: JQueryXHR | null,
                    /** The error message. */
                    message: string
                ) => void,
                /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
                loading?: (loading: boolean) => void,
                /** A callback that can be used to customize the request before it is sent.*/
                beforeSend?: (xhr: JQueryXHR) => void,
                /** If true the request will be sent synchronously (defaults to asynchronous). */
                sync?: boolean,
                /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
                silence?: boolean,
                /** Set to true if the data (params) represents a file to be uploaded. */
                postFile?: boolean
            ): JQuery.jqXHR<T>;

            /**
             * Gets the base URL for the PersonaBar API.
             */
            getBaseUrl(): string;
        }

        /**
         * Allows persisting persona bar user level state.
         */
        export interface Persistant {
            /** Loads the persistant data. */
            load(
                /** Callback that receives the loaded data. */
                callback: (data: Models.PersistantData) => void
            ): void;

            /** Saves the persistant data. */
            save(
                /** The data to save. */
                data: Models.PersistantData,
                /** Optional callback when save completes. */
                callback?: () => void
            ): void;
        }

        /**
         * Utilities available in the persona bar.
         */
        export interface Utilities {
            /** The DNN Services Framework that allows easy interaction with WEB APIs. */
            sf: ServicesFramework;

            /** If true, we are on a device that supports touch. */
            onTouch: boolean;

            /** Provides access to a pre-configured DayJs constructor. */
            dayjs: () => Dayjs;

            /** Allows persisting persona bar user level state. */
            persistant: Persistant;

            /** If true, the persona bar is currently animating. */
            inAnimatin: boolean;

            /** Can be called to center the confirmation dialog. */
            setConfirmationDialogPosition: () => void;

            /** Opens the social tasks (unknown/undocumented feature, probably Evoq only). */
            openSocialTasks: () => void;

            /** Closes the social tasks (unknown/undocumented feature, probably Evoq only). */
            closeSocialTasks: () => void;

            /** Makes the persona bar panel wider. (INOP ?) */
            expandPersonaBarPage: () => void;

            /** Makes the persona bar panel back to the initial size. (INOP ?) */
            contractPersonaBarPage: () => void;

            /** Closes the persona bar. */
            closePersonaBar: (
                /** Gets called once the persona bar is finished closing. */
                callback?: (() => void),
                /** If true, the selection will be kept in focus. */
                keepSelection?: boolean
            ) => void;

            /** Opens a specific persona bar panel (page) */
            loadPanel: (
                /** The unique identifier of the page such as Dnn.Themes . */
                identifier: string,
                /** Parameters to pass to the persona bar module on that page. */
                params: Models.LoadPanelParams,
            ) => void;

            /**
             * Internal method called after a panel has finished loading.
             * Handles extension loading, custom modules, and tab view management.
             */
            panelLoaded: (
                /** The same parameters object that was passed to loadPanel */
                params: Models.LoadPanelParams,
                /** Whether the template was already loaded (true) or newly loaded (false) */
                loaded: boolean,
            ) => void;

            /** Internal method. */
            initCustomModules: (callback: () => void) => void;

            /** Internal method. */
            loadCustomModules: () => void;

            /** Internal method. */
            leaveCustomModules: () => void;

            /** Finds the settings for a given menu identifier parsed into an object (as opposed to just a json string). */
            findMenuSettings: (
                /** The unique identifier of the page such as Dnn.Themes . */
                identifier: string,
                /** The list of menu items. */
                menuItems?: Models.MenuItem[],
            ) => Models.MenuSettings | null;

            /** Saves the settings for a given menu item. */
            updateMenuSettings: (
                /** The unique identifier of the page such as Dnn.Themes . */
                identifier: string,
                /** The settings to save. */
                settings: Models.MenuSettings,
                /** The list of menu items. */
                menuItems?: Models.MenuItem[],
            ) => void;

            /** Loads javascript bundle files using ajax. */
            loadBundleScript: (
                /** The path or paths for the scripts to load. */
                path: string | string[],
                /** Callback when scripts are loaded. */
                callback: () => void
            ) => void;

            /** Loads CSS files. */
            loadCss: (
                /** The path or paths for the CSS files to load. */
                path: string | string[]
            ) => void;

            /** Shows a notification in the persona bar. */
            notify: (
                /** The message to display. */
                message: string,
                /** Optional notification options. */
                options?: Models.NotificationOptions
            ) => void;

            /** Shows an error notification. */
            notifyError: (
                /** The error message. */
                message: string,
                /** How long to display in milliseconds (0 = permanent). */
                duration?: number
            ) => void;

            /** Confirms an action with a dialog. */
            confirm: (
                /** The confirmation message. */
                message: string,
                /** The title of the confirmation dialog. */
                title: string,
                /** Callback when user confirms. */
                onConfirm: () => void,
                /** Callback when user cancels. */
                onCancel?: () => void
            ) => void;

            /** Copies text to the clipboard. */
            copyToClipboard: (
                /** The text to copy. */
                text: string
            ) => boolean;

            /** Trims content to fit within a specified width. */
            trimContentToFit: (
                /** The string to trim. */
                content: string,
                /** The width in pixels to trim to. */
                width: number,
            ) => string;

            /** 
             * Copies an object into another one.
             * Recommend using Object.assign or object propagation instead.
             */
            getObjectCopy: (object: object) => object;

            /** 
             * Throttles the execution of a function to the next available cycle.
             * Same as setTimeout(callback, 0);
             */
            throttleExecution: (callback: () => void) => void;

            /** Just in case a developer forgets what numbers are. */
            ONE_THOUSAND: 1000;

            /** Just in case a developer forgets what numbers are. */
            ONE_MILLION: 1000000;

            /** 
             * Gets a string representing big numbers with an abbreviation.
             * Example: 1000 becomes 1K, 1000000 becomes 1M.
             */
            formatAbbreviateBigNumbers: (number: number) => string;

            /** Gets the current culture from the config. */
            getCulture: () => string;

            /** Gets the current SKU (useful if doing something different for different DNN editions.*/
            getSKU: () => string;

            /** Gets the current locale number separator symbol. */
            getNumbersSeparatorByLocale: () => string;

            /** Formats a number with the proper decimal separator (according to DNN localization). */
            formatCommaSeparate: (number: number) => string;

            /** 
             * Formats an amount of seconds into a string that is more readable for a human.
             * Example: 3600 becomes 1:00:00
             * Example: 3661 becomes 1:01:01
             * Example: 61 becomes 1:01
             * Example: 1 becomes 0:01
             */
            secondsFormatter: (
                /** The amount of seconds. */
                seconds: number
            ) => string;

            /** Gets the application root path */
            getApplicationRootPath: () => string;

            /** Gets the ID of the panel for a given path. */
            getPanelIdFromPath: (
                /** The path. */
                path: string
            ) => string;

            /** 
             * Parses the query string from the Path or path property of the given menu item object
             * and assigns it to a corresponding Query or query property,
             * while updating the original Path or path property to exclude the query string. 
             */
            parseQueryParameter: (
                /** The menu item to parse the query for. */
                item: Models.MenuItem
            ) => void;

            /** Builds the menu viewmodel. */
            buildMenuViewModel: (
                /** The menu structure to build the view model from. */
                menuStructure: Models.MenuStructure
            ) => {menu: {menuItems: Models.MenuItem[]}};

            /** Gets the path defined by the first menu item with a given module name. */
            getPathByModuleName: (
                /** The menu structure to search. */
                menuStructure: Models.MenuStructure,
                /** The name of the module for which to get the path for. */
                moduleName: string,
            ) => string;
        }
    }

    // ==================== Module context contracts ====================

    /**
     * Public contracts for DNN module authors.
     */
    export namespace ModuleContext {
        /** Dictionary of permission keys and values. */
        export interface PermissionsDictionary {
            [key: string]: string;
        }

        /** Common module parameters supplied by DNN wrappers. */
        export interface Params {
            folderName?: string;
            identifier?: string;
            moduleName?: string;
            path?: string;
            query?: string;
            settings?: Settings;
        }

        /** Common settings supplied by DNN wrappers. */
        export interface Settings {
            isAdmin?: boolean;
            isHost?: boolean;
            permissions?: PermissionsDictionary;
        }

        /** Common notification options for DNN client-side wrappers. */
        export interface NotifyOptions {
            timeout?: number;
            clickToClose?: boolean;
            closeButtonText?: string;
            type?: "notify" | "error";
        }

        /** Shared service framework shape used by wrapper utilities. */
        export interface ServicesFramework {
            antiForgeryToken: string;
            getServiceRoot(): string;
            moduleRoot: string;
            controller: string;
        }

        /** Shared utility shape for wrapper modules. */
        export interface Utility<TResx = unknown> {
            resx: TResx;
            sf: ServicesFramework;
            notify: (message: string, options?: NotifyOptions) => void;
            notifyError: (message: string, options?: NotifyOptions) => void;
        }

        /** Generic init config for module entrypoints. */
        export interface InitConfig<TResx, TParams = Params> {
            moduleName: string;
            params: TParams;
            utility: Utility<TResx>;
        }
    }

    // ==================== Validation ====================

    /**
     * Validation utilities
     */
    export namespace Validation {
        /**
         * A custom field validator.
         */
        export interface CustomValidator {
            /** The name of the custom validator. */
            name: string;

            /** Validates an input element. */
            validate(
                /** The value of the input element. */
                value: unknown,
                /** The input element to validate. */
                input: HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement,
            ): boolean;
        }

        /**
         * Utility to validate inputs.
         */
        export interface Validator {
            /** The HTML elements that can be validated. */
            selector: "input, textarea, select";

            /** The list of standard error messages. */
            errorMessages: {
                required: "Text is required",
                minLength: "Text must be at least {0} chars",
                number:  "Only numbers are allowed",
                nonNegativeNumber: "Negative numbers are not allowed",
                positiveNumber: "Only positive numbers are allowed",
                nonDecimalNumber: "Decimal numbers are not allowed",
                email: "Only valid email is allowed",
            };

            /** Performs field validation and reports the errors. */
            validate: (
                /** The parent HTML element containing the fields to validate. */
                container: HTMLElement | JQuery<HTMLElement>,
                /** Optional custom validators to apply. */
                customValidators?: CustomValidator[],
            ) => boolean;
        }
    }
}

export = DnnPersonaBar;
