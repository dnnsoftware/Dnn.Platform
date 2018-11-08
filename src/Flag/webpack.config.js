const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const nodeExternals = require("webpack-node-externals");

module.exports = {
    entry: "./src/Flag.tsx",
    output: {
        path: path.resolve(__dirname, "lib"),
        filename: "Flag.js",
        libraryTarget: "umd",
        library: "Flag"
    },
    module: {
            rules: [
                { test: /\.(js|ts|tsx)$/, exclude: /node_modules/, loaders: [ "awesome-typescript-loader"] },
                { test: /\.png$/, loader: "url-loader?limit=8192" },
                { test: /\.(ts|tsx)$/, enforce: "pre", exclude: /node_modules/, loader: "tslint-loader" }
            ]
    },
    devtool: "inline-source-map",
    resolve: {
            extensions: [".tsx"]
        },
    target: "node", // in order to ignore built-in modules like path, fs, etc.
    externals: ["react", nodeExternals()], // in order to ignore all modules in node_modules folder
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