import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';
import stencil from "@stencil/eslint-plugin";
import dnnElements from "@dnncommunity/dnn-elements/eslint-plugin";

export default tseslint.config(
    eslint.configs.recommended,
    tseslint.configs.recommendedTypeChecked,
    stencil.configs.flat.recommended,
    dnnElements.configs.flat.recommended,
    {
        files: ['*.ts', '*.tsx'],
    },
    {
        ignores: [
            '**/node_modules/**',
            '**/dist/**',
            '**/www/**',
            '**/loader/**',
            '**/stencil.config.ts',
            '**/stencil.dnn.config.ts',
            '**/services/services.d.ts',
            '**/services/client-base.ts',
            '**/eslint.config.ts',
            '**/*.d.ts',
            '**/*.js',
        ],
    },
    {
        languageOptions: {
            ecmaVersion: "latest",
            sourceType: "module",
            parserOptions: {
                projectService: {
                    allowDefaultProject: ["*.d.ts"],
                },
                project: 'tsconfig.json',
            },
        },
    },
    {
        rules: {
            "@typescript-eslint/no-unsafe-return": "off",
            "react/jsx-no-bind": "off",
            "@typescript-eslint/no-deprecated": "warn",
        },
    },
);