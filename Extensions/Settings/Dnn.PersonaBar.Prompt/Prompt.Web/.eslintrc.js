module.exports = {
    "parser": "babel-eslint",
    "plugins": [
        "react",
        "spellcheck",
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
        "eslint-config-dnn",
        "plugin:jest/recommended"
    ],
    "settings": {
        "import/resolver":{
            "node":{
                "extensions": [".js", ".jsx"],
                "paths": ["../node_modules", "src"]
            }
        }
    },
    "parserOptions": {
        "ecmaFeatures": {
            "es6":true,
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
        "ecmaVersion": 6,
        "sourceType": "module"
    },
    "globals": {
        "Promise": false,
    },
    "rules": {
        "jest/no-disabled-tests": "warn",
        "jest/no-focused-tests": "error",
        "jest/no-identical-title": "error",
        "jest/valid-expect": "error",
        "spellcheck/spell-checker": [1,
        {
            "comments": true,
            "strings": true,
            "identifiers": true,
            "skipWords": require("./.eslintskipwords"),
            "skipIfMatch": [
                "http://[^s]*",
                "https://[^s]*",
                "util(s)?",
                "(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)" // CSS hex color
            ],
        }],
    }
};
