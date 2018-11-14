const webpack = require("webpack");
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

const sourceMapLoader = { test: /\.js$/, loader: "source-map-loader", enforce: "pre" };
const eslintLoader = { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" };

const preLoaders = isProduction === true ? [eslintLoader] : [eslintLoader, sourceMapLoader];

module.exports = {
    entry: "./src/main.jsx",
    output: {
        path: "./../scripts/exports/",
        filename: "export-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8070/dist/"
    },

    module: {
        loaders:[{ test: /\.(js|jsx)$/ , exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
                { test: /\.(less|css)$/, loader: "style-loader!css-loader!less-loader" },
                { test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, loader: "url-loader?mimetype=application/font-woff" },
                { test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/, loader: "file-loader?name=[name].[ext]" },
                { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
				{ test: /\.json$/, include: /node_modules/, loader: "json-loader" }],
        preLoaders
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"]
    },
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

if(!isProduction) {
    module.exports.devtool = "source-map";
    module.exports.debug = true;
}