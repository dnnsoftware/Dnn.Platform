module.exports = {
    "parser": "babel-eslint",
    "plugins": [
        "react"
    ],
    "env": {
        "browser": true,
        "commonjs": true,
        "jest":true
    },
    "extends": ["eslint:recommended", "plugin:react/recommended"],
    "settings": {
        "react": {
            "version": "16"
        }
    },
    "parserOptions": {
        "ecmaFeatures": {
            "arrowFunctions": true,
            "blockBindings": true,
            "classes": true,
            "defaultParams": true,
            "destructuring": true,
            "forOf": true,
            "generators": true,
            "jsx": true,
            "modules": true,
            "objectLiteralComputedproperties": true,
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
        "VERSION": false,
        "process": false,
        "describe":false,
        "it" : false,
        "expect" :false
    },
    "rules": {            
      "semi": "error",
      "no-var": "error",
      "indent": ["warn", 4, {"SwitchCase": 1}],
      "no-unused-vars": "warn",
      "no-console": "warn",      
      "keyword-spacing": "warn", 
      "eqeqeq": ["warn", "smart"],
      "space-before-function-paren": ["warn", { "anonymous": "always", "named": "never" }],
      "space-before-blocks": "warn",
      "no-multiple-empty-lines":  "warn",
      "react/jsx-equals-spacing": ["warn", "never"],
      "id-match": ["error", "^([A-Za-z0-9_])+$", {"properties": true}]
    }
};
