const webpack = require("webpack");
const I18nPlugin = require("i18n-webpack-plugin");
const packageJson = require("./package.json");
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

module.exports = Object.keys(languages).map(function (language) {
    return {
        entry: "./src/main.jsx",
        output: {
            path: "./dist/",
            filename: "bundle-" + language + ".js",
            publicPath: isProduction ? "" : "http://localhost:8080/dist/"
        },

        module: {
            loaders: [
                { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
                { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
                { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
                { test: /\.css$/, loader: "style-loader!css-loader" },
                { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
                { test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, loader: "url-loader?mimetype=application/font-woff" },
                { test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/, loader: "file-loader?name=[name].[ext]" },
            ],

            preLoaders: [
                { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" }
            ]
        },

        resolve: {
            extensions: ["", ".js", ".json", ".jsx"]
        },

        externals: require("dnn-webpack-externals"),

        plugins: isProduction ? [
            new webpack.optimize.UglifyJsPlugin(),
            new webpack.optimize.DedupePlugin(),
            new I18nPlugin(languages[language]),
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version),
                "process.env": {
                    "NODE_ENV": JSON.stringify("production")
                }
            })
        ] : [
                new I18nPlugin(languages["en"]),
                new webpack.DefinePlugin({
                    VERSION: JSON.stringify(packageJson.version)
                })
            ]
    };
});