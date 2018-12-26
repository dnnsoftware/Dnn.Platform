module.exports = {
    "plugins": [
        "react"
    ],
    "env": {
        "browser": true,
        "commonjs": true
    },
    "settings": {
        "react":{
            "version": "16"
        }
    },
    "extends": ["eslint:recommended", "plugin:react/recommended"],
    "parserOptions": {
        "ecmaFeatures": {
            "jsx": true
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
      "id-match": ["error", "^([A-Za-z0-9_])+$", {"properties": true}]
    }
};
