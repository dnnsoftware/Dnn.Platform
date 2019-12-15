module.exports = {
    "plugins": [
        "react"
    ],
    "env": {
        "browser": true,
        "commonjs": true
    },
    "extends": ["eslint:recommended", "plugin:react/recommended"],
    "settings": {
        "react": {
          "version": "16"
        }
    },
    "parserOptions": {
        "ecmaFeatures": {
            "jsx": true,
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
        "__": false,
        "Promise": false,
        "VERSION": false
    },
    "rules": {
    //    "spellcheck/spell-checker": [1,
    //     {
    //         "comments": "true",
    //         "strings": "true",
    //         "identifiers": "true",
    //         "skipWords": require("./.eslintskipwords"),
    //         "skipIfMatch": [
    //             "http://[^s]*",
    //             "https://[^s]*",
    //             "(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)" // CSS hex color
    //         ]
    //     }
    //   ],      
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
      "no-useless-escape": "off"
    }
};
