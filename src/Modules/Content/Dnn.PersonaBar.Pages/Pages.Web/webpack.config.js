const webpack = require("webpack");
const packageJson = require("./package.json");
const path = require("path");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = require("dnn-webpack-externals");
module.exports = {
    entry: "./src/main.jsx",
    output: {
        path: "../admin/personaBar/scripts/bundles/",
        filename: "pages-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8090/dist/"
    },

    module: {
        loaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.css$/, loader: "style-loader!css-loader" },
            { test: /\.(ttf|woff|gif|png)$/, loader: "url-loader?limit=8192" }
        ],

        preLoaders: [
           { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" }
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