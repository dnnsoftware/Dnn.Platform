const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const nodeExternals = require('webpack-node-externals');
module.exports = {
    entry: "./src/Droppable.jsx",
    output: {
        path: __dirname + "/lib",
        filename: "Droppable.js",
        libraryTarget: "umd",
        library: "Droppable"
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader", options: { fix: true }},
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["babel-loader?presets[]=react"] }
        ]
    },
    target: 'node', // in order to ignore built-in modules like path, fs, etc.
    externals: ["react", nodeExternals()], // in order to ignore all modules in node_modules folder
    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            "node_modules",
            path.resolve(__dirname, "src")
        ]
    },
    devtool:"inline-sourcemap",
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