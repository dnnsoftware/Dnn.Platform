module.exports = {
    "plugins": [
        "react",
        "spellcheck",
        "import",
        "filenames",
        "babel"
    ],
    "settings": {
        "import/resolver":{
            "webpack": {},
            "node":{
                "extensions":[".js", ".jsx"]
            }
        }
    },
    "parser": "babel-eslint",
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
        "ecmaVersion": 6,
        "sourceType": "module"
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
        "import/no-unresolved": 2,
        "import/named": 2,
        "import/default": 2,
        "filenames/match-exported": 2,
        "filenames/no-index": 2
    }
};