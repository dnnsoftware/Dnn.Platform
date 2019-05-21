const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
module.exports = {
    entry: "./src/main.jsx",
    optimization: {
        minimize: isProduction
    },
    output: {
        path: path.resolve("../../admin/personaBar/Dnn.Users/scripts/bundles/"),
        filename: "users-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8080/dist/"
    },
    devServer: {
        disableHostCheck: !isProduction
    },
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                enforce: "pre",
                exclude: [/node_modules/],
                loader: "eslint-loader",
                options: { fix: true } },
            {
                test: /\.(js|jsx)$/ ,
                exclude: [/node_modules/],
                use: {
                    loader:"babel-loader",
                    options: { 
                        presets: ["@babel/preset-env", "@babel/preset-react"], 
                        "plugins": [
                            "@babel/plugin-transform-react-jsx",
                            "@babel/plugin-proposal-object-rest-spread"
                        ] 
                    } 
                }
            },
            { test: /\.(less|css)$/, loader: ["style-loader","css-loader","less-loader"] },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
            { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
            { test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, loader: "url-loader?mimetype=application/font-woff" },
            { test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/, loader: "file-loader?name=[name].[ext]" },
        ]
    },
    resolve: {
        extensions: [".jsx", ".js", ".json"],
        modules: [
            path.resolve(__dirname, "./src"),
            path.resolve(__dirname, "./node_modules"),  // Try local node_modules
            path.resolve(__dirname, "./src/_exportables/src"),
            path.resolve(__dirname, "./src/_exportables/node_modules"), 
            path.resolve("../../../../../node_modules")   // Last fallback to workspaces node_modules
        ]
    },
    externals: Object.assign(webpackExternals, 
        {
            "dnn-users-common-action-types":"window.dnn.Users.CommonActionTypes",
            "dnn-users-common-components":"window.dnn.Users.CommonComponents",
            "dnn-users-common-reducers": "window.dnn.Users.CommonReducers",
            "dnn-users-common-actions":"window.dnn.Users.CommonActions"
        }),
    devtool: "source-map"
};