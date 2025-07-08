import stencil from "@stencil/eslint-plugin";
import tseslint from "typescript-eslint";
import eslint from "@eslint/js";
import dnnElements from "@dnncommunity/dnn-elements/eslint-plugin";

export default (tseslint.config(
    eslint.configs.recommended,
    tseslint.configs.recommendedTypeChecked,
    stencil.configs.flat.recommended,
    dnnElements.configs.flat.recommended,
    {
        files: [
            "src/**/*.{ts,tsx}",
        ]
    },
    {
        files: ['stencil.config.ts'],
        languageOptions: {
            parser: undefined,
        },
    },
    {
        ignores: [
            "**/node_modules/*",
            "**/dist",
            "**/loader",
            "**/www",
            "**/*.js",
            "**/*.config.ts",
        ],
    },
    {
        languageOptions: {
            ecmaVersion: "latest",
            sourceType: "module",
            parserOptions: {
                projectService: true,
                project: 'tsconfig.json',
                tsconfigRootDir: import.meta.dirname,
            },
        },
    },
    {
        rules: {
            "react/jsx-no-bind": "off",
            "@typescript-eslint/no-deprecated": "warn",
            "@typescript-eslint/prefer-promise-reject-errors": "off",
            "@typescript-eslint/no-unsafe-return": "off"
        },
    }
));