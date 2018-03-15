const webpack = require("webpack");
const packageJson = require("./package.json");
const path = require("path");
const isProduction = process.env.NODE_ENV === "production";
const languages = {
    "en": null
    // TODO: create locallizaton files per language 
    // "de": require("./localizations/de.json"),
    // "es": require("./localizations/es.json"),
    // "fr": require("./localizations/fr.json"),
    // "it": require("./localizations/it.json"),
    // "nl": require("./localizations/nl.json")
};

const webpackExternals = Object.assign({},
    require("dnn-webpack-externals"), {
        "dnn-back-to": "window.dnn.nodeModules.PersonaBarComponents.BackTo"
    });

module.exports = {
    entry: "./src/main.jsx",
    output: {
        path: "../admin/personaBar/scripts/bundles/",
        filename: "site-settings-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8085/dist/"
    },

    module: {
        loaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
            { test: /\.(svg)$/, exclude: /node_modules/, loader: "raw-loader" }
        ],

        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" },
            { test: /\.js$/, loader: "source-map-loader" }
        ]
    },

    resolve: {
        extensions: ["", ".js", ".json", ".jsx"],
        root: [
            path.resolve('./src'),          // Look in src first
            path.resolve('./node_modules')  // Last fallback to node_modules
        ]
    },

    externals: webpackExternals,

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