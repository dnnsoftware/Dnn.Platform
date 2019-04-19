const webpack = require("webpack");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const path = require("path");
const languages = {
    "en": null
    // TODO: create locallizaton files per language 
    // "de": require("./localizations/de.json"),
    // "es": require("./localizations/es.json"),
    // "fr": require("./localizations/fr.json"),
    // "it": require("./localizations/it.json"),
    // "nl": require("./localizations/nl.json")
};

module.exports = {
    entry: "./src/main.jsx",
    optimization: {
        minimize: isProduction
    },
    output: {
        path: path.resolve("../../admin/personaBar/Dnn.AdminLogs/scripts/bundles/"),
        filename: "adminLogs-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8080/dist/"
    },
    devServer: {
        disableHostCheck: !isProduction
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader", options: { fix: true } },
            { test: /\.(js|jsx)$/ , exclude: /node_modules/, loader: "babel-loader" },
            { test: /\.(less|css)$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
        ]
    },

    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            path.resolve("./src"),          // Look in src first
            path.resolve("./node_modules"),  // Try local node_modules
            path.resolve('../node_modules')   // Last fallback to workspaces node_modules
        ]
    },

    externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),

    plugins:
    isProduction
        ?
        [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version),
                "process.env": {
                    "NODE_ENV": JSON.stringify("production")
                }
            })
        ]
        :
        [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version)
            })
        ]
};