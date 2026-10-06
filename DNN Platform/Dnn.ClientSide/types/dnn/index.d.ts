/**
 * @dnn-community/types
 * 
 * TypeScript type definitions for DNN Platform (DotNetNuke)
 * Provides type safety and IntelliSense for DNN JavaScript APIs
 * 
 * @packageDocumentation
 */

/// <reference path="./persona-bar/index.d.ts" />
// Future: /// <reference path="./dnn-core/index.d.ts" />

/**
 * Global augmentation for DNN on the window object.
 * This allows client-side scripts to have type safety when accessing window.dnn.
 */
declare global {
    interface DnnWindow {
        /**
         * Returns a DNN variable by key.
         */
        getVar: (key: string, defaultValue: string) => string;

        /**
         * PersonaBar API - Modern admin interface for DNN Platform.
         * Available when PersonaBar is loaded on the page.
         */
        PersonaBar?: typeof DnnPersonaBar;
    }

    interface Window {
        /**
         * The DNN namespace containing various DNN Platform APIs.
         */
        dnn: DnnWindow;
    }
}

export {};
