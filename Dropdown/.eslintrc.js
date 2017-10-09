module.exports = {
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
    "extends": ["eslint:recommended", "plugin:react/recommended", "eslint-config-dnn", "plugin:jest/recommended"],
    "parserOptions": {
        "ecmaFeatures": {
            "es6":true,
            "jsx": true,
            "experimentalObjectRestSpread": true
        },
        "ecmaVersion": 6,
        "sourceType": "module"
    },
    "globals": {
        "Promise": false,
    },
    "rules": {
       "spellcheck/spell-checker": [1,
        {
            "comments": "true",
            "strings": "true",
            "identifiers": "true",
            "skipWords": require("../.eslintskipwords"),
            "skipIfMatch": [
                "http://[^s]*",
                "https://[^s]*",
                "(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)" // CSS hex color
            ],
            "jest/no-disabled-tests": "warn",
            "jest/no-focused-tests": "error",
            "jest/no-identical-title": "error",
            "jest/valid-expect": "error"
        }
      ]
    }
};
