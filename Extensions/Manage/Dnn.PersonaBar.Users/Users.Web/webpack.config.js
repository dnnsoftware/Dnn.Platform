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
        path: path.resolve("../admin/personaBar/scripts/bundles/"),
        filename: "users-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8080/dist/"
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
                    loader:'babel-loader',
                    options: {
                        presets: ['@babel/react']
                    },
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
            path.resolve(__dirname, "./node_modules"),
            path.resolve(__dirname, "./src/_exportables/src"),
            path.resolve(__dirname, "./src/_exportables/node_modules"),
        ]
    },
    externals: Object.assign(webpackExternals, 
        {
            "dnn-users-common-action-types":"window.dnn.Users.CommonActionTypes",
            "dnn-users-common-components":"window.dnn.Users.CommonComponents",
            "dnn-users-common-reducers": "window.dnn.Users.CommonReducers",
            "dnn-users-common-actions":"window.dnn.Users.CommonActions"
        }),
    devtool: "inline-source-map",
    plugins: isProduction ? [
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