module.exports = {
    "plugins": [
        "react",
        "spellcheck"
    ],
    "env": {
        "browser": true,
        "commonjs": true
    },
    "extends": ["eslint:recommended", "plugin:react/recommended", "eslint-config-dnn"],
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
            "skipWords": require("./.eslintskipwords"),
            "skipIfMatch": [
                "http://[^s]*",
                "https://[^s]*",
                "(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)" // CSS hex color
            ]
        }
      ]
    }
};
