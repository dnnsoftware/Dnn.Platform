const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = require("dnn-webpack-externals");
module.exports = {
    entry: "./src/main.jsx",
    output: {
        path: "../admin/personaBar/scripts/bundles/",
        filename: "users-bundle.js",
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
            path.resolve('./src'),
            path.resolve('./exportables'),  // Look in exportables after
            path.resolve('./node_modules')
        ]
    },
    externals: Object.assign(webpackExternals, 
    {
        "dnn-users-common-action-types":"window.dnn.Users.CommonActionTypes",
        "dnn-users-common-components":"window.dnn.Users.CommonComponents",
        "dnn-users-common-reducers": "window.dnn.Users.CommonReducers",
        "dnn-users-common-actions":"window.dnn.Users.CommonActions"
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