const pkg = require('./package.json')

const reactVersion = () => {
  if (pkg.dependencies && pkg.dependencies.react) {
    return { version: pkg.dependencies.react.replace(/[^0-9.]/g, '') }
  }
  if (pkg.devDependencies && pkg.devDependencies.react) {
    return { version: pkg.devDependencies.react.replace(/[^0-9.]/g, '') }
  }
}

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
          ...reactVersion()
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
