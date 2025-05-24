// @ts-check
import eslintPluginReact from "eslint-plugin-react";
import js from "@eslint/js";
import globals from "globals";

/** @type {import("eslint").Linter.Config[]} */
const config = [
    js.configs.recommended,
    eslintPluginReact.configs.flat.recommended,
    {
        files: ["**/*.js", "**/*.jsx"],
        languageOptions: {
            parserOptions: {
                ecmaFeatures: {
                    jsx: true,
                    arrowFunctions: true,
                    blockBindings: true,
                    classes: true,
                    defaultParams: true,
                    destructuring: true,
                    forOf: true,
                    generators: true,
                    modules: true,
                    objectLiteralComputedProperties: true,
                    regexUFlag: true,
                    regexYFlag: true,
                    spread: true,
                    superInFunctions: false,
                    templateStrings: true
                },
                ecmaVersion: 2018,
                sourceType: "module",
            }, 
            globals: {
                __: false,
                Promise: false,
                VERSION: false,
                process: false,
                ...globals.browser,
                ...globals.jest,
            }
        }
    },
    {
        ignores: [
            "dist/",
            "node_modules/",
        ]
    },
    {
        files: ["src/**/*.js", "src/**/*.jsx"],
        languageOptions: {
            globals: {
                document: "readonly",
                window: "readonly",
                navigator: "readonly",
                setTimeout: "readonly",
            }
        }
    },
    {
        files: ["webpack.config.js"],
        languageOptions: {
            sourceType: "commonjs",
        }
    },
    {
        settings: {
            react: {
                version: "detect"
            }
        }
    },
    {
        rules: {
            "semi": "error",
            "no-var": "error",
            "quotes": ["warn", "double" ],
            "indent": ["warn", 4, {"SwitchCase": 1}],
            "no-unused-vars": "warn",
            "no-console": "warn",      
            "keyword-spacing": "warn", 
            "eqeqeq": "warn",
            "space-before-function-paren": ["warn", { "anonymous": "always", "named": "never" }],
            "space-before-blocks": "warn",
            "no-multiple-empty-lines":  "warn",
            "react/jsx-equals-spacing": ["warn", "never"],
            "react/prop-types": "warn",
            "id-match": ["error", "^([A-Za-z0-9_])+$", {"properties": true}],
            "no-useless-escape": "off",
        },
    }
];

export default config;
