const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = Object.assign({},
    require("dnn-webpack-externals"), {
        "dnn-back-to": "window.dnn.nodeModules.PersonaBarComponents.BackTo"
    });
const languages = {
    "en": null
    // TODO: create locallizaton files per language 
    // "de": require("./localizations/de.json"),
    // "es": require("./localizations/es.json"),
    // "fr": require("./localizations/fr.json"),
    // "it": require("./localizations/it.json"),
    // "nl": require("./localizations/nl.json")
};
const moduleName = "sites";
module.exports = {
    entry: "./src/main.jsx",
    output: {
        path: "../admin/personaBar/scripts/bundles/",
        filename: moduleName + "-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8080/dist/"
    },

    module: {
        loaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" }
        ],

        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" }
        ]
    },

    resolve: {
        extensions: ["", ".js", ".json", ".jsx"],
        root: [
            path.resolve('./src'),          // Look in src first
            path.resolve('./exportables'),  // Look in exportables after
            path.resolve('./node_modules')  // Last fallback to node_modules
        ]
    },
    externals: Object.assign(webpackExternals, {
        "dnn-sites-common-action-types": "window.dnn.Sites.CommonActionTypes",
        "dnn-sites-common-components": "window.dnn.Sites.CommonComponents",
        "dnn-sites-common-reducers": "window.dnn.Sites.CommonReducers",
        "dnn-sites-common-actions": "window.dnn.Sites.CommonActions"
    }),
    plugins: isProduction ? [
        new webpack.optimize.UglifyJsPlugin(),
        new webpack.optimize.DedupePlugin(),
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
            "process.env": {
                "NODE_ENV": JSON.stringify("production")
            }
        })
    ] : [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version)
            })
        ]
};