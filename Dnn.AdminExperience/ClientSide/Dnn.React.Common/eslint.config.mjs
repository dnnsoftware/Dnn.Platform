import { defineConfig } from "eslint/config";
import react from "eslint-plugin-react";
import _import from "eslint-plugin-import";
import { fixupPluginRules } from "@eslint/compat";
import babelParser from "@babel/eslint-parser";

export default defineConfig([
  {
    ignores: ["dist/**", "node_modules/**"],
  },
  {
    files: ["**/*.js", "**/*.jsx"],
    plugins: {
      react,
      import: fixupPluginRules(_import),
    },
    languageOptions: {
      parser: babelParser,
      ecmaVersion: 2020,
      sourceType: "module",
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
          experimentalObjectRestSpread: true,
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
          templateStrings: true,
        },
      },
    },

    settings: {
      "import/resolver": {
        node: {
          extensions: [".js", ".jsx"],
        },
      },

      react: {
        createClass: "createReactClass",
        pragma: "React",
        version: "16.4.2",
      },

      propWrapperFunctions: ["forbidExtraProps"],
    },

    rules: {
      semi: "error",
      "no-var": "error",
      quotes: ["warn", "double"],

      indent: [
        "warn",
        4,
        {
          SwitchCase: 1,
        },
      ],

      "no-console": "warn",
      "keyword-spacing": "warn",
      eqeqeq: "warn",

      "space-before-function-paren": [
        "warn",
        {
          anonymous: "always",
          named: "never",
        },
      ],

      "space-before-blocks": "warn",
      "no-multiple-empty-lines": "warn",
      "react/jsx-equals-spacing": ["warn", "never"],

      "id-match": [
        "error",
        "^([A-Za-z0-9_])+$",
        {
          properties: true,
        },
      ],

      "import/named": 2,
      "import/default": 2,
      "react/no-deprecated": "error",
      "no-dupe-class-members": "error",
    },
  },
]);
