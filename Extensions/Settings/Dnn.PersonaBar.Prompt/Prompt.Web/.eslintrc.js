module.exports = {
    "parser": "babel-eslint",
    "plugins": [
        "react",
        "jest"
    ],
    "env": {
        "browser": true,
        "commonjs": true,
        "jest/globals": true
    },
    "extends": [
        "eslint:recommended",
        "plugin:react/recommended",
        "plugin:jest/recommended"
    ],
    "settings": {
        "react": {
            "version": "16"
        },
        "import/resolver":{
            "node":{
                "extensions": [".js", ".jsx"],
                "paths": ["../node_modules", "src"]
            }
        }
    },
    "parserOptions": {
        "ecmaFeatures": {
            "jsx": true,
            "experimentalObjectRestSpread": true,
            "arrowFunctions": true,
            "blockBindings": true,
            "classes": true,
            "defaultParams": true,
            "destructuring": true,
            "forOf": true,
            "generators": true,
            "modules": true,
            "objectLiteralComputedProperties": true,
            "regexUFlag": true,
            "regexYFlag": true,
            "spread": true,
            "superInFunctions": false,
            "templateStrings": true
        },
        "ecmaVersion": 2018,
        "sourceType": "module"
    },
    "globals": {
        "Promise": false,
    },
    "rules": {
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
        "id-match": ["error", "^([A-Za-z0-9_])+$", {"properties": true}],
        "no-useless-escape": "off",
        "jest/no-disabled-tests": "warn",
        "jest/no-focused-tests": "error",
        "jest/no-identical-title": "error",
        "jest/valid-expect": "error"       
    }
};
